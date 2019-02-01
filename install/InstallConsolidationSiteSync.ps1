param(
    [String] $siteName ='Dpc.SiteSync',
    [int] $port = 92
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

Import-Module WebAdministration

$b = Get-WebBinding -Port $port
if ($b) { throw "Binding for port $port already exists"}

$s = Get-Item "IIS:\sites\$siteName" -ErrorAction SilentlyContinue
if ($s) { throw "Site $siteName already exists"}

$def = Get-Item "IIS:\sites\Default Web Site" -ErrorAction SilentlyContinue
if (!$def) { throw "Default Web Site doesn't exist"}

$root = $def.PhysicalPath -replace "%SystemDrive%",$env:SystemDrive
$sitePath = Join-Path $root $siteName
Write-Output $sitePath
New-Item -Path $sitePath -ItemType Directory -Force

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$parentPath = Split-Path -parent $currentPath
$adminPath = Join-Path $parentPath "Front"

Copy-Item "$adminPath\*" -Destination $sitePath -Force -Recurse

$nLogPath = Join-Path $sitePath "NLogClient.config"
[xml]$nlog = Get-Content -Path $nLogPath
$var = $nlog.nlog.variable | Where-Object {$_.name -eq 'logDirectory'}
$var.value = "C:\Logs\" + $siteName
Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$webConfigPath = Join-Path $sitePath "Web.config"
[xml]$web = Get-Content -Path $webConfigPath

$web.configuration.RemoveChild($web.configuration.connectionStrings)

$qpMode = $web.CreateElement('add')
$qpMode.SetAttribute('key', 'QPMode')
$qpMode.SetAttribute('value', 'true')
$web.configuration.appSettings.AppendChild($qpMode)

$productSerializer = $web.configuration.unity.container.register | Where-Object {$_.type -eq 'IProductSerializer'}
$productSerializer.mapTo = "JsonProductSerializer"

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
