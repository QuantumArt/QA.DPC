<#
  Installs Windows-service
#>

param(
  [Parameter(Mandatory=$true)]
  [String] $name,
  [Parameter(Mandatory=$true)]
  [String] $displayName,
  [Parameter(Mandatory=$true)]
  [String] $description,
  [Parameter(Mandatory=$true)]
  [String] $projectName,
  [ValidatePattern("\d{2}:\d{2}:\d{2}")]
  [String] $timeout = "00:03:00",
  [Parameter(Mandatory=$true)]
  [String] $installRoot,
  [Parameter(Mandatory=$true)]
  [ValidateScript({ Test-Path $_ -PathType Container})]
  [String] $source,
  [Parameter(Mandatory=$true)]
  [String] $login,
  [Parameter(Mandatory=$true)]
  [String] $password,
  [Bool] $start = $true
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$s = Get-Service $name -ErrorAction SilentlyContinue
if ($s) { throw "Server $name already installed" }

if (-not(Test-Path $installRoot)) { New-Item $installRoot -ItemType Directory }
$installFolder = Join-Path $installRoot $name
if (Test-Path $installFolder) { throw "Service folder $installFolder already exists" } else { New-Item $installFolder -ItemType Directory }

if (Test-Path $source -PathType Container)
{
    Write-Verbose "Copy item from $source to $installFolder ..." -Verbose
    Copy-Item "$source\bin\Release\*" "$installFolder" -Force -Recurse
    Write-Verbose "Done" -Verbose
}
elseif (Test-Path $source -PathType Leaf)
{
    Write-Verbose "Unzipping from $source to $installFolder ..." -Verbose
    Expand-Archive -LiteralPath $source -DestinationPath $installFolder -Force
}

$secpasswd = ConvertTo-SecureString $password -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ($login, $secpasswd)

Write-Verbose "Installing service: $name" -Verbose
New-Service -name $name -binaryPathName "$installFolder\$projectName.exe" -Description $description -displayName $displayName -startupType Automatic -credential $mycreds
Write-Verbose "Installation completed: $name"

Write-Verbose "Waiting for a while..." -Verbose
Start-Sleep -s 5
Write-Verbose "Done" -Verbose

if ($start)
{
  Start-WinService $name $timeout -Verbose
}
