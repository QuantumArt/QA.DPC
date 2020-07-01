﻿<#
.SYNOPSIS
Удаление ранее установленных компонент каталога

.DESCRIPTION
В процессе удуления
- Для всех сервисов каталога:
    • сервис останавливается
    • удаляются его файлы
- Для всех вею приложений каталога:
    • удаляется web приложение из IIS
    • удаляется его файлы
- Удаляется кастомер код каталога из QP

.EXAMPLE
  .\UninstallConsolidation.ps1 -customerCode 'catalog_consolidation' -installRoot 'C:\QA' -admin 'Dpc.Admin' -notificationSender 'DPC.NotificationSender' -actionsService 'DPC.ActionsService' -front 'Dpc.Front' -webApi 'Dpc.WebApi' -syncApi 'Dpc.SyncApi' -searchApi 'Dpc.SearchApi'
#>
param(
    ## Название DPC.NotificationSender
    [String] $notificationSender = 'DPC.NotificationSender',
    ## Название DPC.ActionsService
    [String] $actionsService = 'DPC.ActionsService',
    ## Название Dpc.Admin
    [String] $admin = 'Dpc.Admin',
    ## Название Dpc.Front
    [String] $front = 'Dpc.Front',
    ## Название Dpc.WebApi
    [String] $webApi = 'Dpc.WebApi',
    ## Название Dpc.SyncApi
    [String] $syncApi = 'Dpc.SyncApi',
    ## Название Dpc.SearchApi
    [String] $searchApi = 'Dpc.SearchApi',
    ## Название QP
    [string] $qpName = 'QP8',
    ## Путь к каталогу установки сервисов каталога
    [String] $installRoot = 'C:\QA',
    ## Кастомер код каталога
    [string] $customerCode
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}


if (-not(Get-Module -Name WebAdministration)) {
    Import-Module WebAdministration
}

function DeleteService    
{
   param(
     [string] $name,
     [string] $installRoot
   )

   $s = Get-Service $name -ErrorAction SilentlyContinue

    if ($s){
        if ( $s.Status -eq "Running"){
            Write-Host "* stoping $name"
            $s.Stop()
            $s.WaitForStatus("Stopped", "00:03:00")
            Start-Sleep -s 10
            Write-Host "* stopped"
        }

        $sobj = Get-WmiObject -Class Win32_Service -Filter "Name='$name'" 
        $sobj.Delete()    
        Write-Host "$name deleted"   
    }
    else{
        Write-Host "$name is not installed"
    }

    $path = Join-Path $installRoot $name

    if (Test-Path $path){
        Remove-Item $path -Recurse -Force
        Write-Host "$name files removed"
    }
    else{
        Write-Host "$name files is not exists"
    }
}

function DeleteSite
{
  param(
     [string] $qp,
     [string] $name
  )  

  if ($qp){
    $alias = "IIS:\sites\$qp\$name"
    $poolAlias = Get-Item "IIS:\AppPools\$qp.$name" -ErrorAction SilentlyContinue
  }
  else{
    $alias = "IIS:\sites\$name"
    $poolAlias = Get-Item "IIS:\AppPools\$name" -ErrorAction SilentlyContinue
  }
  
  $app = Get-Item $alias -ErrorAction SilentlyContinue
  if ($poolAlias) {
    $pool = Get-Item $poolAlias -ErrorAction SilentlyContinue
  }

  if ($app) {      
    $path =  $app.PhysicalPath

    Remove-Item $alias -Recurse -Force    
    Write-Host "$qp\$name deleted"

    if ($pool){
        $pool.Stop()
        Remove-Item $poolAlias -Recurse -Force
        Write-Host "pool $poolAlias deleted"
    }

    Start-Sleep -s 10

    if (Test-Path $path){
        Remove-Item $path -Recurse -Force
        Write-Host "$qp\$name files removed"
    }
  }
}

DeleteService -name $notificationSender -installRoot $installRoot
DeleteService -name $actionsService -installRoot $installRoot
DeleteSite -qp $qpName -name $admin
DeleteSite -name $front
DeleteSite -name $webApi
DeleteSite -name $syncApi
DeleteSite -name $searchApi

Remove-CustomerCode -CustomerCode $customerCode