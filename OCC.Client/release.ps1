$ErrorActionPreference = "Stop"

$version = "1.0.0"
if ($args.Count -gt 0) {
    $version = $args[0]
}

Write-Host "Building OCC.Client for version $version..." -ForegroundColor Client

# 1. Clean
Write-Host "Cleaning..."
dotnet clean OCC.Client.Desktop/OCC.Client.Desktop.csproj
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# 2. Publish
Write-Host "Publishing..."
dotnet publish OCC.Client.Desktop/OCC.Client.Desktop.csproj -c Release -o publish
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# 3. Pack with Velopack
# Note: Ensure 'vpk' tool is installed: dotnet tool install -g vpk
Write-Host "Packing with Velopack..."

# We need to ensure the ID of the app matches what Velopack expects.
# The executable is "OCC.Client.Desktop.exe"
# We'll call the release "OrangeCircleConstruction"
vpk pack -u "OrangeCircleConstruction" -v $version -p publish -e "OCC.Client.Desktop.exe"

if ($LASTEXITCODE -ne 0) { 
    Write-Host "Packing Failed!" -ForegroundColor Red
    exit $LASTEXITCODE 
}

# Rename Setup.exe to something nicer
if (Test-Path "Releases\Setup.exe") {
    Rename-Item "Releases\Setup.exe" "OrangeCircleSetup.exe" -Force
}

Write-Host "Done! Release files are in the 'Releases' folder." -ForegroundColor Green
