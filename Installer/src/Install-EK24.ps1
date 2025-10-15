# EK24 Installer Script

# Define variables
$SoftwareName = "EK24"
$RevitVersion = "2024"
$SoftwareVersion = "14.0.1"
$BackupOutputDir = "T:\50_DESIGN DATA\Software"

# Paths
$AddinsPath = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$RevitVersion"
$DestinationPath = $AddinsPath
$SourcePath = Join-Path $PSScriptRoot "dist"
$AddinFile = Join-Path $AddinsPath "ek24.addin"
$PluginFolder = Join-Path $AddinsPath "ek24"

# Function to delete old files
function Remove-OldInstallation {
    Write-Host "Checking for previous installation..." -ForegroundColor Yellow
    
    if (Test-Path $AddinFile) {
        Write-Host "  Removing old addin file: $AddinFile" -ForegroundColor Cyan
        Remove-Item $AddinFile -Force -ErrorAction SilentlyContinue
    }
    
    if (Test-Path $PluginFolder) {
        Write-Host "  Removing old plugin folder: $PluginFolder" -ForegroundColor Cyan
        Remove-Item $PluginFolder -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# Main installation process
try {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "  $SoftwareName Installer v$SoftwareVersion" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Green
    
    # Check if source directory exists
    if (-not (Test-Path $SourcePath)) {
        throw "Source directory not found: $SourcePath"
    }
    
    # Remove old installation
    Remove-OldInstallation
    
    # Create destination directory if it doesn't exist
    if (-not (Test-Path $DestinationPath)) {
        Write-Host "Creating destination directory..." -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $DestinationPath -Force | Out-Null
    }
    
    # Copy files
    Write-Host "Installing $SoftwareName files to:" -ForegroundColor Yellow
    Write-Host "  $DestinationPath`n" -ForegroundColor Cyan
    
    # Copy with loading animation
    Write-Host "Copying files " -NoNewline -ForegroundColor Yellow
    
    $job = Start-Job -ScriptBlock {
        param($src, $dst)
        Copy-Item -Path "$src\*" -Destination $dst -Recurse -Force
    } -ArgumentList $SourcePath, $DestinationPath
    
    # Show spinner animation while copying
    $spinChars = '|', '/', '-', '\'
    $spinIndex = 0
    
    while ($job.State -eq 'Running') {
        Write-Host ("`b" + $spinChars[$spinIndex]) -NoNewline -ForegroundColor Cyan
        $spinIndex = ($spinIndex + 1) % 4
        Start-Sleep -Milliseconds 100
    }
    
    # Clear spinner and show completion
    Write-Host "`b " -NoNewline
    Write-Host "[OK]" -ForegroundColor Green
    
    # Get job result
    Receive-Job -Job $job -Wait -ErrorAction Stop | Out-Null
    Remove-Job -Job $job
    
    # Unblock all files
    Write-Host "Unblocking files " -NoNewline -ForegroundColor Yellow
    
    $unblockJob = Start-Job -ScriptBlock {
        param($path)
        Get-ChildItem -Path $path -Recurse | Unblock-File
    } -ArgumentList $DestinationPath
    
    # Show spinner for unblock operation
    $spinIndex = 0
    while ($unblockJob.State -eq 'Running') {
        Write-Host ("`b" + $spinChars[$spinIndex]) -NoNewline -ForegroundColor Cyan
        $spinIndex = ($spinIndex + 1) % 4
        Start-Sleep -Milliseconds 100
    }
    
    Write-Host "`b " -NoNewline
    Write-Host "[OK]" -ForegroundColor Green
    
    Receive-Job -Job $unblockJob -Wait -ErrorAction Stop | Out-Null
    Remove-Job -Job $unblockJob

    Write-Host "Files installed to: $DestinationPath`n" -ForegroundColor Cyan
    
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "  Installation completed successfully!" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Green
    
    
} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "  Installation Failed!" -ForegroundColor Red
    Write-Host "========================================`n" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host "`nPress any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
