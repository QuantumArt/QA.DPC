<#
.SYNOPSIS
Installs reference front

.DESCRIPTION
Dpc.Front is a reference front created as web application. It contains published products which is used for QP8.Catalog internal purposes, 
such as reindexing ElasticSearch, tracking of publication history.

.EXAMPLE
  .\InstallConsolidationFront.ps1 -port 8013 -logPath 'C:\Logs'

.EXAMPLE
  .\InstallConsolidationFront.ps1 -port 8012 -siteName 'DPC.Front' -logPath 'C:\Logs' -useProductVersions $true 
#>
param(
    ## Dpc.Front site name
    [Parameter()]
    [String] $siteName ='Dpc.Front',
    ## Dpc.Front port
    [Parameter(Mandatory = $true)]
    [int] $port,
    ## Logs folder
    [Parameter(Mandatory = $true)]
    [String] $logPath,
    ## DPC instance name
    [Parameter()]
    [string] $instanceId = 'Dev',
    ## Use versions (publication history) or not
    [Parameter()]
    [bool] $useProductVersions = $false
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

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$parentPath = Split-Path -parent $currentPath
$sourcePath = Join-Path $parentPath "Front"

Copy-Item "$sourcePath\*" -Destination $sitePath -Force -Recurse

$nLogPath = Join-Path $sitePath "NLog.config"
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
$appsettingsPath = Join-Path $sitePath "appsettings.json"

$appsettings = Get-Content -Path $appsettingsPath  | ConvertFrom-Json
$appsettings.Data | Add-Member NoteProperty "UseProductVersions" $useProductVersions -Force
$appsettings.Data | Add-Member NoteProperty "InstanceId" $instanceId -Force

Set-ItemProperty $appsettingsPath -name IsReadOnly -value $false
$appsettings | ConvertTo-Json | Set-Content -Path $appsettingsPath

$p = Get-Item "IIS:\AppPools\$siteName" -ErrorAction SilentlyContinue

if (!$p) { 

    Write-Host "Creating application pool $siteName..."

    $p = New-Item –Path "IIS:\AppPools\$siteName"
    $p | Set-ItemProperty -Name managedRuntimeVersion -Value 'v4.0'

    Write-Host "Done"
}

$s = New-Item "IIS:\sites\$siteName" -bindings @{protocol="http";bindingInformation="*:${port}:"} -physicalPath $sitePath -type Site
$s | Set-ItemProperty -Name applicationPool -Value $siteName
