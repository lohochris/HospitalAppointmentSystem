# ============================================================
# Database Reset Script for Hospital Appointment System
# Safely backs up and deletes old database to force recreation
# ============================================================

$binPath = "C:\Users\Loho Christopher\source\repos\HospitalAppointmentSystem\HospitalAppointmentSystem\bin\Debug"
$dbFile = Join-Path $binPath "HospitalDB.db"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Hospital DB Reset Utility" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if database exists
if (Test-Path $dbFile) {
	Write-Host "✓ Found database: $dbFile" -ForegroundColor Green

	# Get file info
	$fileInfo = Get-Item $dbFile
	Write-Host "  Size: $($fileInfo.Length / 1KB) KB" -ForegroundColor Gray
	Write-Host "  Last Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
	Write-Host ""

	# Create backup
	$backupFile = Join-Path $binPath "HospitalDB_backup_$timestamp.db"
	Write-Host "Creating backup..." -ForegroundColor Yellow
	Copy-Item $dbFile $backupFile

	if (Test-Path $backupFile) {
		Write-Host "✓ Backup created: HospitalDB_backup_$timestamp.db" -ForegroundColor Green
	} else {
		Write-Host "✗ Backup failed!" -ForegroundColor Red
		exit 1
	}

	# Delete database files
	Write-Host ""
	Write-Host "Deleting old database files..." -ForegroundColor Yellow

	$filesToDelete = @(
		"HospitalDB.db",
		"HospitalDB.db-journal",
		"HospitalDB.db-wal",
		"HospitalDB.db-shm"
	)

	foreach ($file in $filesToDelete) {
		$fullPath = Join-Path $binPath $file
		if (Test-Path $fullPath) {
			Remove-Item $fullPath -Force
			Write-Host "  ✓ Deleted: $file" -ForegroundColor Green
		}
	}

	Write-Host ""
	Write-Host "========================================" -ForegroundColor Cyan
	Write-Host "✓ Database reset complete!" -ForegroundColor Green
	Write-Host "========================================" -ForegroundColor Cyan
	Write-Host ""
	Write-Host "Next steps:" -ForegroundColor Yellow
	Write-Host "1. Run your application" -ForegroundColor White
	Write-Host "2. Database will be recreated with correct schema" -ForegroundColor White
	Write-Host "3. All new tables will include RBAC columns" -ForegroundColor White
	Write-Host ""
	Write-Host "Backup location:" -ForegroundColor Yellow
	Write-Host "  $backupFile" -ForegroundColor White

} else {
	Write-Host "✓ No database found - will be created on next run" -ForegroundColor Green
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
