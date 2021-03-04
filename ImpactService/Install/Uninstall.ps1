<#
.SYNOPSIS
Deletes Impact module for QP8.ProductCatalog

.DESCRIPTION
While deleting
  - application is removed from IIS
  - application files are removed

.EXAMPLE
  .\Uninstall.ps1 -impactName 'Dpc.Impact' 
#>
param(
    ## Impact name
    [String] $impactName = 'Dpc.Impact'
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


function DeleteSite
{
    param(
        [string] $qp,
        [string] $name
    )  

    $alias = if ($qp) { "IIS:\sites\$qp\$name" } else { "IIS:\sites\$name" }
    $displayName = if ($qp) { "application $name for site $qp" } else { "Site $name" }

    $app = Get-Item $alias -ErrorAction SilentlyContinue

    if ($app) {      
        $path =  $app.PhysicalPath
        $poolName = $app.applicationPool

        if ($poolName) {
            Stop-AppPool $poolName | Out-Null
            Remove-Item "IIS:\AppPools\$poolName" -Recurse -Force
            Write-Host "pool $poolName deleted"
        }

        Remove-Item $alias -Recurse -Force    
        Write-Host "$displayName deleted"

        if (Test-Path $path){
            Remove-Item $path -Recurse -Force
            Write-Host "files of $displayName deleted"
        }
    }
}

function Stop-AppPool
{
    param(
        [Parameter(Mandatory = $true)]
        [String] $AppPoolName
    )

    $s = Get-Item "IIS:\AppPools\$AppPoolName" -ErrorAction SilentlyContinue

    if ($s -and $s.State -ne "Stopped")
    {
        Write-Verbose "Stopping AppPool $AppPoolName..." 
        $s.Stop()
        $endTime = $(get-date).AddMinutes('1')
        while($(get-date) -lt $endtime)
        {
            Start-Sleep -Seconds 1
            if ($s.State -ne "Stopping")
            {
                if ($s.State -eq "Stopped") {
                    Write-Verbose "Stopped" 
                }
                break
            }
        }
    }

    return $s.State -eq "Stopped"
}


DeleteSite -name $impactName
