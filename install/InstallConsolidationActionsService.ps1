<#
.SYNOPSIS
Installs DPC.ActionsService

.DESCRIPTION
DPC.ActionsService is a windows service for processing long DPC tasks e.g product publication and calling routine tasks by schedule

.EXAMPLE
  .\InstallConsolidationActionsService.ps1 -actionsPort 8011 -notifyPort 8012 -installRoot 'C:\QA' -logPath 'C:\Logs' 

.EXAMPLE
  .\InstallConsolidationActionsService.ps1 -actionsPort 8011 -notifyPort 8012 -installRoot 'C:\QA' -logPath 'C:\Logs' -name 'DPC.ActionsService' 

#>
param(
    ## Service Name
    [Parameter()]
    [String] $name = 'DPC.ActionsService',
    ## Service Display Name
    [Parameter()]
    [String] $displayName = 'DPC Actions Service',
    ## Service Description
    [Parameter()]
    [String] $description = 'Run long tasks for DPC with updating progress',
    ## Path to install DPC Services
    [Parameter(Mandatory = $true)]
    [String] $installRoot,
    ## User account to run service
    [Parameter()]
    [String] $login = 'NT AUTHORITY\SYSTEM',
    ## User password to run service
    [Parameter()]
    [String] $password = 'dummy',
    ## Service port to run
    [Parameter(Mandatory = $true)]
    [int] $actionsPort,
    ## DPC.Notificationsender port
    [Parameter(Mandatory = $true)]
    [int] $notifyPort,
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
$sourcePath = Join-Path $parentPath "ActionsService"

$installParams = @{
  name = $name;
  displayName = $displayName;
  description = $description;
  installRoot = $installRoot;
  login = $login;
  password = $password;
  projectName = "QA.Core.ProductCatalog.ActionsService";
  source = $sourcePath;
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

$json.Loader.UseFileSizeService = $false
$json.Properties.EnableScheduleProcess = $false

$integration = $json.Integration
if (!$integration) {
    $integration = New-Object PSObject
    $json | Add-Member NoteProperty "Integration" $integration
}

$integration | Add-Member NoteProperty "RestNotificationUrl" "http://${env:COMPUTERNAME}:$notifyPort" -Force

Set-ItemProperty $appSettingsPath -name IsReadOnly -value $false
$json | ConvertTo-Json | Out-File $appSettingsPath

$hostingPath = Join-Path $installPath "hosting.json"
$json = Get-Content -Path $hostingPath | ConvertFrom-Json

$json.urls = "http://*:${actionsPort}"

Set-ItemProperty $hostingPath -name IsReadOnly -value $false
$json | ConvertTo-Json | Out-File $hostingPath

$s = Get-Service $name

if ( $s.Status -eq "Stopped")
{
    Write-Host "Starting service $name..."
    $s.Start()
}
$timeout = "00:03:00";
try { $s.WaitForStatus("Running", $timeout) } catch [System.ServiceProcess.TimeoutException] { throw [System.ApplicationException] "Service '$name' hasn't been started in '$timeout' interval" } 
Write-Host "$name Running"
