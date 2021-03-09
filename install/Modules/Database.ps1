function Execute-Sql {
    [CmdletBinding()]
    param(
        [String] $database ='',
        [Parameter(Mandatory = $true)]
        [String] $server ='',
        [Parameter()]
        [String] $name ='',
        [Parameter()]
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
            $query"" | psql -qbtAx -d '$cnnString' 2>&1"
        Write-Verbose $expr
        $result = Invoke-Expression $expr
        if ($result -match "ERROR:|FATAL:") { $result | Write-Host }     
    } else {
        $useSqlPs = (-not(Get-Module -ListAvailable -Name SqlServer))
        $moduleName = if ($useSqlPs) { "SqlPS" } else { "SqlServer" }
        if (-not(Get-Module -Name $moduleName)) {
            Import-Module $moduleName
        }

        $invokeParams = @{
            Query = $query;
            QueryTimeout = 0
        }

        Write-Verbose $query

        if ($useSqlPs) {
          Set-SqlPsInvokeParams -invokeParams $invokeParams -database $database -server $server -name $name -pass $pass
        } else {
            $invokeParams.ConnectionString = $cnnString;
        }

        $result = Invoke-Sqlcmd @invokeParams -AbortOnError
    }
    return $result
}

function Execute-File {
    [CmdletBinding()]
    param(
        [String] $database ='',
        [Parameter(Mandatory = $true)]
        [String] $server ='',
        [Parameter()]
        [String] $name ='',
        [Parameter()]
        [String] $pass = '',
        [Parameter(Mandatory = $true)]
        [ValidateScript({ if ($_) { Test-Path $_} })]
        [String] $path = '',
        [int] $dbType = 0
    )

    $cnnString = Get-ConnectionString -database $database -server $server -name $name -pass $pass -dbType $dbType

    if ($dbType -eq 1) {
        $expr = "psql -b -d '$cnnString' -c '\! chcp 1251' -c'\pset pager off' -f '$path' 2>&1"
        Write-Verbose "Executing DB script: ${path}"
        $result = Invoke-Expression $expr
        if ($result -match "ERROR:") { $result | Write-Host }     
    } else {
        $useSqlPs = (-not(Get-Module -ListAvailable -Name SqlServer))
        $moduleName = if ($useSqlPs) { "SqlPS" } else { "SqlServer" }
        if (-not(Get-Module -Name $moduleName)) {
            Import-Module $moduleName
        }

        $invokeParams = @{
            InputFile = $path;
            QueryTimeout = 0
        }

        if ($useSqlPs) {
            Set-SqlPsInvokeParams -invokeParams $invokeParams -database $database -server $server -name $name -pass $pass
        } else {
            $invokeParams.ConnectionString = $cnnString;
        }
        Invoke-Sqlcmd @invokeParams -AbortOnError
    }
}

function Get-ConnectionString
{
    <#
        .SYNOPSIS    
        Get connection string

        .DESCRIPTION
        Allows to get connection string for given server
        
        .PARAMETER server
        Database server name

        .PARAMETER database
        Database name

        .PARAMETER name
        Database server login

        .PARAMETER pass
        Database server password
        
        .PARAMETER dbType
        Database type: 0 - SQL Server, 1 - PostgreSQL
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string] $server,
        [Parameter()]
        [string] $database,
        [Parameter()]
        [string] $name,
        [Parameter()]
        [string] $pass,
        [string] $dbType = 0,
        [bool] $forConfig = $false
    )

    if ($dbType -eq 1) {
        $db = if ($database) { $database } else { "postgres" }
        if ($forConfig) {
            return "Server=$server;Database=$db;User Id=$name;Password=$pass;"
        } else {
            return "postgresql://${name}:$pass@$server/$db"
        }
    } else {
        $databasePart = if ($database) { "Initial Catalog=$database;" } else { "" }
        $securityPart = if ($name -and $pass) { "User ID=$name;Password=$pass;" }
        else { "Integrated Security=True;" }
        return "Data Source=$server;$databasePart$securityPart"
    }
}

function Set-SqlPsInvokeParams
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [hashtable] $invokeParams,
        [Parameter()]
        [string] $server,
        [Parameter()]
        [string] $database,
        [Parameter()]
        [string] $name,
        [Parameter()]
        [string] $pass
    )

    $invokeParams.ServerInstance = $server
    if ($database) {
        $invokeParams.Database = $database
    }
    if ($user -and $pass) {
        $invokeParams.Username = $user
        $invokeParams.Password = $pass
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

        .PARAMETER dbType
        Database type: 0 - SQL Server, 1 - PostgreSQL
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
        [bool] $resetUserPassword = $false,
        [Parameter()]
        [String] $login,
        [Parameter()]
        [String] $password,
        [int] $dbType = 0
    )
    $executeParams = @{
        database = $databaseName;
        server = $databaseServer;
        dbType = $dbType;
    }

    if ($login -and $password) {
        $executeParams.name = $login;
        $executeParams.pass = $password;
    }

    if ($dbType -eq 1) {
        $userPassword = $userPassword.Replace('$', '`$')
        $resetQuery = if ($resetUserPassword) {"
            ALTER USER $userName WITH PASSWORD '$userPassword';
            RAISE NOTICE 'Changed password for user $userName';"} 
        else { "RAISE NOTICE 'User ''$userName'' already exists';" }

        $createQuery = "do ```$```$
        begin
        CREATE USER $userName WITH PASSWORD '$userPassword';
        RAISE NOTICE 'User ''$userName'' has been successfully created';
        EXCEPTION
            WHEN duplicate_object THEN
            $resetQuery
        end ```$```$;"
        
        Write-Host "Creating user $userName..."
        $result = Execute-Sql @executeParams -query $createQuery
        Write-Host $result

        $grantQuery = "
            GRANT CONNECT ON DATABASE $databaseName TO $userName;
            GRANT USAGE ON SCHEMA public TO $userName;
            GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO $userName;
            GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO $userName;
            GRANT EXECUTE ON ALL ROUTINES IN SCHEMA public TO $userName;
        "
        Write-Host "Granting access to user $userName..."
        Execute-Sql @executeParams -query $grantQuery | Out-Null
        Write-Host "Done"

    } else {
   
        $resetPasswordQuery = if ($resetUserPassword) {"
            ALTER LOGIN $userName WITH PASSWORD = N'$userPassword'
            ALTER LOGIN $userName ENABLE
            SELECT 2 val"} 
        else { "SELECT 0 val"  }
      
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
        
        if ($res.val -eq 1) {
            Write-Host "A database $databaseName login $userName is created"  
        }
        
        if ($res.val -eq 2) {
            Write-Host "A database $databaseName login $userName password is reseted" 
        }
        
        $res = Execute-Sql @executeParams -query $userQuery 
        
        if ($res.val -eq 1) {
            Write-Host "A database $databaseName user $userName is created"  
        }
        
        Write-Host "In database $databaseName for user $userName added role db_owner" 
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

        .PARAMETER dbType
        Database type: 0 - SQL Server, 1 - PostgreSQL
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
        name = $login;
        pass = $password;
        dbType = $dbType;
    }

    if ($dbType -eq 1) {

        Write-Host "Dropping database $databaseName..."
        $shutDownQuery = 
        "UPDATE pg_database SET datallowconn = 'false' WHERE datname = '$databaseName'; 
        SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$databaseName';"
        Execute-Sql @executeParams -query $shutDownQuery  | Out-Null
        $dropQuery = "DROP DATABASE IF EXISTS $databaseName"
        Execute-Sql @executeParams -query $dropQuery  | Out-Null
        Write-Host "Done"

        Write-Host "Creating empty database $databaseName..."
        $createQuery = "CREATE DATABASE $databaseName"
        Execute-Sql @executeParams -query $createQuery | Out-Null
        Write-Host "Done"

        Write-Host "Restoring $databaseName from $backupPath..."
        $cnnString = Get-ConnectionString @executeParams -database $databaseName 
        $numCores = (Get-WmiObject -class Win32_ComputerSystem).NumberOfLogicalProcessors
        Invoke-Expression "pg_restore -Fc -d '$cnnString' -j $numCores --no-privileges --no-owner '$backupPath'"
        Write-Host "Done"

    } else {
 
        if ([string]::IsNullOrEmpty($databaseDir))
        { 
            $dirQuery = "SELECT CONVERT(SYSNAME, SERVERPROPERTY('InstanceDefaultDataPath')) [Path]"  
            $databaseDir = (Execute-Sql @executeParams -query $dirQuery).Path
        }
         
        Write-Host "Restoring $databaseName from $backupPath"
        $restoreQuery = "RESTORE FILELISTONLY FROM DISK = N'$backupPath';"
        $databaseFiles = Execute-Sql @executeParams -query $restoreQuery
    
        $query =
        "IF (EXISTS (SELECT null FROM [master].[dbo].[sysdatabases] WHERE name = N'$databaseName'))
            ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; 
        RESTORE DATABASE [$databaseName]
            FROM DISK = N'$backupPath' WITH REPLACE, FILE = 1"
  
        foreach ($databaseFile in $databaseFiles)
        {
            $logicalName = $databaseFile.LogicalName      
            $fileExtension = [IO.Path]::GetExtension($databaseFile.PhysicalName)
            $query += ",
            MOVE N'$logicalName' TO N'$databaseDir$databaseName$fileExtension'"
        }
  
        $query += "
        ALTER DATABASE [$databaseName] SET MULTI_USER;
        ALTER DATABASE [$databaseName] SET RECOVERY SIMPLE;"
    
    
        Execute-Sql @executeParams -query $query | Out-Null
    
        Write-Host "$databaseName is restored"
    }
}
