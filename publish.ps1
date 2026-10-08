# Builds the release artefact: a single self-contained BlindSpot.exe that needs no
# .NET install. Run from the project root.
#
#   .\publish.ps1
#
# Output: bin\publish\BlindSpot.exe

param(
    [switch]$Open    # open the output folder when done
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Building single-file self-contained release..." -ForegroundColor Cyan
Push-Location $root
try {
    dotnet publish -c Release -p:PublishProfile=ReleaseSingleFile --nologo
    if ($LASTEXITCODE -ne 0) { throw "publish failed (exit $LASTEXITCODE)" }
}
finally { Pop-Location }

$exe = Join-Path $root "bin\publish\BlindSpot.exe"
if (-not (Test-Path $exe)) { throw "expected output not found: $exe" }

$size = (Get-Item $exe).Length / 1MB
Write-Host ""
Write-Host ("Done: {0}  ({1:N1} MB)" -f $exe, $size) -ForegroundColor Green
Write-Host "This exe contains the .NET runtime - no install required on the target machine." -ForegroundColor Green

if ($Open) { Start-Process "explorer.exe" (Join-Path $root "bin\publish") }
