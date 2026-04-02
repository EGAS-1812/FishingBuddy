Param(
    [Parameter(ValueFromPipeline = $true)]
    [string]$InputJson
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$logDirectory = Join-Path $repoRoot "lab-1"
$logFile = Join-Path $logDirectory "agent_log.txt"

if (-not (Test-Path -Path $logDirectory)) {
    New-Item -Path $logDirectory -ItemType Directory -Force | Out-Null
}

if (-not (Test-Path -Path $logFile)) {
    New-Item -Path $logFile -ItemType File -Force | Out-Null
}

if (-not $InputJson) {
    $InputJson = [Console]::In.ReadToEnd()
}

$timestamp = Get-Date -Format "u"
$prompt = $null

try {
    $payload = $InputJson | ConvertFrom-Json -ErrorAction Stop
    if ($null -ne $payload.prompt) {
        $prompt = [string]$payload.prompt
    }
} catch {
    # If the payload is not JSON, log the raw content.
}

if ([string]::IsNullOrWhiteSpace($prompt)) {
    $prompt = $InputJson.Trim()
}

Add-Content -Path $logFile -Value "[$timestamp] $prompt"
