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

    .PARAMETER userPassword
    Database user password to be added

    .PARAMETER resetUserPassword
    Reset database user password if user exists

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
    [Parameter(Mandatory = $true)]
    [string] $userPassword,
    [Parameter()]
    [string] $resetUserPassword = $false,
    [Parameter()]
    [String] $login,
    [Parameter()]
    [String] $password
  )

  Import-Module -Name SqlServer

  $connectionString = Get-ConnectionString -ServerInstance $databaseServer -DatabaseName $databaseName -Username $login -Password $password

  if ($resetUserPassword){
    $resetPasswordQuery = "
    ALTER LOGIN $userName WITH PASSWORD = N'$userPassword'
    ALTER LOGIN $userName ENABLE
    SELECT 2 [val]"
  } else {
    $resetPasswordQuery = "SELECT 0 [val]"
  }

  $loginQuery = 
  "IF NOT EXISTS(SELECT NULL FROM [master].[dbo].[syslogins] WHERE [Name] = '$userName' )
    BEGIN
      CREATE LOGIN $userName WITH PASSWORD = N'$userPassword', DEFAULT_DATABASE = master, DEFAULT_LANGUAGE = US_ENGLISH
      ALTER LOGIN $userName ENABLE
      SELECT 1 [val]
    END
    ELSE
    BEGIN
      $resetPasswordQuery
    END"

  $userQuery = 
    "IF USER_ID('$userName') IS NULL
      BEGIN
        CREATE USER [$userName] FOR LOGIN [$userName] WITH DEFAULT_SCHEMA=[dbo]
        SELECT 1 [val]
      END
      ELSE
        SELECT 0 [val]
    GO
    sp_addrolemember 'db_owner', '$userName'"

  $res = Invoke-Sqlcmd -Query $loginQuery -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop

  if ($res.val -eq 1)
  {
    Write-Verbose "A database $databaseName login $userName is created"  -Verbose
  }

  if ($res.val -eq 2)
  {
    Write-Verbose "A database $databaseName login $userName password is reseted" -Verbose
  }

  $res = Invoke-Sqlcmd -Query $userQuery -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop

  if ($res.val -eq 1)
  {
    Write-Verbose "A database $databaseName user $userName is created"  -Verbose
  }

  Write-Verbose "In database $databaseName for user $userName added role db_owner"  -Verbose
}