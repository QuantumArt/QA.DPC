<#
    .SYNOPSIS
    Installs Impact module for QP8.ProductCatalog

    .DESCRIPTION
    During installation script does the following actions:
    - Deletes components from previous installation
    - Installs impact web site

    .EXAMPLE
    .\Install.ps1 -logPath 'C:\Logs' -elasticUrl 'http://elastic01:9200' -liveIndexName 'products' -stageIndexName 'products_stage'

#>

param(
    ## Cleanup (or not) previous version 
    [Parameter()]
    [bool] $cleanUp = $true,
    ## Impact site name
    [Parameter()]
    [String] $siteName = 'Dpc.Impact',
    ## Impact port
    [Parameter()]
    [int] $port = 8033,
    ## ElasticSearch base address
    [Parameter(Mandatory = $true)]
    [string] $elasticUrl,
    ## Logs folder
    [Parameter(Mandatory = $true)]
    [String] $logPath,
    ## Live index name
    [Parameter(Mandatory = $true)]
    [String] $liveIndexName,
    ## Stage index name
    [Parameter(Mandatory = $true)]
    [String] $stageIndexName 
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$installLog = Join-Path $logPath "install.log"
Start-Transcript -Path $installLog -Append

. (Join-Path $currentPath "Modules\Get-SiteOrApplication.ps1")


if ($cleanUp) {
    $uninstallPath = Join-Path $currentPath "Uninstall.ps1"
    Invoke-Expression "$uninstallPath -ImpactName '$siteName'"
}

$scriptName = Join-Path $currentPath "InstallImpact.ps1"
Invoke-Expression "$scriptName -SiteName '$siteName' -Port $port -ElasticUrl '$elasticUrl' -LiveIndexName '$liveIndexName' -StageIndexName '$stageIndexName' -logPath '$logPath' "
