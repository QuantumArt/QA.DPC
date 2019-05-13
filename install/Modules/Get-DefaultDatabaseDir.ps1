function Get-DefaultDatabaseDir
{
   <#
    .SYNOPSIS    
    Directory path for database files

    .DESCRIPTION
    Allows to get directory path for database files
    
    .PARAMETER databaseServer
    Database server name

    .PARAMETER login
    Database server login

    .PARAMETER password
    Database server password
  #>
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string] $databaseServer,     
    [Parameter()]
    [String] $login,
    [Parameter()]
    [String] $password
  )

  Import-Module -Name SqlServer
  $connectionString = Get-ConnectionString -ServerInstance $databaseServer -Username $login -Password $password
  return (Invoke-Sqlcmd -Query "SELECT CONVERT(SYSNAME, SERVERPROPERTY('InstanceDefaultDataPath')) [Path]" -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop).Path
}
