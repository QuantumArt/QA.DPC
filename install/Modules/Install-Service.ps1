function Install-Service
{
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
    [String] $password
  )

  If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
  {
      $arguments = "& '" + $myinvocation.mycommand.definition + "'"
      Start-Process powershell -Verb runAs -ArgumentList $arguments
      Break
  }

  $s = Get-Service $name -ErrorAction SilentlyContinue
  if ($s) { throw "Server $name already installed" }

  if (-not(Test-Path $installRoot)) { New-Item $installRoot -ItemType Directory | Out-Null }
  $installFolder = Join-Path $installRoot $name
  if (Test-Path $installFolder) { throw "Service folder $installFolder already exists" } else { New-Item $installFolder -ItemType Directory | Out-Null }

  if (Test-Path $source -PathType Container)
  {
      Write-Host "Copy item from $source to $installFolder ..." 
      Copy-Item "$source\*" "$installFolder" -Force -Recurse
      Write-Host "Done" 
  }
  elseif (Test-Path $source -PathType Leaf)
  {
      Write-Host "Unzipping from $source to $installFolder ..."
      Expand-Archive -LiteralPath $source -DestinationPath $installFolder -Force
  }

  $secpasswd = ConvertTo-SecureString $password -AsPlainText -Force
  $mycreds = New-Object System.Management.Automation.PSCredential ($login, $secpasswd)

  Write-Host "Installing service: $name"
  New-Service -name $name -binaryPathName "dotnet $installFolder\$projectName.dll" -Description $description -displayName $displayName -startupType Automatic -credential $mycreds
  Write-Host "Installation completed: $name"

}
