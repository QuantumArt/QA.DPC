param(
    [String] $name = 'DPC.NotificationSender',
    [String] $displayName = 'DPC Notification Service',
    [String] $description = 'Puts product updates into the DB queue and sends them to the fronts in a failover manner', #ns---
    [String] $installRoot = 'C:\QA',
    [String] $login = 'NT AUTHORITY\SYSTEM',
    [String] $password = 'dummy',
    [String] $notifyPort = '8013',
    [String] $source = 'C:\DPC.MTS\NotificationsSender'
)


If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = split-path -parent $MyInvocation.MyCommand.Definition
$projectName = "QA.Core.DPC.NotificationSender"

$defaultSource = split-path -parent $currentPath
$zipPath = Join-Path $currentPath "$projectName.zip"
if ((Test-Path $zipPath)) { $defaultSource = $zipPath }

$source = Read-Or-Default $source "Please enter path to sources" $defaultSource

Invoke-Expression "InstallService.ps1 -Name '$name' -DisplayName '$displayName' -Description '$description' -ProjectName '$projectName' -InstallRoot '$installRoot' -source '$source' -login '$login' -password '$password' -start `$false"

$installPath = Join-Path $installRoot $name

$nLogPath = Join-Path $installPath "NLogClient.config"
[xml]$nlog = Get-Content -Path $nLogPath
$var = $nlog.nlog.targets.target | Where-Object {$_.name -eq 'fileinfo'}
$var2 = $nlog.nlog.targets.target | Where-Object {$_.name -eq 'fileexception'}
$var.fileName = $var.fileName -Replace $projectName, $name
$var.archiveFileName = $var.archiveFileName -Replace $projectName, $name
$var2.fileName = $var2.fileName -Replace $projectName, $name
$var2.archiveFileName = $var2.archiveFileName -Replace $projectName, $name
Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$appConfigPath = Join-Path $installPath "$projectName.exe.config"
[xml]$app = Get-Content -Path $appConfigPath


$qpMode = $app.CreateElement('add')
$qpMode.SetAttribute('key', 'QPMode')
$qpMode.SetAttribute('value', 'true')
$app.SelectSingleNode('//configuration/appSettings').AppendChild($qpMode)

$app.configuration.RemoveChild($app.configuration.connectionStrings)

$container = $app.configuration.unity.container
$notificationProvider = $container.register | Where-Object {$_.type -eq 'INotificationProvider'}
$notificationProvider.mapTo = "NotificationContentProvider";
$settingsService = $container.register | Where-Object {$_.type -eq 'ISettingsService'}
$settingsService.mapTo = "SettingsFromQpService";

$endpoint = $app.configuration.'system.serviceModel'.services.service.host.baseAddresses.add 
$endpoint.baseAddress = "http://${env:COMPUTERNAME}:$notifyPort/DpcNotificationService"

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

