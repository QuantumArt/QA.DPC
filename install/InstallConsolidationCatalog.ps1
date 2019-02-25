<#
.SYNOPSIS
Установка каталога

.DESCRIPTION
В процессе установки каталога:
- Провдодится валидация параметров и окружения на возможность установки
- Опционально очищаются ранее установленные компоненты каталога
- Устанавливаются сервисы:
    • Dpc.Admin: Бэкэнд каталога
    • DPC.ActionsService: Сервис выполнения задач
    • DPC.NotificationSender: Сервис публикации продуктов
    • Dpc.SiteSync: Референсная витрина
    • Dpc.SyncApi: Витрина индексации продуктов в Elasticsearch
    • Dpc.SearchApi: Поиск продуктов по индексам Elasticsearch
    • Dpc.WebApi: API каталога      
- Развертывается база данных каталога из бэкапа
- База каталога обновляется до актуального состояния
- Регистрируется в QP кастомер код каталога

.EXAMPLE
  .\InstallConsolidationCatalog.ps1 -databaseServer dbhost -targetBackupPath c:\temp\catalog_consolidation.bak -customerLogin login -customerPassword pass -currentSqlPath \\storage\current.sql  -installRoot C:\QA  -elasticsearchHost 'http://node1:9200; http://node2:9200' -customerCode catalog_consolidation -notifyPort 8012 -siteSyncPort 8013 -searchApiPort 8014 -syncApiPort 8015 -webApiPort 8016  -backendPort 89

.EXAMPLE
  .\InstallConsolidationCatalog.ps1 -databaseServer dbhost -sourceBackupPath \\storage\catalog_consolidation.bak -targetBackupPath c:\temp\catalog_consolidation.bak -customerLogin login -customerPassword pass -currentSqlPath \\storage\current.sql  -installRoot C:\QA  -elasticsearchHost 'http://node1:9200; http://node2:9200' -customerCode catalog_consolidation -notifyPort 8012 -siteSyncPort 8013 -searchApiPort 8014 -syncApiPort 8015 -webApiPort 8016  -backendPort 89
#>
param(
    ## Флаг очистки ранее установленных компонент каталога
    [Parameter()]
    [bool] $cleanUp = $true,
    ## Сервер баз данных
    [Parameter(Mandatory = $true)]
    [string] $databaseServer,
    ## Путь к бэкапу базы каталога
    [Parameter()]
    [ValidateScript({ if (-not [string]::IsNullOrEmpty($_)) { Test-Path $_}})]
    [string] $sourceBackupPath,
    ## Локальный путь к бэкапу базы каталога на сервере баз данных
    [Parameter(Mandatory = $true)]
    [string] $targetBackupPath,
    ## Пользователь для коннекта к базе данных каталога
    [Parameter()]
    [string] $customerLogin,
    ## Пароль для коннекта к базе данных каталога
    [Parameter()]
    [string] $customerPassword,
    ## Путь к скрипту актуализации базы данных каталога
    [Parameter(Mandatory = $true)]
    [ValidateScript({ if (-not [string]::IsNullOrEmpty($_)) { Test-Path $_}})]
    [string] $currentSqlPath,
    ## Кастомер код каталога
    [Parameter(Mandatory = $true)]
    [string] $customerCode, 
    ## Порт DPC.NotificationSender
    [Parameter(Mandatory = $true)]
    [int] $notifyPort,
    ## Порт Dpc.SiteSync
    [Parameter(Mandatory = $true)]
    [int] $siteSyncPort,
    ## Порт Dpc.SearchApi
    [Parameter(Mandatory = $true)]
    [int] $searchApiPort,
    ## Порт Dpc.SyncApi
    [Parameter(Mandatory = $true)]
    [int] $syncApiPort,
    ## Порт Dpc.WebApi
    [Parameter(Mandatory = $true)]
    [int] $webApiPort,
    ## Порт бэкэнда QP
    [Parameter(Mandatory = $true)]
    [int] $backendPort,
    ## Хост кластера Elasticsearch
    [Parameter(Mandatory = $true)]
    [string] $elasticsearchHost,
    ## Путь к каталогу установки сервисов каталога
    [Parameter(Mandatory = $true)]
    [ValidateScript({ if (-not [string]::IsNullOrEmpty($_)) { Test-Path $_}})]
    [string] $installRoot,
    ## Флаг версионности на референсной витрине
    [Parameter()]
    [bool] $useProductVersions = $false,
    ## Название QP
    [Parameter()]
    [string] $qpName = 'QP8',
    ## Название Dpc.Admin
    [Parameter()]
    [string] $adminName = 'Dpc.Admin',
    ## Название DPC.ActionsService
    [Parameter()]
    [string] $actionsName = 'DPC.ActionsService',
    ## Название Dpc.SearchApi
    [Parameter()]
    [string] $searchApiName = 'Dpc.SearchApi',
    ## Название DPC.NotificationSender
    [Parameter()]
    [string] $notificationsName = 'DPC.NotificationSender',
    ## Название Dpc.SiteSync
    [Parameter()]
    [string] $siteSyncName = 'Dpc.SiteSync',
    ## Название Dpc.WebApi
    [Parameter()]
    [string] $webApiName = 'Dpc.WebApi',
    ## Название Dpc.SyncApi
    [Parameter()]
    [string] $syncApiName = 'Dpc.SyncApi'
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$actionsArtifactName = 'ActionsRunner' 
$notificationsArtifactName = 'NotificationsSender'
$highloadFrontArtifactName = 'HighloadFront'
$siteSyncArtifactName = 'Front'
$webApiArtifactName = 'WebApi'

$siteSyncHost = "${env:COMPUTERNAME}:$siteSyncPort"
$syncApiHost = "${env:COMPUTERNAME}:$syncApiPort"
$adminHost = "${env:COMPUTERNAME}:$backendPort/$adminName"

$currentPath = Split-path -parent $MyInvocation.MyCommand.Definition
$parentPath = Split-Path -parent $currentPath

Import-Module WebAdministration
Import-Module SqlServer

. (Join-Path $currentPath "Modules\Add-DatabaseUser.ps1")
. (Join-Path $currentPath "Modules\Restore-Database.ps1")
. (Join-Path $currentPath "Modules\Get-ConnectionString.ps1")
. (Join-Path $currentPath "Modules\CustomerCode.ps1")
. (Join-Path $currentPath "Modules\Get-SiteOrApplication.ps1")

$validationPath = Join-Path $currentPath "ValidateConsolidation.ps1"
Invoke-Expression "$validationPath -DatabaseServer '$databaseServer'"

if ($cleanUp){
    $uninstallPath = Join-Path $currentPath "UninstallConsolidation.ps1"
    $params = "-CustomerCode '$customerCode' -InstallRoot '$installRoot' -Admin '$adminName' -NotificationSender '$notificationsName' -ActionsService '$actionsName' -SiteSync '$siteSyncName' -WebApi '$webApiName' -SyncApi '$syncApiName' -SearchApi '$searchApiName' -QpName '$qpName'"
    Invoke-Expression "$uninstallPath $params"
}

#Invoke-Expression "$validationPath -NotifyPort $notifyPort -SiteSyncPort $syncApiPort -SearchApiPort $searchApiPort -SyncApiPort $syncApiPort -WebApiPort $webApiPort"

$installAdminiPath = Join-Path $currentPath "InstallConsolidationAdmin.ps1"
#Invoke-Expression "$installAdminiPath -NotifyPort $notifyPort -SyncPort $syncApiPort -Admin '$adminName' -Qp '$qpName'"

$installNotificationSenderPath = Join-Path $currentPath "InstallConsolidationNotificationSender.ps1"
$source = Join-Path $parentPath $notificationsArtifactName
#Invoke-Expression "$installNotificationSenderPath -NotifyPort $notifyPort -InstallRoot $installRoot -Name '$notificationsName' -Source '$source'"

$installActionsServicePath = Join-Path $currentPath "InstallConsolidationActionsService.ps1"
$source = Join-Path $parentPath $actionsArtifactName
#Invoke-Expression "$installActionsServicePath -NotifyPort $notifyPort -InstallRoot $installRoot -Name '$actionsName' -Source '$source'"

$installSiteSyncPath = Join-Path $currentPath "InstallConsolidationSiteSync.ps1"
$source = Join-Path $parentPath $siteSyncArtifactName
#Invoke-Expression "$installSiteSyncPath -Port $siteSyncPort -SiteName '$siteSyncName' -UseProductVersions `$$useProductVersions"

$installHighloadFrontPath = Join-Path $currentPath "InstallConsolidationHighloadFront.ps1"
$source = Join-Path $parentPath $highloadFrontArtifactName
#Invoke-Expression "$installHighloadFrontPath -Port $syncApiPort -SiteName '$syncApiName' -CanUpdate `$$true"
#Invoke-Expression "$installHighloadFrontPath -Port $searchApiPort -SiteName '$searchApiName' -CanUpdate `$$false"

$installWebApiPath = Join-Path $currentPath "InstallConsolidationWebApi.ps1"
$source = Join-Path $parentPath $webApiArtifactName
#Invoke-Expression "$installWebApiPath -Port $webApiPort -SiteName '$webApiName' -NotifyPort $notifyPort"

if (-not $customerLogin){
    $customerLogin = "${customerCode}_Login"
}
if (-not $customerPassword){

    $customerPassword = New-Guid
}

$installCustomerCodePath = Join-Path $currentPath "InstallConsolidationCustomerCode.ps1"
$params = "-DatabaseServer '$databaseServer' -TargetBackupPath '$targetBackupPath' -CustomerCode '$customerCode' -CustomerLogin '$customerLogin' -CustomerPassword '$customerPassword' -CurrentSqlPath '$currentSqlPath' -SiteSyncHost '$siteSyncHost' -SyncApiHost '$syncApiHost' -ElasticsearchHost '$elasticsearchHost' -AdminHost '$adminHost'"
if (-not [string]::IsNullOrEmpty($sourceBackupPath)) { $params = "$params -SourceBackupPath '$sourceBackupPath'" }
#Invoke-Expression "$installCustomerCodePath $params"