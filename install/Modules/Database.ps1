function Execute-Sql {
    [CmdletBinding()]
    param(
        [String] $database ='',
        [Parameter(Mandatory = $true)]
        [String] $server ='',
        [Parameter(Mandatory = $true)]
        [String] $name ='',
        [Parameter(Mandatory = $true)]
        [String] $pass = '',
        [Parameter(Mandatory = $true)]
        [String] $query = '',
        [int] $dbType = 0
    )

    $cnnString = Get-ConnectionString -database $database -server $server -name $name -pass $pass -dbType $dbType

    if ($dbType -eq 1) {
        $expr = """
            \! chcp 1251
            \pset pager off
            $query"" | psql -qtAx -d '$cnnString'"
        $result = Invoke-Expression $expr
    }
    else {
        $useSqlPs = (-not(Get-Module -ListAvailable -Name SqlServer))
        $moduleName = if ($useSqlPs) { "SqlPS" } else { "SqlServer" }
        Import-Module $moduleName

        

        $invokeParams = @{
            ConnectionString = $cnnString;
            Query = $query;
            QueryTimeout = 0
        }

        $result = Invoke-Sqlcmd @invokeParams -Verbose -AbortOnError
    }
    return $result
}

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
    [string] $server,
    [Parameter()]
    [string] $database,
    [Parameter()]
    [string] $name,
    [Parameter()]
    [string] $pass,
    [string] $dbType = 0
  )

  if ($dbType -eq 1) {
    $db = if ([string]::IsNullOrEmpty($databaseName)) { "postgres" } else { $databaseName }
    return "postgresql://${userName}:$password@$serverInstance/$db"
  } else {
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

}

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
    [String] $password,
    [int] $dbType = 0
  )
  $executeParams = @{
    database = $databaseName;
    server = $databaseServer;
    user = $userName;
    pass = $userPassword;
    dbType = $dbType;
  }

  if ($dbType -eq 1) 
  {
    $resetQuery = if ($resetUserPassword) {"
        ALTER USER $userName WITH PASSWORD '$userPassword';
        RAISE NOTICE 'Changed password for user $userName';"
    } else { "RAISE NOTICE 'User $userName' already exists" }

    $createQuery = "do `$`$
    begin
      CREATE USER $userName WITH PASSWORD '$userPassword';
      RAISE NOTICE 'User $userName has been successfully created';
      EXCEPTION
        WHEN duplicate_object THEN
          $resetQuery
    end `$`$;"
    
    $result = Execute-Sql @executeParams -query $createQuery
    Write-Verbose $result

    $grantQuery = "GRANT ALL PRIVILEGES ON DATABASE $databaseName TO $userName"
    $result = Execute-Sql @executeParams -query $grantQuery
    Write-Verbose $result

  } else {
    $useSqlPs = (-not(Get-Module -ListAvailable -Name SqlServer))
    $moduleName = if ($useSqlPs) { "SqlPS" } else { "SqlServer" }
    Import-Module $moduleName
    
    $resetPasswordQuery = if ($resetUserPassword) {"
        ALTER LOGIN $userName WITH PASSWORD = N'$userPassword'
        ALTER LOGIN $userName ENABLE
        SELECT 2 val"
    } else { "SELECT 0 val"  }
      
    $loginQuery = "
      IF NOT EXISTS(SELECT NULL FROM [master].[dbo].[syslogins] WHERE [Name] = '$userName' )
      BEGIN
        CREATE LOGIN $userName WITH PASSWORD = N'$userPassword', DEFAULT_DATABASE = master, DEFAULT_LANGUAGE = US_ENGLISH
        ALTER LOGIN $userName ENABLE
        SELECT 1 val
      END
      ELSE
      BEGIN
        $resetPasswordQuery
      END"
    
    $userQuery = "
      IF USER_ID('$userName') IS NULL
      BEGIN
        CREATE USER [$userName] FOR LOGIN [$userName] WITH DEFAULT_SCHEMA=[dbo]
        SELECT 1 val
      END
      ELSE
        SELECT 0 val
      GO
      sp_addrolemember 'db_owner', '$userName'"
    
    $res = Execute-Sql @executeParams -query $loginQuery 
    
    if ($res.val -eq 1)
    {
      Write-Verbose "A database $databaseName login $userName is created"  
    }
    
    if ($res.val -eq 2)
    {
      Write-Verbose "A database $databaseName login $userName password is reseted" 
    }
    
    $res = Execute-Sql @executeParams -query $userQuery 
    
    if ($res.val -eq 1)
    {
      Write-Verbose "A database $databaseName user $userName is created"  
    }
    
    Write-Verbose "In database $databaseName for user $userName added role db_owner" 
  }
}

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
    Database server login (should be omitted in case of Windows Authentication)

    .PARAMETER password
    Database server password (should be omitted in case of Windows Authentication)
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
    [String] $password,
    [Parameter()]
    [int] $dbType = 0
  )

  $executeParams = @{
    server = $databaseServer;
    user = $login;
    pass = $password;
    dbType = $dbType;
  }

  if ($dbType -eq 1) {

    $createQuery = "SELECT 'CREATE DATABASE $databaseName'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$databaseName')\gexec"

    Execute-Sql @executeParams -query $createQuery
    $cnnString = Get-ConnectionString @executeParams -database $databaseName 

    Invoke-Expression "pg_restore -d '$cnnString' '$backupPath'"

  } else {
    $useSqlPs = (-not(Get-Module -ListAvailable -Name SqlServer))
    $moduleName = if ($useSqlPs) { "SqlPS" } else { "SqlServer" }
    Import-Module $moduleName
  
    if ([string]::IsNullOrEmpty($databaseDir))
    { 
      $dirQuery = "SELECT CONVERT(SYSNAME, SERVERPROPERTY('InstanceDefaultDataPath')) [Path]"  
      $databaseDir = (Execute-Sql @executeParams -query $dirQuery).Path
    }
         
    Write-Output "Restoring $databaseName from $backupPath"
    $restoreQuery = "RESTORE FILELISTONLY FROM DISK = N'$backupPath';"
    $databaseFiles = Execute-Sql @executeParams -query $restoreQuery
  
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
  
    Execute-Sql @executeParams -query $query 
  
    Write-Output "$databaseName is restored"
  }

}
