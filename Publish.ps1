<#
.SYNOPSIS
    Publishes PanoramicData.SshServer to NuGet.org

.DESCRIPTION
    This script performs the following steps:
    1. Checks for clean git working directory (porcelain)
    2. Determines the Nerdbank git version
    3. Checks that nuget-key.txt exists, has content, and is gitignored
    4. Runs unit tests (unless -SkipTests is specified)
    5. Publishes to NuGet.org

.PARAMETER SkipTests
    Skips running unit tests

.EXAMPLE
    .\Publish.ps1
    .\Publish.ps1 -SkipTests
#>

param(
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$InformationPreference = 'Continue'

# Step 1: Check for clean git working directory
Write-Information "Checking for clean git working directory..."
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Error "Git working directory is not clean. Please commit or stash your changes."
    exit 1
}
Write-Information "Git working directory is clean."

# Step 2: Determine the Nerdbank git version
Write-Information "Determining Nerdbank git version..."
$version = nbgv get-version -v NuGetPackageVersion
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to determine Nerdbank git version. Ensure nbgv is installed (dotnet tool install -g nbgv)."
    exit 1
}
Write-Information "Version: $version"

# Step 3: Check that nuget-key.txt exists, has content, and is gitignored
Write-Information "Checking nuget-key.txt..."
$nugetKeyPath = Join-Path $PSScriptRoot "nuget-key.txt"

if (-not (Test-Path $nugetKeyPath)) {
    Write-Error "nuget-key.txt does not exist. Create it with your NuGet API key."
    exit 1
}

$nugetKey = Get-Content $nugetKeyPath -Raw
if ([string]::IsNullOrWhiteSpace($nugetKey)) {
    Write-Error "nuget-key.txt is empty. Add your NuGet API key."
    exit 1
}
$nugetKey = $nugetKey.Trim()

# Check if nuget-key.txt is gitignored
$gitCheckIgnore = git check-ignore -q "nuget-key.txt" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "nuget-key.txt is not gitignored. Add it to .gitignore to protect your API key."
    exit 1
}
Write-Information "nuget-key.txt is valid and gitignored."

# Step 4: Run unit tests (unless -SkipTests is specified)
if (-not $SkipTests) {
    Write-Information "Running unit tests..."
    dotnet test
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Unit tests failed."
        exit 1
    }
    Write-Information "Unit tests passed."
} else {
    Write-Information "Skipping unit tests."
}

# Step 5: Publish to NuGet.org
Write-Information "Building and packing..."
dotnet build "PanoramicData.SshServer\PanoramicData.SshServer.csproj" -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build the project."
    exit 1
}

dotnet pack "PanoramicData.SshServer\PanoramicData.SshServer.csproj" -c Release --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to pack the project."
    exit 1
}

$nupkgPath = Get-ChildItem -Path "PanoramicData.SshServer\bin\Release" -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $nupkgPath) {
    Write-Error "Could not find .nupkg file."
    exit 1
}

Write-Information "Publishing $($nupkgPath.Name) to NuGet.org..."
dotnet nuget push $nupkgPath.FullName --api-key $nugetKey --source https://api.nuget.org/v3/index.json
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to publish to NuGet.org."
    exit 1
}

Write-Information "Successfully published $($nupkgPath.Name) to NuGet.org!"
exit 0
