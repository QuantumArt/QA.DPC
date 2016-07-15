param(
    [String] $name,
    [String] $displayName,
    [String] $description,
    [String] $installRoot,
    [String] $login,
    [String] $password,
    [String] $dbname,
    [String] $catalogDbName,
    [String] $dbServerName,
    [String] $adminLogin,
    [String] $adminPassword,
    [String] $dbLogin,
    [String] $dbPassword,
    [String] $port,
    [String] $source
)

function Read-Or-Default([String] $value, [String] $message, [String] $defaultValue)
{
    $fullMessage = $message
    if (-not([string]::IsNullOrEmpty($defaultValue)))  { $fullMessage = $fullMessage + "(default name - $defaultValue)" }
    if ([string]::IsNullOrEmpty($value)) { $value = Read-Host $fullMessage  }
    if ([string]::IsNullOrEmpty($value)) { $value = $defaultValue }
    return $value
}

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = split-path -parent $MyInvocation.MyCommand.Definition
$projectName = "QA.Core.DPC.NotificationSender"

$name = Read-Or-Default $name "Please enter notifications service name" "DPC.NotificationSender"
$displayName = Read-Or-Default $displayName "Please enter notifications service display name" "DPC Notification Service"
$description = Read-Or-Default $description "Please enter notifications service description" "Puts product updates into the DB queue and sends them to the fronts in a failover manner"
$installRoot = Read-Or-Default $installRoot "Please enter install root" "C:\QA"

$login = Read-Or-Default $login "Please enter login to run service" "NT AUTHORITY\SYSTEM"
$password = Read-Or-Default $password "Please enter password to run service" "dummy"
$dbname = Read-Or-Default $dbname "Please enter database name for notifications" "dpc_notifications"
$catalogDbName = Read-Or-Default $catalogDbName "Please enter database name for catalog" "catalog"

$dbServerName = Read-Or-Default $dbServerName "Please enter db server name to connect"
$adminLogin = Read-Or-Default $adminLogin "Please enter admin login name for creating databases"
$adminPassword = Read-Or-Default $adminPassword "Please enter admin password for creating databases"
$dbLogin = Read-Or-Default $dbLogin "Please enter login name to connect databases"
$dbPassword = Read-Or-Default $dbPassword "Please enter password to connect databases"
$port = Read-Or-Default $port "Please enter port for notification sender" "8011"
$url = "http://${env:COMPUTERNAME}:$port/DpcNotificationService"

$defaultSource = split-path -parent $currentPath
$zipPath = Join-Path $currentPath "$projectName.zip"
if ((Test-Path $zipPath)) { $defaultSource = $zipPath }

$source = Read-Or-Default $source "Please enter path to sources" $defaultSource

$dbScriptPath = Join-Path $currentPath "dpc_notifications.sql"

Invoke-Expression "InstallService.ps1 -Name '$name' -DisplayName '$displayName' -Description '$description' -ProjectName '$projectName' -InstallRoot '$installRoot' -source '$source' -login '$login' -password '$password' -start 'N'"

Invoke-Expression "CreateDB.ps1 -DbName '$dbName' -DbServerName '$dbServerName' -AdminLogin '$adminLogin' -AdminPassword '$adminPassword' -Login '$dbLogin' -ScriptPath '$dbScriptPath'"

$installPath = Join-Path $installRoot $name

$nLogPath = Join-Path $installPath "NLogClient.config"
[xml]$nlog = Get-Content -Path $nLogPath
$var = $nlog.nlog.targets.target | where {$_.name -eq 'fileinfo'}
$var2 = $nlog.nlog.targets.target | where {$_.name -eq 'fileexception'}
$var.fileName = $var.fileName -Replace $projectName, $name
$var.archiveFileName = $var.archiveFileName -Replace $projectName, $name
$var2.fileName = $var2.fileName -Replace $projectName, $name
$var2.archiveFileName = $var2.archiveFileName -Replace $projectName, $name
Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)


$appConfigPath = Join-Path $installPath "$projectName.exe.config"
[xml]$app = Get-Content -Path $appConfigPath
$var = $app.configuration.connectionStrings.add | where {$_.name -eq 'QA.Core.DPC.Properties.Settings.beeline_dpc_notificationsConnectionString'}
$var2 = $app.configuration.connectionStrings.add | where {$_.name -eq 'qp_database'}
$var3 = $app.configuration.'system.serviceModel'.services.service.host.baseAddresses.add 
$var.connectionString = "Initial Catalog=$dbName;Data Source=$dbServerName;User ID=$dbLogin;Password=$dbPassword"
$var2.connectionString = "Initial Catalog=$catalogDbName;Data Source=$dbServerName;User ID=$dbLogin;Password=$dbPassword"
$var3.baseAddress = $url
Set-ItemProperty $appConfigPath -name IsReadOnly -value $false
$app.Save($appConfigPath)

$s = Get-Service $name

if ( $s.Status -eq "Stopped")
{
    Write-Output "Starting service $name..."
    $s.Start()
}
$timeout = "00:03:00";
try { $s.WaitForStatus("Running", $timeout) } catch [System.ServiceProcess.TimeoutException] { throw [System.ApplicationException] "Service '$name' hasn't been started in '$timeout' interval" } 
Write-Output "$name Running"

