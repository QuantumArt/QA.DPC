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
  .\ValidateConsolidation.ps1 -notifyPort 8012 -siteSyncPort 8013 -searchApiPort 8014 -syncApiPort 8015 -webApiPort 8016
#>
param(
    ## Порт DPC.NotificationSender
    [Parameter()]
    [int] $notifyPort,
    ## Порт Dpc.SiteSync
    [Parameter()]
    [int] $siteSyncPort,
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

  if ($port){
    $connected = $false

    Try{
      $connected = (New-Object System.Net.Sockets.TcpClient('localhost', $port)).Connected
    }Catch{    
    }

    If ($connected){
      Throw "$name $port is busy"
    }
  }
  
}

Import-Module -Name SqlServer

If ($databaseServer){
  $requiredRuintime = '2.2.1'
  $actualRuntime = (Get-ChildItem (Get-Command dotnet).Path.Replace('dotnet.exe', 'shared\Microsoft.NETCore.App')).Name
  If ($actualRuntime -notcontains $requiredRuintime){ Throw "requared $requiredRuintime NETCore runtime" }

  $path = Get-QPConfigurationPath

  Try {
    $connectionString = Get-ConnectionString -ServerInstance $databaseServer -Username $login -Password $password
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $sqlConnection.Open()
  } Catch {
    Throw "SQL server $databaseServer is inaccessible"
  } Finally {
    $sqlConnection.Close()
  }
}

Test-Port -Port $notifyPort -Name "NotifyPort"
Test-Port -Port $siteSyncPort -Name "SiteSyncPort"
Test-Port -Port $searchApiPort -Name "SearchApiPort"
Test-Port -Port $webApiPort -Name "WebApiPort"