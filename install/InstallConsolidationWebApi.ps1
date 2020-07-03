<#
.SYNOPSIS
Installs DPC.WebAPI

.DESCRIPTION
DPC.WebAPI provides program interface for executing CRUD-operations with products and calling DPC custom actions

.EXAMPLE
  .\InstallConsolidationWebApi.ps1 -port 8016 -notifyPort 8012 -siteName 'Dpc.WebApi' -logPath 'C:\Logs'

.EXAMPLE
   .\InstallConsolidationWebApi.ps1 -port 8016 -notifyPort 8012 -logPath 'C:\Logs'
#>
param(
    ## Dpc.WebApi site name
    [Parameter()]
    [String] $siteName ='Dpc.WebApi',
    ## Dpc.WebApi port
    [Parameter(Mandatory = $true)]
    [int] $port,
    ## Logs folder
    [Parameter(Mandatory = $true)]
    [String] $logPath,    
    ## DPC.NotificationSender port
    [Parameter(Mandatory = $true)]
    [int] $notifyPort
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition

. (Join-Path $currentPath "Modules\Get-SiteOrApplication.ps1")

$b = Get-WebBinding -Port $port
if ($b) { throw "Binding for port $port already exists"}

$s = Get-SiteOrApplication $siteName
if ($s) { throw "Site $siteName already exists"}

$def = Get-SiteOrApplication "Default Web Site"
if (!$def) { throw "Default Web Site doesn't exist"}

$root = $def.PhysicalPath -replace "%SystemDrive%",$env:SystemDrive
$sitePath = Join-Path $root $siteName
Write-Verbose $sitePath
New-Item -Path $sitePath -ItemType Directory -Force | Out-Null

$parentPath = Split-Path -parent $currentPath
$sourcePath = Join-Path $parentPath "WebApi"

Copy-Item "$sourcePath\*" -Destination $sitePath -Force -Recurse

$nLogPath = Join-Path $sitePath "NLogClient.config"

[xml]$nlog = Get-Content -Path $nLogPath

$nlog.nlog.internalLogFile = [string](Join-Path $logPath "internal-log.txt")

$node = $nlog.nlog.variable | Where-Object {$_.name -eq 'logDirectory'}
$node.value = [string](Join-Path $logPath $siteName)

$node = $nlog.nlog.rules.logger | Where-Object {$_.levels -eq 'Warn,Error,Fatal'}
$node.writeTo = "fileException"

$node = $nlog.nlog.rules.logger | Where-Object {$_.levels -eq 'Info'}
$node.writeTo = "fileInfo"

Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$appSettingsPath = Join-Path $sitePath "appsettings.json"
$json = Get-Content -Path $appSettingsPath | ConvertFrom-Json

$loader = $json.Loader
$loader.UseFileSizeService = $false

$integration = ($json | Get-Member "Integration")
if (!$integration) {
    $integration = New-Object PSObject
    $json | Add-Member NoteProperty "Integration" $integration
}

$integration | Add-Member NoteProperty "RestNotificationUrl" "http://${env:COMPUTERNAME}:$notifyPort" -Force

Set-ItemProperty $appSettingsPath -name IsReadOnly -value $false
$json | ConvertTo-Json | Out-File $appSettingsPath

$p = Get-Item "IIS:\AppPools\$siteName" -ErrorAction SilentlyContinue

if (!$p) { 

    Write-Host "Creating application pool $siteName..."

    $p = New-Item –Path "IIS:\AppPools\$siteName"
    $p | Set-ItemProperty -Name managedRuntimeVersion -Value 'v4.0'

    Write-Host "Done"
}

$s = New-Item "IIS:\sites\$siteName" -bindings @{protocol="http";bindingInformation="*:${port}:"} -physicalPath $sitePath -type Site
$s | Set-ItemProperty -Name applicationPool -Value $siteName
