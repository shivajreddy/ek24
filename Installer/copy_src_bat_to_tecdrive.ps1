# EK24 Deployment Script
# Copies src folder and .bat file to network drive
# Archives old versions automatically

# Define paths
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$SourceSrcFolder = Join-Path $ScriptRoot "src"
$SourceBatFile = Join-Path $ScriptRoot "Install-EK24.bat"
$DestinationBase = "T:\50_DESIGN DATA\Software"
$ArchiveBase = Join-Path $DestinationBase "Archive"
$DestinationSrcFolder = Join-Path $DestinationBase "src"
$DestinationBatFile = Join-Path $DestinationBase "Install-EK24.bat"

# Function to archive old files
function Backup-OldFiles {
    param (
        [string]$SrcFolder,
        [string]$BatFile,
        [string]$ArchivePath
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
    $archiveFolder = Join-Path $ArchivePath "EK24_Backup_$timestamp"
    
    # Create archive folder
    if (-not (Test-Path $archiveFolder)) {
        New-Item -ItemType Directory -Path $archiveFolder -Force | Out-Null
    }
    
    # Move old src folder if it exists
    if (Test-Path $SrcFolder) {
        Write-Host "  Moving old src folder to archive..." -ForegroundColor Yellow
        Move-Item -Path $SrcFolder -Destination $archiveFolder -Force
    }
    
    # Move old bat file if it exists
    if (Test-Path $BatFile) {
        Write-Host "  Moving old .bat file to archive..." -ForegroundColor Yellow
        Move-Item -Path $BatFile -Destination $archiveFolder -Force
    }
    
    Write-Host "  Old files archived to: $archiveFolder" -ForegroundColor Cyan
}

# Main deployment process
try {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "  EK24 Deployment Script" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Green
    
    # Verify source files exist
    if (-not (Test-Path $SourceSrcFolder)) {
        throw "Source 'src' folder not found: $SourceSrcFolder"
    }
    
    if (-not (Test-Path $SourceBatFile)) {
        throw "Source .bat file not found: $SourceBatFile"
    }
    
    # Verify destination drive is accessible
    if (-not (Test-Path "T:\")) {
        throw "T: drive is not accessible. Please ensure the network drive is mounted."
    }
    
    # Create destination base folder if it doesn't exist
    if (-not (Test-Path $DestinationBase)) {
        Write-Host "Creating destination folder..." -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $DestinationBase -Force | Out-Null
    }
    
    # Create archive folder if it doesn't exist
    if (-not (Test-Path $ArchiveBase)) {
        Write-Host "Creating archive folder..." -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $ArchiveBase -Force | Out-Null
    }
    
    # Check if old files exist and archive them
    if ((Test-Path $DestinationSrcFolder) -or (Test-Path $DestinationBatFile)) {
        Write-Host "Old installation found. Archiving..." -ForegroundColor Yellow
        Backup-OldFiles -SrcFolder $DestinationSrcFolder -BatFile $DestinationBatFile -ArchivePath $ArchiveBase
        Write-Host ""
    } else {
        Write-Host "No previous installation found. Proceeding with fresh deployment..." -ForegroundColor Yellow
        Write-Host ""
    }
    
    # Copy new files
    Write-Host "Copying new files to destination..." -ForegroundColor Yellow
    
    # Copy src folder
    Write-Host "  Copying src folder..." -ForegroundColor Cyan
    Copy-Item -Path $SourceSrcFolder -Destination $DestinationBase -Recurse -Force
    
    # Copy bat file
    Write-Host "  Copying Install-EK24.bat..." -ForegroundColor Cyan
    Copy-Item -Path $SourceBatFile -Destination $DestinationBase -Force
    
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "  Deployment completed successfully!" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Green
    
    Write-Host "Files deployed to: $DestinationBase" -ForegroundColor Cyan
    Write-Host "Archives located at: $ArchiveBase`n" -ForegroundColor Cyan
    
} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "  Deployment Failed!" -ForegroundColor Red
    Write-Host "========================================`n" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host "`nPress any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
