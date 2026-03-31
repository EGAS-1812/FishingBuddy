Param(
    [Parameter(ValueFromPipeline = $true)]
    [string]$InputJson
)

$logFile = Join-Path $PSScriptRoot "agent_log.txt"

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
