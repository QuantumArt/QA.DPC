param(
    [String] $siteName ='',
    [String] $port ='',
    [String] $catalogDbName = '',
    [String] $dbServerName ='',
    [String] $dbLogin ='',
    [String] $dbPassword = '',
    [String] $notifyPort = '',
    [String] $machine = ''
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

Import-Module WebAdministration

$siteName = Read-Or-Default $siteName "Please enter site name for DPC API" "DPC.API"
$port = Read-Or-Default $port "Please enter port for DPC API binding" "93"

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
$adminPath = Join-Path $parentPath "WebAPI"

Copy-Item "$adminPath\*" -Destination $sitePath -Force -Recurse

$projectName = "QA.ProductCatalog.WebApi"
$nLogPath = Join-Path $sitePath "NLogClient.config"
[xml]$nlog = Get-Content -Path $nLogPath
$var = $nlog.nlog.targets.target | where {$_.name -eq 'fileinfo'}
$var2 = $nlog.nlog.targets.target | where {$_.name -eq 'fileexception'}
$var.fileName = $var.fileName -Replace $projectName, $siteName
$var.archiveFileName = $var.archiveFileName -Replace $projectName, $siteName
$var2.fileName = $var2.fileName -Replace $projectName, $siteName
$var2.archiveFileName = $var2.archiveFileName -Replace $projectName, $siteName
Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$dbServerName = Read-Or-Default $dbServerName "Please enter db server name to connect"
$dbLogin = Read-Or-Default $dbLogin "Please enter login name to connect databases"
$dbPassword = Read-Or-Default $dbPassword "Please enter password to connect databases"
$catalogDbName = Read-Or-Default $catalogDbName "Please enter catalog db name to reference" "catalog"
$notifyPort = Read-Or-Default $notifyPort "Please enter port for notification sender" "8011"
$machine = Read-Or-Default $machine "Please enter machine name for services" "${env:COMPUTERNAME}"
$url = "http://${machine}:$notifyPort/DpcNotificationService"

$webConfigPath = Join-Path $sitePath "Web.config"
[xml]$web = Get-Content -Path $webConfigPath
$var = $web.configuration.connectionStrings.add | where {$_.name -eq 'qp_database'}
$var2 = $web.configuration.'system.serviceModel'.client.endpoint
$var.connectionString = "Initial Catalog=$catalogDbName;Data Source=$dbServerName;User ID=$dbLogin;Password=$dbPassword"
$var2.address = $url
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