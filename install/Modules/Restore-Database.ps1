function Restore-Database
{
  <#
    .SYNOPSIS
    Restore database from backup file

    .DESCRIPTION
    Allows to restore database from backup file
    
    .PARAMETER databaseServer
    Database server name

    .PARAMETER databaseName
    Database name to restore

   .PARAMETER backupPath
    Path to backup file

    .PARAMETER databaseDir
    Directory path for database files

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
    [string] $backupPath,
    [Parameter()]
    [string] $databaseDir,
    [Parameter()]
    [String] $login,
    [Parameter()]
    [String] $password
  )

  Import-Module -Name SqlServer

  if ([string]::IsNullOrEmpty($databaseDir))
  {    
    $databaseDir = Get-DefaultDatabaseDir -DatabaseServer $databaseServer -Login $login -Password $password
  }
       
  Write-Output "Restoring $databaseName from $backupPath"

  $connectionString = Get-ConnectionString -ServerInstance $databaseServer -Username $login -Password $password
  $databaseFiles = Invoke-Sqlcmd -Query "RESTORE FILELISTONLY FROM DISK = N'$backupPath';" -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop

  $query =
  "IF (EXISTS (SELECT null FROM [master].[dbo].[sysdatabases] WHERE name = N'$databaseName'))
      ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; 
  RESTORE DATABASE [$databaseName]
      FROM DISK = N'$backupPath' WITH FILE = 1"

  foreach($databaseFile in $databaseFiles)
  {
      $logicalName = $databaseFile.LogicalName      
      $fileExtension = [IO.Path]::GetExtension($databaseFile.PhysicalName)
      $query += ",
      MOVE N'$logicalName' TO N'$databaseDir$databaseName$fileExtension'"
  }

  $query += "
  ALTER DATABASE [$databaseName] SET MULTI_USER;
  ALTER DATABASE [$databaseName] SET RECOVERY SIMPLE;"

  Write-Output "execute`n$query"

  Invoke-Sqlcmd -Query $query -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop

  Write-Output "$databaseName is restored"
}