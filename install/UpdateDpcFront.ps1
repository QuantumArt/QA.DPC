param(
    [String] $source ='',
    [String] $siteName ='',
    [String] $dbname ='',
    [String] $dbServerName ='',
    [String] $sqlLogin ='',
    [String] $sqlPassword = '',
    [bool] $isJson = $true 
)


if (-not(Get-Module -Name WebAdministration)) {
    Import-Module WebAdministration
}

if ([string]::IsNullOrEmpty($source)) { throw "Parameter source is empty" }
if ([string]::IsNullOrEmpty($siteName)) { throw "Site name is empty" }
if ([string]::IsNullOrEmpty($dbname)) { throw "Parameter dbname is empty" }
if ([string]::IsNullOrEmpty($dbServerName)) { throw "Parameter dbServerName is empty" }
if ([string]::IsNullOrEmpty($sqlLogin)) { throw "Parameter sqlLogin is empty" }
if ([string]::IsNullOrEmpty($sqlPassword)) { throw "Parameter sqlPassword is empty" }

Invoke-Expression "ReplaceSite.ps1 -source '$source' -name '$siteName' -transformConfig `$false"

$s = Get-Item "IIS:\sites\$siteName" -ErrorAction SilentlyContinue
if (!$s) { throw "Site $siteName not exists"}
$sitePath = $s.PhysicalPath

$nLogPath = Join-Path $sitePath "NLogClient.config"
[xml]$nlog = Get-Content -Path $nLogPath
$var = $nlog.nlog.variable | where {$_.name -eq 'logDirectory'}
$var.value = "C:\Logs\$siteName"
Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$webConfigPath = Join-Path $sitePath "Web.config"
[xml]$web = Get-Content -Path $webConfigPath

$mainCnn = "Initial Catalog=$dbName;Data Source=$dbServerName;User ID=$sqlLogin;Password=$sqlPassword";
$var = $web.configuration.connectionStrings.add | where {$_.name -eq 'QA.Core.DPC.Integration.Properties.Settings.dpc_webConnectionString'}
$var.connectionString = $mainCnn

$var = $web.configuration.unity.container.register | where {$_.'type' -eq 'IProductSerializer'}
$var.mapTo = If ($isJson) {"JsonProductSerializer"} Else {"XmlProductSerializer"} 

$var = $web.configuration.'system.web'.compilation
$var.RemoveAttribute("debug")
Set-ItemProperty $webConfigPath -name IsReadOnly -value $false
$web.Save($webConfigPath)






