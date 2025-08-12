# Modern deployment script for FCInvoice
# Usage: .\publish.ps1 [-Configuration Release] [-Runtime win-x64] [-SelfContained]

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained = $false
)

$ProjectPath = "FCInvoice.UI\FCInvoice.UI.csproj"
$OutputPath = "publish\$Runtime"

Write-Host "Publishing FCInvoice..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "Runtime: $Runtime" -ForegroundColor Cyan
Write-Host "Self-contained: $SelfContained" -ForegroundColor Cyan

# Clean previous publish
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
    Write-Host "Cleaned previous publish directory" -ForegroundColor Yellow
}

# Build arguments
$buildArgs = @(
    "publish"
    $ProjectPath
    "--configuration", $Configuration
    "--runtime", $Runtime
    "--output", $OutputPath
    "--verbosity", "minimal"
)

if ($SelfContained) {
    $buildArgs += "--self-contained"
    Write-Host "Creating self-contained deployment..." -ForegroundColor Cyan
} else {
    $buildArgs += "--no-self-contained"
    Write-Host "Creating framework-dependent deployment..." -ForegroundColor Cyan
}

# Execute publish
try {
    & dotnet @buildArgs
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Publish completed successfully!" -ForegroundColor Green
        Write-Host "Output: $((Get-Item $OutputPath).FullName)" -ForegroundColor Green
    } else {
        Write-Host "❌ Publish failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} catch {
    Write-Host "❌ Publish failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}