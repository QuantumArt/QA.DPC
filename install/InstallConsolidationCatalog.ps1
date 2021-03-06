﻿<#
    .SYNOPSIS
    Installs QP.ProductCatalog components

    .DESCRIPTION
    During installation script does the following actions:
    - Validates input parameters and environment
    - Deletes components from previous installation
    - Installs services and applications:
        • Dpc.Admin: Catalog administration module
        • DPC.ActionsService: Task execution service
        • DPC.NotificationSender: Product publication service
        • Dpc.Front: Reference front
        • Dpc.SyncApi: API for writing product JSONs into ElasticSearch
        • Dpc.SearchApi: API for searching products in Elasticsearch
        • Dpc.WebApi: API for CRUD operations with products      
    - Restores sample catalog database from backup
    - Updates sample catalog database to actual state
    - Registers sample catalog database as QP customer code

    .EXAMPLE
    .\InstallConsolidationCatalog.ps1 -databaseServer dbhost -installRoot C:\QA  -elasticsearchHost 'http://node1:9200; http://node2:9200' -customerCode catalog_consolidation -backendPort 89

    .EXAMPLE
    .\InstallConsolidationCatalog.ps1 -databaseServer dbhost -targetBackupPath c:\temp\catalog_consolidation.bak -customerLogin login -customerPassword pass -currentSqlPath \\storage\current.sql  -installRoot C:\QA  -elasticsearchHost 'http://node1:9200; http://node2:9200' -customerCode catalog_consolidation -notifyPort 8012 -frontPort 8013 -searchApiPort 8014 -syncApiPort 8015 -webApiPort 8016  -backendPort 89

    .EXAMPLE
    .\InstallConsolidationCatalog.ps1 -databaseServer dbhost -sourceBackupPath \\storage\catalog_consolidation.bak -targetBackupPath c:\temp\catalog_consolidation.bak -customerLogin login -customerPassword pass -currentSqlPath \\storage\current.sql  -installRoot C:\QA  -elasticsearchHost 'http://node1:9200; http://node2:9200' -customerCode catalog_consolidation -notifyPort 8012 -frontPort 8013 -searchApiPort 8014 -syncApiPort 8015 -webApiPort 8016  -backendPort 89
#>
param (
    ## Cleanup (or not) previous version of catalog
    [Parameter()]
    [bool] $cleanUp = $true,
    ## Database server name
    [Parameter(Mandatory = $true)]
    [string] $databaseServer,
    ## Backup file for copying onto database server
    [Parameter()]
    [ValidateScript({ if ($_) { Test-Path $_} })]
    [string] $sourceBackupPath,
    ## Backup file for restoring (server local - for SQL Server)
    [Parameter()]
    [string] $targetBackupPath,
    ## Admin database user name
    [Parameter()]
    [String] $login,
    ## Admin database user password
    [Parameter()]
    [String] $password,
    ## Catalog database user name
    [Parameter()]
    [string] $customerLogin,
    ## Catalog database user password
    [Parameter()]
    [string] $customerPassword,
    ## Path to sql script for bringing catalog database up-to-date
    [Parameter()]
    [ValidateScript({ if ($_) { Test-Path $_} })]
    [string] $currentSqlPath,
    ## Catalog customer code
    [Parameter(Mandatory = $true)]
    [string] $customerCode,
    ## DPC.NotificationSender service port
    [Parameter()]
    [int] $actionsPort = 8011, 
    ## DPC.NotificationSender service port
    [Parameter()]
    [int] $notifyPort = 8012,
    ## Dpc.Front site port
    [Parameter()]
    [int] $frontPort = 8013,
    ## Dpc.SearchApi site port
    [Parameter()]
    [int] $searchApiPort = 8014,
    ## Dpc.SyncApi site port
    [Parameter()]
    [int] $syncApiPort = 8015,
    ## Dpc.WebApi site port
    [Parameter()]
    [int] $webApiPort = 8016,
    ## QP site port
    [Parameter(Mandatory = $true)]
    [int] $backendPort,
    ## Elasticsearch cluster address
    [Parameter(Mandatory = $true)]
    [string] $elasticsearchHost,
    ## Folder to install services
    [Parameter(Mandatory = $true)]
    [ValidateScript({ if ($_) { Test-Path $_} })]
    [string] $installRoot,
    ## Store product versions (or not) on DPC.Front
    [Parameter()]
    [bool] $useProductVersions = $false,
    ## QP site name
    [Parameter()]
    [string] $qpName = 'QP8',
    ## Dpc.Admin site name
    [Parameter()]
    [string] $adminName = 'Dpc.Admin',
    ## DPC.ActionsService service name
    [Parameter()]
    [string] $actionsName = 'DPC.ActionsService',
    ## Dpc.SearchApi site name
    [Parameter()]
    [string] $searchApiName = 'Dpc.SearchApi',
    ## DPC.NotificationSender service name
    [Parameter()]
    [string] $notificationsName = 'DPC.NotificationSender',
    ## Dpc.Front site name
    [Parameter()]
    [string] $frontName = 'Dpc.Front',
    ## Dpc.WebApi site name
    [Parameter()]
    [string] $webApiName = 'Dpc.WebApi',
    ## Dpc.SyncApi site name
    [Parameter()]
    [string] $syncApiName = 'Dpc.SyncApi',
    ## Unique instance name (for fronts)
    [Parameter()]
    [string] $instanceId = 'Dev',
    ## Log folder path
    [Parameter()]
    [string] $logPath = 'C:\Logs',
    ## Extra validation libraries (comma-separated)
    [Parameter()]
    [string] $libraries = '',
    ## Database type: 0 - SQL Server, 1 - Postgres
    [Parameter()]
    [int] $dbType = 0,
    ## Elasticsearch storage options JSON
    [Parameter()]
    [string] $elasticStoreOptions = ''
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

if ($logPath){
    $installLog = Join-Path $logPath "install.log"
    Start-Transcript -Path $installLog -Append
}

$actionsArtifactName = 'ActionsRunner' 
$notificationsArtifactName = 'NotificationsSender'
$highloadFrontArtifactName = 'HighloadFront'
$frontArtifactName = 'Front'
$webApiArtifactName = 'WebApi'

$frontHost = "${env:COMPUTERNAME}:$frontPort"
$syncApiHost = "${env:COMPUTERNAME}:$syncApiPort"
$adminHost = "${env:COMPUTERNAME}:$backendPort/$adminName"

$currentPath = Split-path -parent $MyInvocation.MyCommand.Definition
$parentPath = Split-Path -parent $currentPath

if (!$currentSqlPath) {
    $path = Join-Path $currentPath "current.sql"
    if (Test-Path $path){
        $currentSqlPath = $path
    } else { 
        throw "currentSqlPath is not found on $path"
    }
}

$defaultBackupName = if ($dbType -eq 0) { "catalog_consolidation.bak" } else { "catalog.dump" }
if (!$sourceBackupPath) {
    $path = Join-Path $currentPath $defaultBackupName
    if (Test-Path $path){
        $sourceBackupPath = $path;
    }
}

if (!$targetBackupPath) {
    $targetBackupPath = "C:\temp\$defaultBackupName"
}

if (-not(Get-Module -Name WebAdministration)) {
    Import-Module WebAdministration
}

. (Join-Path $currentPath "Modules\Database.ps1")
. (Join-Path $currentPath "Modules\CustomerCode.ps1")
. (Join-Path $currentPath "Modules\Get-SiteOrApplication.ps1")


$validationPath = Join-Path $currentPath "ValidateConsolidation.ps1"
Invoke-Expression "$validationPath -DatabaseServer '$databaseServer' -login '$login' -password '$password' -dbType $dbType"

if ($cleanUp) {
    $uninstallPath = Join-Path $currentPath "UninstallConsolidation.ps1"
    $params = "-CustomerCode '$customerCode' -InstallRoot '$installRoot' -Admin '$adminName' -NotificationSender '$notificationsName' -ActionsService '$actionsName' -Front '$frontName' -WebApi '$webApiName' -SyncApi '$syncApiName' -SearchApi '$searchApiName' -QpName '$qpName'"
    Invoke-Expression "$uninstallPath $params"
}

Invoke-Expression "$validationPath -actionsPort $actionsPort -notifyPort $notifyPort -frontPort $frontPort -searchApiPort $searchApiPort -syncApiPort $syncApiPort -webApiPort $webApiPort"

$scriptName = Join-Path $currentPath "InstallConsolidationAdmin.ps1"
Invoke-Expression "$scriptName -notifyPort $notifyPort -syncApiPort $syncApiPort -Admin '$adminName' -Qp '$qpName' -LogPath '$logPath' -Libraries '$libraries'"

$scriptName = Join-Path $currentPath "InstallConsolidationNotificationSender.ps1"
$source = Join-Path $parentPath $notificationsArtifactName
Invoke-Expression "$scriptName -NotifyPort $notifyPort -InstallRoot $installRoot -Name '$notificationsName' -LogPath '$logPath' -InstanceId $instanceId"

$scriptName = Join-Path $currentPath "InstallConsolidationActionsService.ps1"
$source = Join-Path $parentPath $actionsArtifactName
Invoke-Expression "$scriptName -actionsPort $actionsPort -NotifyPort $notifyPort -InstallRoot $installRoot -Name '$actionsName' -LogPath '$logPath'"

$scriptName = Join-Path $currentPath "InstallConsolidationFront.ps1"
$source = Join-Path $parentPath $frontArtifactName
Invoke-Expression "$scriptName -Port $frontPort -SiteName '$frontName' -UseProductVersions `$$useProductVersions -LogPath '$logPath' -InstanceId $instanceId"

$scriptName = Join-Path $currentPath "InstallConsolidationHighloadFront.ps1"
$source = Join-Path $parentPath $highloadFrontArtifactName
Invoke-Expression "$scriptName -Port $syncApiPort -SiteName '$syncApiName' -CanUpdate `$$true -LogPath '$logPath' -InstanceId $instanceId -elasticStoreOptions '$elasticStoreOptions'"
Invoke-Expression "$scriptName -Port $searchApiPort -SiteName '$searchApiName' -CanUpdate `$$false -LogPath '$logPath' -InstanceId $instanceId"

$scriptName = Join-Path $currentPath "InstallConsolidationWebApi.ps1"
$source = Join-Path $parentPath $webApiArtifactName
Invoke-Expression "$scriptName -Port $webApiPort -SiteName '$webApiName' -NotifyPort $notifyPort -LogPath '$logPath'"

$scriptName = Join-Path $currentPath "InstallConsolidationCustomerCode.ps1"
$params = "-DatabaseServer '$databaseServer' -TargetBackupPath '$targetBackupPath' -CustomerCode '$customerCode' -CustomerLogin '$customerLogin' -CustomerPassword '$customerPassword' -CurrentSqlPath '$currentSqlPath' -FrontHost '$frontHost' -SyncApiHost '$syncApiHost' -ElasticsearchHost '$elasticsearchHost' -AdminHost '$adminHost' -DbType $dbType -UploadFolder $customerCode"
if ($sourceBackupPath) { 
    $params = "$params -SourceBackupPath '$sourceBackupPath'" 
}
if ($login -and $password) {
    $params = "$params  -Login '$login' -Password '$password'" 
}
Invoke-Expression "$scriptName $params"