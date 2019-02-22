<#
.SYNOPSIS
Установка сервиса выполнения задач

.DESCRIPTION
Cервис выполнения задач DPC.ActionsService это win служба для выполнения Custom Actions без блокировки интерфейса бэкэнда и выполнения задач по расписанию

.EXAMPLE
  .\InstallConsolidationActionsService.ps1 -notifyPort 8012 -installRoot 'C:\QA' -source 'C:\Catalog\ActionsRunner'

.EXAMPLE
  .\InstallConsolidationActionsService.ps1 -notifyPort 8012 -installRoot 'C:\QA' -name 'DPC.ActionsService' -source 'C:\Catalog\ActionsRunner'

#>
param(
    ## Алиас DPC.ActionsService
    [Parameter()]
    [String] $name = 'DPC.ActionsService',
    ## Название DPC.ActionsService
    [Parameter()]
    [String] $displayName = 'DPC Actions Service',
    ## Описание DPC.ActionsService
    [Parameter()]
    [String] $description = 'Run long tasks for DPC with updating progress',
    ## Путь к каталогу установки сервисов каталога
    [Parameter(Mandatory = $true)]
    [String] $installRoot,
    ## Пользователь от которого будет запущен сервис
    [Parameter()]
    [String] $login = 'NT AUTHORITY\SYSTEM',
    ## Пароль пользователя
    [Parameter()]
    [String] $password = 'dummy',
    ## Порт DPC.NotificationSender
    [Parameter(Mandatory = $true)]
    [int] $notifyPort,
    ## Путь к сервису, откуда он будет установлен
    [Parameter(Mandatory = $true)]    
    [String] $source
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$projectName = "QA.Core.ProductCatalog.ActionsService"
$installPath = Join-Path $installRoot $name


$currentPath = Split-path -parent $MyInvocation.MyCommand.Definition
$InstallServicePath = Join-Path $currentPath "InstallService.ps1"
Invoke-Expression "$InstallServicePath -Name '$name' -DisplayName '$displayName' -Description '$description' -ProjectName '$projectName' -InstallRoot '$installRoot' -source '$source' -login '$login' -password '$password' -start `$false"

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
$app.configuration.appSettings.AppendChild($qpMode)

$app.SelectSingleNode('//configuration/applicationSettings/QA.Core.ProductCatalog.ActionsService.Properties.Settings/setting[@name="EnableSheduleProcess"]/value').InnerXml = 'False'

$app.configuration.RemoveChild($app.configuration.connectionStrings)

$container = $app.configuration.unity.container
$articleFormatter = $container.register | Where-Object {$_.type -eq 'IArticleFormatter'}
$articleFormatter.mapTo = "JsonProductFormatter";
$settingsService = $container.register | Where-Object {$_.type -eq 'ISettingsService'}
$settingsService.mapTo = "SettingsFromQpService";

$endpoint = $app.configuration.'system.serviceModel'.client.endpoint
$endpoint.address = "http://${env:COMPUTERNAME}:$notifyPort/DpcNotificationService"

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
