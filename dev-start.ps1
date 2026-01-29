<#
.SYNOPSIS
    Orchestrates the build and execution of the WebGL frontend and .NET backend.

.DESCRIPTION
    1. Checks if a Unity WebGL build exists. If not (or if -Force is used), runs the build via Unity CLI.
    2. Starts the Backend server (dotnet run) on port 5024 in a new window.
    3. Starts the Frontend server (python http.server) on port 8000 in a new window.

.PARAMETER Force
    Forces a rebuild of the WebGL client even if the Build folder already exists.

.EXAMPLE
    .\dev-start.ps1
    Starts the servers, building only if necessary.

.EXAMPLE
    .\dev-start.ps1 -Force
    Rebuilds the client and then starts the servers.
#>

[CmdletBinding()]
param (
    [switch]$Force
)

# Configuration
$BackendPort = 5024
$FrontendPort = 8000
$BuildDir = Join-Path $PSScriptRoot "Build"
$LogFile = Join-Path $PSScriptRoot "build.log"
$ProjectRoot = $PSScriptRoot

# --- Unity Path Detection ---
# Try to find the specific version first, then fallback to any Unity.exe in Hub
$UnityHubPath = "C:\Program Files\Unity\Hub\Editor"
$TargetVersion = "6000.3.6f1" # User specified version (Unity 6)

$UnityPath = ""

if (Test-Path "$UnityHubPath\$TargetVersion\Editor\Unity.exe") {
    $UnityPath = "$UnityHubPath\$TargetVersion\Editor\Unity.exe"
}
else {
    # Try to find any Unity.exe in the Hub folder
    $PotentialPaths = Get-ChildItem -Path $UnityHubPath -Filter "Unity.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName
    if ($PotentialPaths) {
        # Pick the last one (usually latest version if sorted by name/date, but simple pick here)
        $UnityPath = $PotentialPaths | Select-Object -Last 1
    }
}

# Allow manual override if detection failed or is wrong
# $UnityPath = "C:\Path\To\Your\Unity.exe"

if (-not $UnityPath -or -not (Test-Path $UnityPath)) {
    Write-Error "Unity executable not found. Please edit the `$UnityPath variable in this script."
    exit 1
}

Write-Host "Using Unity at: $UnityPath" -ForegroundColor Cyan

# --- Step 1: Build ---
$BuildExists = Test-Path (Join-Path $BuildDir "index.html")

if ($Force -or -not $BuildExists) {
    Write-Host "Starting WebGL Build... (This may take a while, check $LogFile for details)" -ForegroundColor Yellow

    # Ensure build directory is clean if forcing? Unity usually handles overwrite, but let's just run it.

    $BuildArgs = @(
        "-quit",
        "-batchmode",
        "-projectPath", "$ProjectRoot",
        "-executeMethod", "BuildScript.PerformWebGLBuild",
        "-logFile", "$LogFile"
    )

    $Process = Start-Process -FilePath $UnityPath -ArgumentList $BuildArgs -Wait -PassThru -NoNewWindow

    if ($Process.ExitCode -ne 0) {
        Write-Error "Build failed with exit code $($Process.ExitCode). Check $LogFile for details."
        exit $Process.ExitCode
    }

    Write-Host "Build completed successfully." -ForegroundColor Green
}
else {
    Write-Host "Build found at $BuildDir. Skipping build step." -ForegroundColor Green
}

# --- Step 2: Backend ---
Write-Host "Starting Backend Server on port $BackendPort..." -ForegroundColor Cyan
$BackendScriptBlock = {
    param($Root, $Port)
    Set-Location "$Root/Server/Game.Server"
    Write-Host "Launching Backend..."
    dotnet run --urls "http://localhost:$Port"
    Read-Host "Press Enter to exit..."
}

Start-Process powershell -ArgumentList "-NoExit", "-Command", "& {$BackendScriptBlock} -Root '$ProjectRoot' -Port $BackendPort"

# --- Step 3: Frontend ---
Write-Host "Starting Frontend Server on port $FrontendPort..." -ForegroundColor Cyan
$FrontendScriptBlock = {
    param($Path, $Port)
    Set-Location $Path
    Write-Host "Launching Frontend Server..."
    python -m http.server $Port
    Read-Host "Press Enter to exit..."
}

if (Test-Path $BuildDir) {
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "& {$FrontendScriptBlock} -Path '$BuildDir' -Port $FrontendPort"
}
else {
    Write-Error "Build directory not found! Cannot start frontend."
}

Write-Host "Development environment started." -ForegroundColor Green
