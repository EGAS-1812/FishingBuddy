param(
    [string]$AppUrl = "http://localhost:5142",
    [string]$LaunchProfile = "https",
    [string]$CloudflaredPath = "tools/cloudflared/cloudflared.exe",
    [switch]$Restart
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).Path
$statePath = Join-Path $scriptRoot "mobile-demo-state.json"
$urlFilePath = Join-Path $repoRoot "URLMOBILE.txt"
$appOutLogPath = Join-Path $scriptRoot "mobile-demo-app.out.log"
$appErrLogPath = Join-Path $scriptRoot "mobile-demo-app.err.log"
$tunnelOutLogPath = Join-Path $scriptRoot "mobile-demo-tunnel.out.log"
$tunnelErrLogPath = Join-Path $scriptRoot "mobile-demo-tunnel.err.log"

function Stop-ExistingProcesses {
    $names = @("FishingBuddy", "cloudflared")
    foreach ($name in $names) {
        $procs = Get-Process -Name $name -ErrorAction SilentlyContinue
        foreach ($proc in $procs) {
            try {
                Stop-Process -Id $proc.Id -Force -ErrorAction Stop
                Write-Host "Stopped $($proc.ProcessName) (PID $($proc.Id))."
            }
            catch {
                Write-Warning "Could not stop PID $($proc.Id): $($_.Exception.Message)"
            }
        }
    }
}

if ($Restart) {
    Stop-ExistingProcesses
}

if (-not (Test-Path (Join-Path $repoRoot $CloudflaredPath))) {
    throw "cloudflared not found at '$CloudflaredPath'."
}

if (Test-Path $appOutLogPath) { Remove-Item $appOutLogPath -Force }
if (Test-Path $appErrLogPath) { Remove-Item $appErrLogPath -Force }
if (Test-Path $tunnelOutLogPath) { Remove-Item $tunnelOutLogPath -Force }
if (Test-Path $tunnelErrLogPath) { Remove-Item $tunnelErrLogPath -Force }
if (Test-Path $urlFilePath) {
    Set-Content -Path $urlFilePath -Value "mobilelink: (starting...)" -Encoding UTF8
}

$appProc = Start-Process -FilePath "dotnet" `
    -ArgumentList @("run", "--project", "FishingBuddy.csproj", "--launch-profile", $LaunchProfile) `
    -WorkingDirectory $repoRoot `
    -RedirectStandardOutput $appOutLogPath `
    -RedirectStandardError $appErrLogPath `
    -PassThru

Write-Host "Started app (PID $($appProc.Id)). Waiting for $AppUrl ..."

$ready = $false
for ($i = 0; $i -lt 60; $i++) {
    try {
        $response = Invoke-WebRequest -Uri $AppUrl -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
            $ready = $true
            break
        }
    }
    catch {
        if ($appProc.HasExited) {
            throw "App process exited early. Check $appOutLogPath and $appErrLogPath"
        }
    }

    [System.Threading.Thread]::Sleep(500)
}

if (-not $ready) {
    throw "App did not become reachable at $AppUrl. Check $appOutLogPath and $appErrLogPath"
}

Write-Host "App is reachable. Starting tunnel..."

$tunnelProc = Start-Process -FilePath (Join-Path $repoRoot $CloudflaredPath) `
    -ArgumentList @("tunnel", "--url", $AppUrl, "--no-autoupdate") `
    -WorkingDirectory $repoRoot `
    -RedirectStandardOutput $tunnelOutLogPath `
    -RedirectStandardError $tunnelErrLogPath `
    -PassThru

$url = $null
for ($i = 0; $i -lt 60; $i++) {
    if ($tunnelProc.HasExited) {
        throw "cloudflared exited early. Check $tunnelLogPath"
    }

    if ((Test-Path $tunnelOutLogPath) -or (Test-Path $tunnelErrLogPath)) {
        $outContent = if (Test-Path $tunnelOutLogPath) { (Get-Content $tunnelOutLogPath -Raw) } else { "" }
        $errContent = if (Test-Path $tunnelErrLogPath) { (Get-Content $tunnelErrLogPath -Raw) } else { "" }
        $content = "{0}`n{1}" -f ([string]$outContent), ([string]$errContent)
        $match = [regex]::Match($content, "https://[a-zA-Z0-9\-]+\.trycloudflare\.com")
        if ($match.Success) {
            $url = $match.Value
            break
        }
    }

    [System.Threading.Thread]::Sleep(500)
}

if (-not $url) {
    throw "Could not find quick tunnel URL in $tunnelOutLogPath"
}

Set-Content -Path $urlFilePath -Value ("mobilelink: {0}" -f $url) -Encoding UTF8

$state = [ordered]@{
    startedAt = (Get-Date).ToString("o")
    launchProfile = $LaunchProfile
    appUrl = $AppUrl
    publicUrl = $url
    publicUrlFile = $urlFilePath
    appPid = $appProc.Id
    tunnelPid = $tunnelProc.Id
    appOutLog = $appOutLogPath
    appErrLog = $appErrLogPath
    tunnelOutLog = $tunnelOutLogPath
    tunnelErrLog = $tunnelErrLogPath
}

$state | ConvertTo-Json | Set-Content -Path $statePath -Encoding UTF8

Write-Host ""
Write-Host "Mobile demo is ready:"
Write-Host "Public URL: $url"
Write-Host "Saved to: $urlFilePath"
Write-Host "App PID: $($appProc.Id)"
Write-Host "Tunnel PID: $($tunnelProc.Id)"
Write-Host "State file: $statePath"
Write-Host ""
Write-Host "To stop everything later:"
Write-Host "powershell -ExecutionPolicy Bypass -File tools/scripts/Stop-MobileDemo.ps1"
