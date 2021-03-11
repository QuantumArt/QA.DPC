<#
    .SYNOPSIS
    Validates whether Impact module could be installed

    .DESCRIPTION
    Checks:
    - .NET Core Runtime 3.1.12 or newer is installed
    - Port is available

    .EXAMPLE
    .\Validate.ps1 -impactPort 8033
    
#>
param(
    ## Порт DPC.Impact
    [Parameter()]
    [int] $impactPort
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

$requiredRuntime = '3.1.1[2-9]'
  
Try {
    $actualRuntimes = (Get-ChildItem (Get-Command dotnet).Path.Replace('dotnet.exe', 'shared\Microsoft.AspNetCore.App')).Name
} Catch {
    Write-Error $_.Exception
    Throw "Check ASP.NET Core runtime : failed"
}

if (!($actualRuntimes | Where-Object {$_ -match $requiredRuntime})){ Throw "Check ASP.NET Core runtime 3.1.x (3.1.12 or newer) : failed" }



Test-Port -Port $impactPort -Name "ImpactPort"
