<#
.SYNOPSIS
Installs QP8.Catalog administration module

.DESCRIPTION
QP8.Catalog administration module contains the following components:
- Custom Actions for product processing
- Remote validators for products

.EXAMPLE
  .\InstallConsolidationAdmin.ps1 -notifyPort 8012 -syncPort 8013

.EXAMPLE
  .\InstallConsolidationAdmin.ps1 -notifyPort 8012 -syncPort 8013 -admin 'Dpc.Admin' -qp 'QP8' -backend 'Backend'
#>
param(
    ## QP site name
    [Parameter()]
    [String] $qp ='QP8',
    ## QP application name
    [Parameter()]
    [String] $backend ='Backend',
    ## Dpc.Admin application name
    [Parameter()]
    [String] $admin ='Dpc.Admin',
    ## DPC.NotificationSender port
    [Parameter(Mandatory = $true)]
    [int] $notifyPort,
    ## Dpc.Sync port
    [Parameter(Mandatory = $true)]
    [int] $syncApiPort,
    ## Logs folder
    [Parameter(Mandatory = $true)]
    [String] $logPath,
    ## Extra Validation Libraries
    [Parameter()]
    [String] $libraries = ''

)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition

. (Join-Path $currentPath "Modules\Get-SiteOrApplication.ps1")

$qpApp = Get-SiteOrApplication -name $qp 
if (!$qpApp) { throw "QP application $qp is not exists"}

$adminApp = Get-SiteOrApplication -name $qp -application $admin 
if ($adminApp) { throw "Admin application $admin is exists"}


$parentPath = Split-Path -parent $currentPath
$sourcePath = Join-Path $parentPath "Admin"

$backendApp = Get-SiteOrApplication -name $qp -application $backend 
if (!$backendApp)
{
    $backendApp = $qpApp
}
$root = Split-Path -parent $backendApp.PhysicalPath

$adminPath = Join-Path $root $admin
New-Item -Path $adminPath -ItemType Directory -Force

Copy-Item "$sourcePath\*" -Destination $adminPath -Force -Recurse

$nLogPath = Join-Path $adminPath "NLogClient.config"
[xml]$nlog = Get-Content -Path $nLogPath

$nlog.nlog.internalLogFile = [string](Join-Path $logPath "internal-log.txt")

$var = $nlog.nlog.variable | Where-Object {$_.name -eq 'logDirectory'}
$var.value = [string](Join-Path $logPath "$qp.$admin")

$var2 = $nlog.nlog.rules.logger | Where-Object {$_.levels -eq 'Warn,Error,Fatal'}
$var2.writeTo = "fileException"

$var3 = $nlog.nlog.rules.logger | Where-Object {$_.levels -eq 'Info'}
$var3.writeTo = "fileInfo"

Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$appSettingsPath = Join-Path $adminPath "appsettings.json"
$json = Get-Content -Path $appSettingsPath | ConvertFrom-Json

$loader = $json.Loader
$loader.UseFileSizeService = $false

$integration = ($json | Get-Member "Integration")
if (!$integration) {
    $integration = New-Object PSObject
    $json | Add-Member NoteProperty "Integration" $integration
}

$integration | Add-Member NoteProperty "RestNotificationUrl" "http://${env:COMPUTERNAME}:$notifyPort" -Force
$integration | Add-Member NoteProperty "HighloadFrontSyncUrl" "http://${env:COMPUTERNAME}:$syncApiPort" -Force
if ($libraries) {
    $libarr = @()
    foreach ($lib in $libraries.Split(',')) { $libarr += $lib.Trim()}
    $integration | Add-Member NoteProperty "ExtraValidationLibraries" $libarr -Force
}

Set-ItemProperty $appSettingsPath -name IsReadOnly -value $false
$json | ConvertTo-Json | Out-File $appSettingsPath

$adminPool = Get-Item "IIS:\AppPools\$qp.$admin" -ErrorAction SilentlyContinue

if (!$adminPool) { 

    Write-Host "Creating application pool $qp.$admin..."

    $adminPool = New-Item –Path "IIS:\AppPools\$qp.$admin"
    $adminPool | Set-ItemProperty -Name managedRuntimeVersion -Value 'v4.0'

    Write-Host "Done"
}

New-Item "IIS:\sites\$qp\$admin" -physicalPath $adminPath -applicationPool "$qp.$admin" -type Application


