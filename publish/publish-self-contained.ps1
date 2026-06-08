$exitCode = 0

try {
    $ErrorActionPreference = "Stop"

    $scriptDir = $PSScriptRoot
    $repoRoot = Split-Path -Parent $scriptDir
    $projectPath = Join-Path $repoRoot "SnjVoiceChanger\SnjVoiceChanger.csproj"
    $publishDir = Join-Path $scriptDir "app"

    Write-Host "Publishing Snj Voice Changer v1.1 self-contained..." -ForegroundColor Cyan
    Write-Host "Project: $projectPath"
    Write-Host "Output:  $publishDir"

    if (Test-Path -LiteralPath $publishDir) {
        Write-Host "Cleaning previous publish output..."
        Remove-Item -LiteralPath $publishDir -Recurse -Force
    }

    dotnet publish $projectPath `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=false `
        -o $publishDir

    Write-Host ""
    Write-Host "Publish completed successfully." -ForegroundColor Green
    Write-Host "Next: run publish\build-installer.ps1"
}
catch {
    $exitCode = 1
    Write-Host ""
    Write-Host "Publish failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
finally {
    Write-Host ""
    Read-Host "Press Enter to close"
}

if ($exitCode -ne 0) {
    exit $exitCode
}
