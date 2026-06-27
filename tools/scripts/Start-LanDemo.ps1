param(
    [int]$Port = 5142,
    [switch]$Restart
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).Path
$appUrl = "http://0.0.0.0:$Port"
$statePath = Join-Path $scriptRoot "lan-demo-state.json"
$appOutLogPath = Join-Path $scriptRoot "lan-demo-app.out.log"
$appErrLogPath = Join-Path $scriptRoot "lan-demo-app.err.log"

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

if (Test-Path $appOutLogPath) { Remove-Item $appOutLogPath -Force }
if (Test-Path $appErrLogPath) { Remove-Item $appErrLogPath -Force }

$appProc = Start-Process -FilePath "dotnet" `
    -ArgumentList @("run", "--project", "FishingBuddy.csproj", "--urls", $appUrl) `
    -WorkingDirectory $repoRoot `
    -RedirectStandardOutput $appOutLogPath `
    -RedirectStandardError $appErrLogPath `
    -PassThru

Write-Host "Started LAN demo app (PID $($appProc.Id)). Waiting for port $Port ..."

$ready = $false
for ($i = 0; $i -lt 60; $i++) {
    try {
        $portCheck = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue
        if ($portCheck.TcpTestSucceeded) {
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
    throw "App did not start listening on port $Port. Check $appOutLogPath and $appErrLogPath"
}

$addresses = Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object {
        $_.IPAddress -notlike '127.*' -and
        $_.PrefixOrigin -ne 'WellKnown' -and
        $_.IPAddress -notlike '169.254.*'
    } |
    Sort-Object InterfaceMetric, SkipAsSource |
    Select-Object -ExpandProperty IPAddress -Unique

$urls = @($addresses | ForEach-Object { "http://${_}:$Port" })

$state = [ordered]@{
    startedAt = (Get-Date).ToString("o")
    appUrl = $appUrl
    appPid = $appProc.Id
    lanUrls = $urls
    appOutLog = $appOutLogPath
    appErrLog = $appErrLogPath
}

$state | ConvertTo-Json | Set-Content -Path $statePath -Encoding UTF8

Write-Host ""
Write-Host "LAN demo is ready. Open one of these on your phone while it is on the same Wi-Fi:"
foreach ($url in $urls) {
    Write-Host $url
}
Write-Host ""
Write-Host "State file: $statePath"
Write-Host "To stop later:"
Write-Host "powershell -ExecutionPolicy Bypass -File tools/scripts/Stop-MobileDemo.ps1"
