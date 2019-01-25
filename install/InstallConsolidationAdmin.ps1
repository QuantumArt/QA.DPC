param(
    [String] $qp ='QP8',
    [String] $backend ='Backend',
    [String] $admin ='Dpc.Admin',
    [String] $notifyPort = '8013',
    [String] $syncPort = '8012'
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

Import-Module WebAdministration

$qpApp = Get-Item "IIS:\sites\$qp" -ErrorAction SilentlyContinue
if (!$qpApp) { throw "QP application $qp is not exists"}

$adminApp = Get-Item "IIS:\sites\$qp\$admin" -ErrorAction SilentlyContinue
if ($adminApp) { throw "Admin application $admin is exists"}

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$parentPath = Split-Path -parent $currentPath
$sourcePath = Join-Path $parentPath "Admin"

$backendPath = (Get-SiteOrApplication -name $qp -application $backend -Verbose).PhysicalPath
$root = Split-Path -parent $backendPath
$adminPath = Join-Path $root $admin
New-Item -Path $adminPath -ItemType Directory -Force

Copy-Item "$sourcePath\*" -Destination $adminPath -Force -Recurse

$projectName = "QA.ProductCatalog.Admin.WebApp"
$nLogPath = Join-Path $adminPath "NLogClient.config"
[xml]$nlog = Get-Content -Path $nLogPath
$var = $nlog.nlog.targets.target | Where-Object {$_.name -eq 'fileinfo'}
$var2 = $nlog.nlog.targets.target | Where-Object {$_.name -eq 'fileexception'}

$var.fileName = $var.fileName -Replace $projectName, "$qp.$admin"
$var.archiveFileName = $var.archiveFileName -Replace $projectName, "$qp.$admin"
$var2.fileName = $var2.fileName -Replace $projectName, "$qp.$admin"
$var2.archiveFileName = $var2.archiveFileName -Replace $projectName, "$qp.$admin"

$var3 = $nlog.nlog.targets.target | Where-Object {$_.name -eq 'debug'}
if ($var3)
{
    $var3.ParentNode.RemoveChild($var3)
}
$var4 = $nlog.nlog.rules.logger | Where-Object {$_.level -eq 'Debug'}
$var4.writeTo = "fileInfo"

Set-ItemProperty $nLogPath -name IsReadOnly -value $false
$nlog.Save($nLogPath)

$webConfigPath = Join-Path $adminPath "Web.config"
[xml]$web = Get-Content -Path $webConfigPath

$syncApi = $web.configuration.appSettings.add | Where-Object {$_.key -eq 'HighloadFront.SyncApi'}
if ($syncApi){
    $web.configuration.appSettings.RemoveChild($syncApi)
}

$warmUp = $web.configuration.appSettings.add | Where-Object {$_.key -eq 'LoaderWarmUpProductId'}
if ($warmUp){
    $web.configuration.appSettings.RemoveChild($warmUp)
}

$qpMode = $web.CreateElement('add')
$qpMode.SetAttribute('key', 'QPMode')
$qpMode.SetAttribute('value', 'true')
$web.configuration.appSettings.AppendChild($qpMode)

$syncApi = $web.CreateElement('add')
$syncApi.SetAttribute('key', 'HighloadFront.SyncApi')
$syncApi.SetAttribute('value', "http://${env:COMPUTERNAME}:$syncPort")
$web.configuration.appSettings.AppendChild($syncApi)

$web.configuration.RemoveChild($web.configuration.connectionStrings)

$container = $web.configuration.unity.container
$productControlProvider = $container.register | Where-Object {$_.type -eq 'IProductControlProvider'}
$productControlProvider.mapTo = "ContentBasedProductControlProvider";
$settingsService = $container.register | Where-Object {$_.type -eq 'ISettingsService'}
$settingsService.mapTo = "SettingsFromQpService";
$articleFormatter = $container.register | Where-Object {$_.type -eq 'IArticleFormatter'}
$articleFormatter.mapTo = "JsonProductFormatter";

$endpoint = $web.configuration.'system.serviceModel'.client.endpoint
$endpoint.address = "http://${env:COMPUTERNAME}:$notifyPort/DpcNotificationService"

Set-ItemProperty $webConfigPath -name IsReadOnly -value $false
$web.Save($webConfigPath)

Copy-Item $webConfigPath ($webConfigPath -replace ".config", ".config2")
Copy-Item $nLogPath ($nLogPath -replace ".config", ".config2")
Remove-Item -Path (Join-Path $adminPath "*.config") -Force
Get-ChildItem (Join-Path $adminPath "*.config2") | Rename-Item -newname { $_.name -replace '\.config2','.config' }

$adminPool = Get-Item "IIS:\AppPools\$qp.$admin" -ErrorAction SilentlyContinue

if (!$adminPool) { 

    Write-Host "Creating application pool $qp.$admin..."

    $adminPool = New-Item –Path "IIS:\AppPools\$qp.$admin"
    $adminPool | Set-ItemProperty -Name managedRuntimeVersion -Value 'v4.0'

    Write-Host "Done"
}

New-Item "IIS:\sites\$qp\$admin" -physicalPath $adminPath -applicationPool "$qp.$admin" -type Application


