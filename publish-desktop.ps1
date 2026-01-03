param (
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

# Check for vpk
if (-not (Get-Command vpk -ErrorAction SilentlyContinue)) {
    Write-Host "Velopack CLI (vpk) not found. Installing..."
    dotnet tool install -g vpk
}

$Project = "OCC.Client\OCC.Client.Desktop\OCC.Client.Desktop.csproj"
$PublishDir = ".\publish"
$ReleaseDir = ".\Releases"

Write-Host "Building and Publishing version $Version..."

# Clean publish dir
if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }

# Dotnet publish
dotnet publish $Project -c Release -o $PublishDir /p:Version=$Version

Write-Host "Packing release..."

# Create Releases dir if not exists
if (-not (Test-Path $ReleaseDir)) { New-Item -ItemType Directory -Path $ReleaseDir }

# Run vpk
# -u : Package ID
# -v : Version
# -p : Pack directory (where the files are)
# -e : Main executable name
# --releaseDir : Output directory (defaults to Releases)

vpk pack -u "OCC.Client" -v $Version -p $PublishDir -e "OCC.Client.Desktop.exe" --releaseDir $ReleaseDir

Write-Host "--------------------------------------------------------"
Write-Host "Release created in $ReleaseDir"
Write-Host "To enable updates:"
Write-Host "1. Create a public folder or GitHub Release."
Write-Host "2. Upload all files from $ReleaseDir (Setup.exe, RELEASES, .nupkg) to that location."
Write-Host "3. Ensure UpdateService.cs points to that URL."
Write-Host "--------------------------------------------------------"
