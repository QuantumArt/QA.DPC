function Add-DatabaseUser
{
  <#
    .SYNOPSIS
    Create User

    .DESCRIPTION
    Creates user for given database if not exists. Then adds role db_owner.
    
    .PARAMETER databaseServer
    Database server name

    .PARAMETER databaseName
    Database name to restore which user will be added in

    .PARAMETER userName
    Database user to be added

    .PARAMETER login
    Database server login

    .PARAMETER password
    Database server password
  #>
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string] $databaseServer,  
    [Parameter(Mandatory = $true)]
    [string] $databaseName,
    [Parameter(Mandatory = $true)]
    [string] $userName,
    [Parameter()]
    [String] $login,
    [Parameter()]
    [String] $password
  )

  Import-Module -Name SqlServer

  $connectionString = Get-ConnectionString -ServerInstance $databaseServer -DatabaseName $databaseName -Username $login -Password $password
  $query = 
    "IF USER_ID('$userName') IS NULL
      BEGIN
        CREATE USER [$userName] FOR LOGIN [$userName] WITH DEFAULT_SCHEMA=[dbo]
        SELECT 1 [val]
      END
      ELSE
        SELECT 0 [val]
    GO
    sp_addrolemember 'db_owner', '$userName'"

  $res = Invoke-Sqlcmd -Query $query  -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop

  if ($res.val -eq 1)
  {
    Write-Verbose "A database $databaseName user $userName is created"  -Verbose
  }

  Write-Verbose "In database $databaseName for user $userName added role db_owner"  -Verbose
}