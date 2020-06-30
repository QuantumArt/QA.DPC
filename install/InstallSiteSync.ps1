param(
    [String] $siteName ='',
    [String] $port ='',
    [String] $siteDbName = '',
    [String] $dbServerName ='',
    [String] $dbLogin ='',
    [String] $dbPassword = '',
    [String] $sqlAdminLogin ='',
    [String] $sqlAdminPassword = '',
    [String] $productSerializer = ''
)

function Read-Or-Default([String] $value, [String] $message, [String] $defaultValue)
{
    $fullMessage = $message
    if (-not([string]::IsNullOrEmpty($defaultValue)))  { $fullMessage = $fullMessage + "(default name - $defaultValue)" }
    if ([string]::IsNullOrEmpty($value)) { $value = Read-Host $fullMessage  }
    if ([string]::IsNullOrEmpty($value)) { $value = $defaultValue }
    return $value
}

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}


if (-not(Get-Module -Name WebAdministration)) {
    Import-Module WebAdministration
}

$siteName = Read-Or-Default $siteName "Please enter site name for DPC Front" "DPC.Front"
$port = Read-Or-Default $port "Please enter port for DPC Front binding" "95"

$intPort = 0
if (-not([int32]::TryParse($port, [ref] $intPort))) { throw "Incorrect port: $port" }
$b = Get-WebBinding -Port $port
if ($b) { throw "Binding for port $port already exists"}

$s = Get-Item "IIS:\sites\$siteName" -ErrorAction SilentlyContinue
if ($s) { throw "Site $siteName already exists"}

$def = Get-Item "IIS:\sites\Default Web Site" -ErrorAction SilentlyContinue
if (!$def) { throw "Default Web Site doesn't exist"}

$root = $def.PhysicalPath -replace "%SystemDrive%",$env:SystemDrive
$sitePath = Join-Path $root $siteName
Write-Verbose $sitePath
New-Item -Path $sitePath -ItemType Directory -Force

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$parentPath = Split-Path -parent $currentPath
$adminPath = Join-Path $parentPath "Front"

Copy-Item "$adminPath\*" -Destination $sitePath -Force -Recurse

$nLogPath = Join-Path $sitePath "NLogClient.config"
[xml]$nlog = Get-Content -Path $nLogPath
$var = $nlog.nlog.variable | where {$_.name -eq 'logDirectory'}
$var.value = "C:\Logs\" + $siteName
Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$sqlAdminLogin = Read-Or-Default $sqlAdminLogin "Please enter admin login name for creating databases"
$sqlAdminPassword = Read-Or-Default $sqlAdminPassword "Please enter admin password for creating databases"
$dbServerName = Read-Or-Default $dbServerName "Please enter db server name to connect"
$dbLogin = Read-Or-Default $dbLogin "Please enter login name to connect databases"
$dbPassword = Read-Or-Default $dbPassword "Please enter password to connect databases"
$siteDbName = Read-Or-Default $siteDbName "Please enter Front db name to reference" "Front"
$productSerializer = Read-Or-Default $productSerializer "Please enter product serializer: XmlProductSerializer or JsonProductSerializer" "XmlProductSerializer"

$webConfigPath = Join-Path $sitePath "Web.config"
[xml]$web = Get-Content -Path $webConfigPath
$var = $web.configuration.connectionStrings.add | where {$_.name -eq 'QA.Core.DPC.Integration.Properties.Settings.dpc_webConnectionString'}
$var.connectionString = "Initial Catalog=$siteDbName;Data Source=$dbServerName;User ID=$dbLogin;Password=$dbPassword"
$var2 = $web.configuration.unity.container.register | where {$_.type -eq 'IProductSerializer'}
$var2.mapTo = $productSerializer
Set-ItemProperty $webConfigPath -name IsReadOnly -value $false
$web.Save($webConfigPath)

Copy-Item $webConfigPath ($webConfigPath -replace ".config", ".config2")
Copy-Item $nLogPath ($nLogPath -replace ".config", ".config2")
Remove-Item -Path (Join-Path $sitePath "*.config") -Force
Get-ChildItem (Join-Path $sitePath "*.config2") | Rename-Item -newname { $_.name -replace '\.config2','.config' }

$p = Get-Item "IIS:\AppPools\$siteName" -ErrorAction SilentlyContinue

if (!$p) { 

    Write-Host "Creating application pool $siteName..."

    $p = New-Item –Path "IIS:\AppPools\$siteName"
    $p | Set-ItemProperty -Name managedRuntimeVersion -Value 'v4.0'

    Write-Host "Done"
}

$s = New-Item "IIS:\sites\$siteName" -bindings @{protocol="http";bindingInformation="*:${port}:"} -physicalPath $sitePath -type Site
$s | Set-ItemProperty -Name applicationPool -Value $siteName

if ($sqlAdminLogin) { 

Invoke-Expression "CreateLogin.ps1 -Login '$dbLogin' -Password '$dbPassword' -DbServerName '$dbServerName' -AdminLogin '$sqlAdminLogin' -AdminPassword '$sqlAdminPassword'"

$scriptPath = Join-Path $currentPath "dpc_SiteSync.sql"
Invoke-Expression "CreateDb.ps1 -ScriptPath '$scriptPath' -Login '$dbLogin' -Password '$dbPassword' -DbName '$siteDbName' -DbServerName '$dbServerName' -AdminLogin '$sqlAdminLogin' -AdminPassword '$sqlAdminPassword' "

} 