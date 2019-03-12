function Get-ConnectionString
{
   <#
    .SYNOPSIS    
    Get connection string

    .DESCRIPTION
    Allows to get connection string for given server
    
    .PARAMETER serverInstance
    Database server name

    .PARAMETER databaseName
    Database name

    .PARAMETER userName
    Database server login

    .PARAMETER password
    Database server password
  #>
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string] $serverInstance,
    [Parameter()]
    [string] $databaseName,
    [Parameter()]
    [string] $userName,
    [Parameter()]
    [string] $password
  )

    if (-not [string]::IsNullOrEmpty($databaseName))
  {
    $databasePart = "Initial Catalog=$databaseName;"
  }

  if ( -not [string]::IsNullOrEmpty($userName) -and -not [string]::IsNullOrEmpty($password))
  {
    $securityPart = "User ID=$userName;Password=$password;"    
  }
  else
  {
    $securityPart = "Integrated Security=True;"    
  }

  return "Data Source=$serverInstance;$databasePart$securityPart"
}
