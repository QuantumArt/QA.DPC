<#
.SYNOPSIS
Installs DPC.Impact

.DESCRIPTION
DPC.Impact is a web service which uses product information from ElasticSearch and returns product calculator data.

.EXAMPLE
  .\InstallImpact.ps1 -port 8033 -logPath 'C:\Logs' -elasticUrl 'http://elastic01:9200' -liveIndexName 'products' -stageIndexName 'products_stage'

.EXAMPLE
 .\InstallImpact.ps1 -port 8032 -siteName 'DPC.Impact' -logPath 'C:\Logs' -elasticUrl 'http://elastic01:9200;http://elastic02:9200' -liveIndexName 'products' -stageIndexName 'products_stage'
#>
param(
    ## Impact site name
    [Parameter()]
    [String] $siteName = 'Dpc.Impact',
    ## Impact port
    [Parameter(Mandatory = $true)]
    [int] $port,
    ## ElasticSearch URL
    [Parameter(Mandatory = $true)]
    [string] $elasticUrl,
    ## Elasticsearch conneciton timeout (sec)
    [Parameter()]
    [int] $timeout = 60,
    ## Logs folder
    [Parameter(Mandatory = $true)]
    [String] $logPath,
    ## Live index name
    [Parameter(Mandatory = $true)]
    [String] $liveIndexName,
    ## Stage index name
    [Parameter(Mandatory = $true)]
    [String] $stageIndexName,
    ## Extra calculation libraries
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
$sourcePath = Join-Path $parentPath "Impact"

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

$node = $nlog.nlog.rules.logger | Where-Object {$_.levels -eq 'Trace'}
$node.removeAttribute("writeTo")

Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$appSettingsPath = Join-Path $sitePath "appsettings.json"
$json = Get-Content -Path $appSettingsPath | ConvertFrom-Json

$json.ElasticBaseAddress = $elasticUrl
$json.ElasticIndexes = @()
$json.ElasticIndexes += (New-Object PSObject -Property @{Name=$liveIndexName; State="live"; Language="invariant" })
$json.ElasticIndexes += (New-Object PSObject -Property @{Name=$stageIndexName; State="stage"; Language="invariant" })

if ($libraries) {
    $libarr = @()
    foreach ($lib in $libraries.Split(',')) { $libarr += $lib.Trim()}
    $integration | Add-Member NoteProperty "ExtraLibraries" $libarr -Force
}

$json | Add-Member NoteProperty "HttpTimeout" $timeout -Force

Set-ItemProperty $appsettingsPath -name IsReadOnly -value $false
$json | ConvertTo-Json -Depth 5 | Set-Content -Path $appsettingsPath

$p = Get-Item "IIS:\AppPools\$siteName" -ErrorAction SilentlyContinue

if (!$p) { 

    Write-Host "Creating application pool $siteName..."

    $p = New-Item –Path "IIS:\AppPools\$siteName"
    $p | Set-ItemProperty -Name managedRuntimeVersion -Value 'v4.0'

    Write-Host "Done"
}

$s = New-Item "IIS:\sites\$siteName" -bindings @{protocol="http";bindingInformation="*:${port}:"} -physicalPath $sitePath -type Site
$s | Set-ItemProperty -Name applicationPool -Value $siteName
