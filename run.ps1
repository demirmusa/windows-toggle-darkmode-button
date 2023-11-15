# Define the project directory relative to the script location
$projectDir = Join-Path -Path $PSScriptRoot -ChildPath "src\WindowsToggleDarkModeButton"

# Change the current location to the project directory
Set-Location -Path $projectDir

# Define the publish directory relative to the script location
$publishDir = Join-Path -Path $PSScriptRoot -ChildPath "publish"

# Check if the publish directory exists and clear its contents if it does
if (Test-Path -Path $publishDir) {
    # Clear the contents of the publish directory
    Remove-Item -Path "$publishDir\*" -Recurse -Force
}

# Run the dotnet publish command with the specified parameters
dotnet publish -c Release -r win-x64 --self-contained -o $publishDir /p:PublishSingleFile=true /p:PublishReadyToRun=true

# Print a completion message to the host
Write-Host "Publish has been completed"

# Define the path to the user's Startup folder
$startupFolder = [Environment]::GetFolderPath("Startup")

# Define the path for the shortcut
$lnkPath = Join-Path -Path $startupFolder -ChildPath "Toggle Dark Mode.lnk"

# Check if the shortcut already exists and delete it if it does
if (Test-Path -Path $lnkPath) {
    Remove-Item -Path $lnkPath -Force
}

# Create a new shortcut to the published application
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($lnkPath)
$shortcut.TargetPath = Join-Path -Path $publishDir -ChildPath "Toggle Dark Mode.exe"
$shortcut.WorkingDirectory = $publishDir # Set the "Start in" directory
$shortcut.Save()

# Release the COM object
[System.Runtime.InteropServices.Marshal]::ReleaseComObject($shell) | Out-Null
Remove-Variable -Name shell

# Output a message to indicate the startup shortcut has been created or replaced
Write-Host "Startup shortcut has been created or replaced."

# Start the application
Start-Process -FilePath $shortcut.TargetPath

# Output a message to indicate the application has started
Write-Host "Application has been started."

Set-Location -Path $PSScriptRoot