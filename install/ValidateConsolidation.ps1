<#
    .SYNOPSIS
    Проверка возможности установки компонент каталога

    .DESCRIPTION
    Проверяет:
    - Наличие нужного NETCore runtime
    - QP установлен
    - Доступность сервера баз данных
    - Доступность портов

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
    [String] $password
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

If ($databaseServer) {
    $requiredRuintime = '2.2.8'
    $actualRuntime = (Get-ChildItem (Get-Command dotnet).Path.Replace('dotnet.exe', 'shared\Microsoft.NETCore.App')).Name
    If ($actualRuntime -notcontains $requiredRuintime){ Throw "requared $requiredRuintime NETCore runtime" }

    $path = Get-QPConfigurationPath

    Try {
        $connectionString = Get-ConnectionString -server $databaseServer -user $login -pass $password
        $sqlConnection = New-Object System.Data.SqlClient.SqlConnection $connectionString
        $sqlConnection.Open()
    } Catch {
        Throw "SQL server $databaseServer is inaccessible"
    } Finally {
        $sqlConnection.Close()
    }
}

Test-Port -Port $actionsPort -Name "ActionsPort"
Test-Port -Port $notifyPort -Name "NotifyPort"
Test-Port -Port $frontPort -Name "FrontPort"
Test-Port -Port $syncApiPort -Name "SyncApiPort"
Test-Port -Port $searchApiPort -Name "SearchApiPort"
Test-Port -Port $webApiPort -Name "WebApiPort"