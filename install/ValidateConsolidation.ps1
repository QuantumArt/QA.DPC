<#
    .SYNOPSIS
    Validates whether catalog could be installed

    .DESCRIPTION
    Checks:
    - .NET Core Runtime 3.1.8 is installed
    - QP is installed
    - Database Server is avaialable for current user (or with specific credentials)
    - Ports are available

    .EXAMPLE
    .\ValidateConsolidation.ps1 -databaseServer 'dbhost'
    
    .EXAMPLE
    .\ValidateConsolidation.ps1 -actionsPort 8011 -notifyPort 8012 -frontPort 8013 -searchApiPort 8014 -syncApiPort 8015 -webApiPort 8016
#>
param(
    ## Порт DPC.ActionsService
    [Parameter()]
    [int] $actionsPort,
    ## Порт DPC.NotificationSender
    [Parameter()]
    [int] $notifyPort,
    ## Порт Dpc.Front
    [Parameter()]
    [int] $frontPort,
    ## Порт Dpc.SearchApi
    [Parameter()]
    [int] $searchApiPort,
    ## Порт Dpc.SyncApi
    [Parameter()]
    [int] $syncApiPort,
    ## Порт Dpc.WebApi
    [Parameter()]
    [int] $webApiPort,
    ## Сервер баз данных
    [Parameter()]
    [string] $databaseServer,
    ## Пользователь для сервера баз данных
    [Parameter()]
    [String] $login,
    ## Пароль для сервера баз данных
    [Parameter()]
    [String] $password,
    ## Database type: 0 - SQL Server, 1 - Postgres
    [Parameter()]
    [int] $dbType = 0
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

function Test-Port
{
    param(
        [int] $port,
        [string] $name
    )

    if ($port) {
        $connected = $false

        Try{
            $connected = (New-Object System.Net.Sockets.TcpClient('localhost', $port)).Connected
        } Catch { }

        If ($connected){
            Throw "$name $port is busy"
        }
    }
  
}

$useSqlPs = (-not(Get-Module -ListAvailable -Name SqlServer))
$moduleName = if ($useSqlPs) { "SqlPS" } else { "SqlServer" }
if (-not(Get-Module -Name $moduleName)) {
    Import-Module $moduleName
}

$requiredRuntime = '3.1.8'
  
Try {
    $actualRuntime = (Get-ChildItem (Get-Command dotnet).Path.Replace('dotnet.exe', 'shared\Microsoft.AspNetCore.App')).Name
} Catch {
    Write-Error $_.Exception
    Throw "Check ASP.NET Core runtime : failed"
} 
If ($actualRuntime -notcontains $requiredRuntime){ Throw "Check ASP.NET Core runtime $requiredRuntime : failed" }

If ($databaseServer) {
    Try {
        Execute-Sql -server $databaseServer -name $login -pass $password -query "select 1 as result" -dbType $dbType | Out-Null
    } Catch {
        Write-Error $_.Exception
        Throw "Test connection to  $databaseServer : failed"
    } 
}

Test-Port -Port $actionsPort -Name "ActionsPort"
Test-Port -Port $notifyPort -Name "NotifyPort"
Test-Port -Port $frontPort -Name "FrontPort"
Test-Port -Port $syncApiPort -Name "SyncApiPort"
Test-Port -Port $searchApiPort -Name "SearchApiPort"
Test-Port -Port $webApiPort -Name "WebApiPort"