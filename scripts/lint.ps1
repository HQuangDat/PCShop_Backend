param()

$project = "PCShop_Backend.csproj"
$failed = $false

Write-Host ""
Write-Host "=== [1/3] Build + Analyzer Warnings ===" -ForegroundColor Cyan
$build = dotnet build $project 2>&1
$build | ForEach-Object {
    if ($_ -match "warning|error" -and $_ -notmatch "^Build") { Write-Host $_ }
}
$build | Select-String "Warning\(s\)|Error\(s\)" | ForEach-Object { Write-Host $_ }
if ($LASTEXITCODE -ne 0) { $failed = $true }

Write-Host ""
Write-Host "=== [2/3] Format Check ===" -ForegroundColor Cyan
dotnet format $project --verify-no-changes 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Run 'dotnet format $project' to fix formatting." -ForegroundColor Yellow
    $failed = $true
} else {
    Write-Host "Formatting OK." -ForegroundColor Green
}

Write-Host ""
Write-Host "=== [3/3] Vulnerable Packages ===" -ForegroundColor Cyan
dotnet list package --vulnerable 2>&1
if ($LASTEXITCODE -ne 0) { $failed = $true }

Write-Host ""
if ($failed) {
    Write-Host "LINT FAILED - fix the issues above before committing." -ForegroundColor Red
    exit 1
} else {
    Write-Host "ALL CHECKS PASSED" -ForegroundColor Green
    exit 0
}
