$exitCode = 0

function Resolve-MSBuildPath {
    $knownPaths = @(
        "MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )

    foreach ($path in $knownPaths) {
        if ([string]::IsNullOrWhiteSpace($path)) {
            continue
        }

        $command = Get-Command $path -ErrorAction SilentlyContinue
        if ($command) {
            return $command.Source
        }

        if (Test-Path -LiteralPath $path) {
            return $path
        }
    }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path -LiteralPath $vswhere) {
        $candidate = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\Current\Bin\MSBuild.exe" | Select-Object -First 1
        if (-not [string]::IsNullOrWhiteSpace($candidate) -and (Test-Path -LiteralPath $candidate)) {
            return $candidate
        }
    }

    return $null
}

function Build-NativeProject([string]$msbuildPath, [string]$projectPath, [string]$projectName) {
    Write-Host "Building native host: $projectName..."
    & $msbuildPath $projectPath /m /p:Configuration=Release /p:Platform=x64
    if ($LASTEXITCODE -ne 0) {
        throw "$projectName build failed with code $LASTEXITCODE."
    }
}

try {
    $ErrorActionPreference = "Stop"

    $scriptDir = $PSScriptRoot
    $repoRoot = Split-Path -Parent $scriptDir
    $projectPath = Join-Path $repoRoot "SnjVoiceChanger\SnjVoiceChanger.csproj"
    $nativeVst3ProjectPath = Join-Path $repoRoot "SnjVstHostNative\SnjVstHostNative.vcxproj"
    $nativeVst2ProjectPath = Join-Path $repoRoot "SnjVst2HostNative\SnjVst2HostNative.vcxproj"
    $publishDir = Join-Path $scriptDir "app"
    $nativeHostPath = Join-Path $repoRoot "SnjVoiceChanger\bin\Release\net9.0-windows\SnjVstHostNative.dll"
    $nativeVst2HostPath = Join-Path $repoRoot "SnjVoiceChanger\bin\Release\net9.0-windows\SnjVst2HostNative.dll"

    Write-Host "Publishing Snj Voice Changer v1.2 self-contained..." -ForegroundColor Cyan
    Write-Host "Project: $projectPath"
    Write-Host "Output:  $publishDir"

    $msbuild = Resolve-MSBuildPath
    if (-not $msbuild) {
        throw "MSBuild.exe was not found. Install Visual Studio 2022 C++ build tools or build native projects in Visual Studio Release x64 first."
    }

    Write-Host "MSBuild: $msbuild"
    Build-NativeProject $msbuild $nativeVst3ProjectPath "SnjVstHostNative"
    Build-NativeProject $msbuild $nativeVst2ProjectPath "SnjVst2HostNative"

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

    if (-not (Test-Path -LiteralPath $nativeVst2HostPath)) {
        throw "Native VST2 host was not found: $nativeVst2HostPath. Build the solution in Visual Studio Release x64 first."
    }

    Copy-Item -LiteralPath $nativeVst2HostPath -Destination $publishDir -Force
    Write-Host "Copied native VST2 host: SnjVst2HostNative.dll"

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
