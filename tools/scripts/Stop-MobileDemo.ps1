param(
    [switch]$IncludeDotnet
)

$ErrorActionPreference = "Continue"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$statePath = Join-Path $scriptRoot "mobile-demo-state.json"
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).Path
$urlFilePath = Join-Path $repoRoot "URLMOBILE.txt"

function Stop-ById([int]$pid) {
    try {
        $proc = Get-Process -Id $pid -ErrorAction Stop
        Stop-Process -Id $pid -Force -ErrorAction Stop
        Write-Host "Stopped $($proc.ProcessName) (PID $pid)."
    }
    catch {
        Write-Verbose "PID $pid not running."
    }
}

if (Test-Path $statePath) {
    try {
        $state = Get-Content $statePath -Raw | ConvertFrom-Json
        if ($state.appPid) { Stop-ById -pid ([int]$state.appPid) }
        if ($state.tunnelPid) { Stop-ById -pid ([int]$state.tunnelPid) }
    }
    catch {
        Write-Warning "Could not read state file: $($_.Exception.Message)"
    }
}

$names = @("FishingBuddy", "cloudflared")
if ($IncludeDotnet) {
    $names += "dotnet"
}

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

if (Test-Path $statePath) {
    Remove-Item $statePath -Force
    Write-Host "Removed state file."
}

Set-Content -Path $urlFilePath -Value "mobilelink: (stopped)" -Encoding UTF8
Write-Host "Updated URL file."

Write-Host "Mobile demo processes stopped."
