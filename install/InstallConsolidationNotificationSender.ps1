<#
.SYNOPSIS
Installs DPC.NotificationSender

.DESCRIPTION
DPC.ActionsService is a windows service for sending published products to fronts which could be both internal (DPC.Front, DPC.HighloadFront) and external

.EXAMPLE
  .\InstallConsolidationNotificationSender.ps1 -notifyPort 8012 -installRoot 'C:\QA' -logPath 'C:\Logs'

.EXAMPLE
  .\InstallConsolidationNotificationSender.ps1 -notifyPort 8012 -installRoot 'C:\QA' -logPath 'C:\Logs' -name 'DPC.NotificationSender' 

#>
param(
    ## Алиас DPC.NotificationSender
    [Parameter()]
    [String] $name = 'DPC.NotificationSender',
    ## Название DPC.NotificationSender
    [Parameter()]
    [String] $displayName = 'DPC Notification Service',
    ## Описание DPC.NotificationSender
    [Parameter()]
    [String] $description = 'Puts product updates into the DB queue and sends them to the fronts in a failover manner',
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
    ## DPC instance name
    [Parameter()]
    [string] $instanceId = 'Dev',
    ## Logs folder
    [Parameter(Mandatory = $true)]
    [String] $logPath
)


If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = Split-path -parent $MyInvocation.MyCommand.Definition

. (Join-Path $currentPath "Modules\Install-Service.ps1")

$parentPath = Split-Path -parent $currentPath
$sourcePath = Join-Path $parentPath "NotificationSender"

$installParams = @{
  name = $name;
  displayName = $displayName;
  description = $description;
  installRoot = $installRoot;
  login = $login;
  password = $password;
  projectName = "QA.Core.DPC.NotificationSender";
  source = $sourcePath;
  start = $false
}

Install-Service @installParams 

$installPath = Join-Path $installRoot $name
$nLogPath = Join-Path $installPath "NLogClient.config"
[xml]$nlog = Get-Content -Path $nLogPath

$nlog.nlog.internalLogFile = [string](Join-Path $logPath "internal-log.txt")

$node = $nlog.nlog.variable | Where-Object {$_.name -eq 'logDirectory'}
$node.value = [string](Join-Path $logPath $name)

$node = $nlog.nlog.rules.logger | Where-Object {$_.levels -eq 'Warn,Error,Fatal'}
$node.writeTo = "fileException"

$node = $nlog.nlog.rules.logger | Where-Object {$_.levels -eq 'Info'}
$node.writeTo = "fileInfo"

Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$appSettingsPath = Join-Path $installPath "appsettings.json"
$json = Get-Content -Path $appSettingsPath | ConvertFrom-Json

$json.Properties | Add-Member NoteProperty "InstanceId" $instanceId -Force

Set-ItemProperty $appSettingsPath -name IsReadOnly -value $false
$json | ConvertTo-Json | Out-File $appSettingsPath

$hostingPath = Join-Path $installPath "hosting.json"
$json = Get-Content -Path $hostingPath | ConvertFrom-Json

$json.urls = "http://*:${notifyPort}"

Set-ItemProperty $hostingPath -name IsReadOnly -value $false
$json | ConvertTo-Json | Out-File $hostingPath

$s = Get-Service $name

if ( $s.Status -eq "Stopped")
{
    Write-Output "Starting service $name..."
    $s.Start()
}
$timeout = "00:03:00";
try { $s.WaitForStatus("Running", $timeout) } catch [System.ServiceProcess.TimeoutException] { throw [System.ApplicationException] "Service '$name' hasn't been started in '$timeout' interval" } 
Write-Output "$name Running"

