<#
.SYNOPSIS
Installs HighloadFront

.DESCRIPTION
HighloadFront is a web application for working with Elasticsearch indices.
Used in two possible options:
- Dpc.SyncApi: For updating Elasticsearch indicies
- Dpc.SearchApi: For searching into Elasticsearch indices

.EXAMPLE
  .\InstallConsolidationHighloadFront.ps1 -port 8015 -siteName 'Dpc.SyncApi' -canUpdate $true -logPath 'C:\Logs' -timeout 45 

.EXAMPLE
   .\InstallConsolidationHighloadFront.ps1 -port 8014 -siteName 'Dpc.SearchApi' -canUpdate $false -logPath 'C:\Logs' -sonicElasticStore "{""ValueSeparator"":""{or}"";""WildcardStarMark"":""{any}""} -Verbose
#>
param(
    ## HighloadFront site name
    [Parameter(Mandatory = $true)]
    [String] $siteName,
    ## HighloadFront port
    [Parameter(Mandatory = $true)]
    [int] $port,
    ## Flag which allows updating ElasticSearch indices
    [Parameter(Mandatory = $true)]
    [bool] $canUpdate,
    ## Elasticsearch conneciton timeout (sec)
    [Parameter()]
    [int] $timeout = 60,
    ## Logs folder
    [Parameter(Mandatory = $true)]
    [String] $logPath,
    ## DPC instance name
    [Parameter()]
    [string] $instanceId = 'Dev',
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
$sourcePath = Join-Path $parentPath "HighloadFront"

Write-Host "Copying files from $sourcePath to $sitePath..."
Copy-Item "$sourcePath\*" -Destination $sitePath -Force -Recurse
Write-Host "Done"

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

$json.Data | Add-Member NoteProperty "ElasticTimeout" $timeout -Force
$json.Data | Add-Member NoteProperty "InstanceId" $instanceId -Force
$json.Data.CanUpdate = $canUpdate

if ($elasticStoreOptions) {
    $elasticStoreJson = $elasticStoreOptions | ConvertFrom-Json
    if ($elasticStoreJson) {
        $elasticStoreJson.psobject.Properties | % {
            $json.SonicElasticStore | Add-Member -MemberType $_.MemberType -Name $_.Name -Value $_.Value -Force
        }
    } else {
        Write-Verbose "Invalid `$elasticStoreOptions json, merge skipped"
    }
}

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
