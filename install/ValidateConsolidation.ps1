<#
.SYNOPSIS
Проверка возможности установки компонент каталога

.DESCRIPTION
Проверяет наличие нужного NETCore runtime и доступность портов

.EXAMPLE
  .\ValidateConsolidation.ps1 -notifyPort 8012 -siteSyncPort 8013 -searchApiPort 8014 -syncApiPort 8015 -webApiPort 8016
#>
param(
    ## Порт DPC.NotificationSender
    [Parameter(Mandatory = $true)]
    [int] $notifyPort,
    ## Порт Dpc.SiteSync
    [Parameter(Mandatory = $true)]
    [int] $siteSyncPort,
    ## Порт Dpc.SearchApi
    [Parameter(Mandatory = $true)]
    [int] $searchApiPort,
    ## Порт Dpc.SyncApi
    [Parameter(Mandatory = $true)]
    [int] $syncApiPort,
    ## Порт Dpc.WebApi
    [Parameter(Mandatory = $true)]
    [int] $webApiPort
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$requiredRuintime = '2.2.1'
$actualRuntime = (Get-ChildItem (Get-Command dotnet).Path.Replace('dotnet.exe', 'shared\Microsoft.NETCore.App')).Name
If ($actualRuntime -notcontains $requiredRuintime){ Throw "requared $requiredRuintime NETCore runtime" }

If ((Test-NetConnection -Port $notifyPort -WarningAction SilentlyContinue).TcpTestSucceeded){ Throw "NotifyPort $notifyPort is busy" }
If ((Test-NetConnection -Port $siteSyncPort -WarningAction SilentlyContinue).TcpTestSucceeded){ Throw "SiteSyncPort $siteSyncPort is busy" }
If ((Test-NetConnection -Port $searchApiPort -WarningAction SilentlyContinue).TcpTestSucceeded){ Throw "SearchApiPort $searchApiPort is busy" }
If ((Test-NetConnection -Port $webApiPort -WarningAction SilentlyContinue).TcpTestSucceeded){ Throw "WebApiPort $webApiPort is busy" }


