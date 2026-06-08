$exitCode = 0

try {
    $ErrorActionPreference = "Stop"

    $scriptDir = $PSScriptRoot
    $repoRoot = Split-Path -Parent $scriptDir
    $projectPath = Join-Path $repoRoot "SnjVoiceChanger\SnjVoiceChanger.csproj"
    $publishDir = Join-Path $scriptDir "app"
    $nativeHostPath = Join-Path $repoRoot "SnjVoiceChanger\bin\Release\net9.0-windows\SnjVstHostNative.dll"

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

    if (-not (Test-Path -LiteralPath $nativeHostPath)) {
        throw "Native VST host was not found: $nativeHostPath. Build the solution in Visual Studio Release x64 first."
    }

    Copy-Item -LiteralPath $nativeHostPath -Destination $publishDir -Force
    Write-Host "Copied native VST host: SnjVstHostNative.dll"

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
