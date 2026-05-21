param(
    [Parameter(Mandatory = $true)][string]$AppSettingsPath,
    [Parameter(Mandatory = $true)][string]$RunnerOutputPath
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $AppSettingsPath)) {
    throw "appsettings.json not found: $AppSettingsPath"
}

$app = Get-Content $AppSettingsPath -Raw | ConvertFrom-Json
$parallel = [bool]$app.EnableParallelExecution
$maxBrowsers = 1

if ($null -ne $app.MaxParallelBrowsers) {
    $maxBrowsers = [Math]::Max(1, [int]$app.MaxParallelBrowsers)
}

$maxThreads = if ($parallel) { $maxBrowsers } else { 1 }

$runner = [ordered]@{
    '$schema'                   = 'https://xunit.net/schema/v2.3/xunit.runner.schema.json'
    parallelizeAssembly         = $parallel
    parallelizeTestCollections  = $parallel
    maxParallelThreads          = $maxThreads
}

$dir = Split-Path $RunnerOutputPath -Parent
if ($dir -and -not (Test-Path $dir)) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
}

$runner | ConvertTo-Json -Depth 5 | Set-Content -Path $RunnerOutputPath -Encoding UTF8
Write-Host "Synced xunit.runner.json: parallel=$parallel maxParallelThreads=$maxThreads -> $RunnerOutputPath"
