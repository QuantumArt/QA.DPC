param(
    [Parameter()]
    [bool] $cleanUp = $true,
    [Parameter(Mandatory = $true)]
    [string] $databaseServer,
    [Parameter()]
    [ValidateScript({ if (-not [string]::IsNullOrEmpty($_)) { Test-Path $_}})]
    [string] $sourceBackupPath,
    [Parameter(Mandatory = $true)]
    [string] $targetBackupPath,
    [Parameter(Mandatory = $true)]
    [string] $customerLogin,
    [Parameter(Mandatory = $true)]
    [string] $customerPassword,
    [Parameter(Mandatory = $true)]
    [ValidateScript({ if (-not [string]::IsNullOrEmpty($_)) { Test-Path $_}})]
    [string] $currentSqlPath,
    [Parameter(Mandatory = $true)]
    [string] $customerCode,    
    [Parameter(Mandatory = $true)]
    [int] $notifyPort,
    [Parameter(Mandatory = $true)]
    [int] $siteSyncPort,
    [Parameter(Mandatory = $true)]
    [int] $searchApiPort,
    [Parameter(Mandatory = $true)]
    [int] $syncApiPort,
    [Parameter(Mandatory = $true)]
    [int] $webApiPort,
    [Parameter(Mandatory = $true)]
    [int] $backendPort,
    [Parameter(Mandatory = $true)]
    [string] $elasticsearchHost,
    [Parameter(Mandatory = $true)]
    [ValidateScript({ if (-not [string]::IsNullOrEmpty($_)) { Test-Path $_}})]
    [string] $installRoot,
    [Parameter()]
    [bool] $useProductVersions = $false,
    [Parameter()]
    [string] $qpName = 'QP8',
    [Parameter()]
    [string] $adminName = 'Dpc.Admin',
    [Parameter()]
    [string] $actionsName = 'DPC.ActionsService',
    [Parameter()]
    [string] $searchApiName = 'Dpc.SearchApi',
    [Parameter()]
    [string] $notificationsName = 'DPC.NotificationSender',
    [Parameter()]
    [string] $siteSyncName = 'Dpc.SiteSync',
    [Parameter()]
    [string] $webApiName = 'Dpc.WebApi',
    [Parameter()]
    [string] $syncApiName = 'Dpc.SyncApi'
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$requiredRuintime = '2.2.1'
$actualRuntime = (dir (Get-Command dotnet).Path.Replace('dotnet.exe', 'shared\Microsoft.NETCore.App')).Name
If ($actualRuntime -ne $requiredRuintime){ Throw "requared $requiredRuintime NETCore runtime" }

$actionsArtifactName = 'ActionsRunner' 
$adminArtifactName = 'Admin'
$notificationsArtifactName = 'NotificationsSender'
$installArtifactName = 'Install'
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

if ($cleanUp){
    $uninstallPath = Join-Path $currentPath "UninstallConsolidation.ps1"
    $params = "-CustomerCode '$customerCode' -InstallRoot '$installRoot' -Admin '$adminName' -NotificationSender '$notificationsName' -ActionsService '$actionsName' -SiteSync '$siteSyncName' -WebApi '$webApiName' -SyncApi '$syncApiName' -SearchApi '$searchApiName'"
    Invoke-Expression "$uninstallPath $params"
}

$installAdminiPath = Join-Path $currentPath "InstallConsolidationAdmin.ps1"
Invoke-Expression "$installAdminiPath -NotifyPort $notifyPort -SyncPort $syncApiPort -Admin '$adminName' -Qp '$qpName'"

$installNotificationSenderPath = Join-Path $currentPath "InstallConsolidationNotificationSender.ps1"
$source = Join-Path $parentPath $notificationsArtifactName
Invoke-Expression "$installNotificationSenderPath -NotifyPort $notifyPort -InstallRoot $installRoot -Name '$notificationsName' -Source '$source'"

$installActionsServicePath = Join-Path $currentPath "InstallConsolidationActionsService.ps1"
$source = Join-Path $parentPath $actionsArtifactName
Invoke-Expression "$installActionsServicePath -NotifyPort $notifyPort -InstallRoot $installRoot -Name '$actionsName' -Source '$source'"

$installSiteSyncPath = Join-Path $currentPath "InstallConsolidationSiteSync.ps1"
$source = Join-Path $parentPath $siteSyncArtifactName
Invoke-Expression "$installSiteSyncPath -Port $siteSyncPort -SiteName '$siteSyncName' -UseProductVersions `$$useProductVersions"

$installHighloadFrontPath = Join-Path $currentPath "InstallConsolidationHighloadFront.ps1"
$source = Join-Path $parentPath $highloadFrontArtifactName
Invoke-Expression "$installHighloadFrontPath -Port $syncApiPort -SiteName '$syncApiName' -CanUpdate `$$true"
Invoke-Expression "$installHighloadFrontPath -Port $searchApiPort -SiteName '$searchApiName' -CanUpdate `$$false"

$installWebApiPath = Join-Path $currentPath "InstallConsolidationWebApi.ps1"
$source = Join-Path $parentPath $webApiArtifactName
Invoke-Expression "$installWebApiPath -Port $webApiPort -SiteName '$webApiName' -NotifyPort $notifyPort"

$installCustomerCodePath = Join-Path $currentPath "InstallConsolidationCustomerCode.ps1"
$params = "-DatabaseServer '$databaseServer' -TargetBackupPath '$targetBackupPath' -CustomerCode '$customerCode' -CustomerLogin '$customerLogin' -CustomerPassword '$customerPassword' -CurrentSqlPath '$currentSqlPath' -SiteSyncHost '$siteSyncHost' -SyncApiHost '$syncApiHost' -ElasticsearchHost '$elasticsearchHost' -AdminHost '$adminHost'"
if (-not [string]::IsNullOrEmpty($sourceBackupPath)) { $params = "$params -SourceBackupPath '$sourceBackupPath'" }
Invoke-Expression "$installCustomerCodePath $params"
