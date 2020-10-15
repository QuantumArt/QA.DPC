<#
    .SYNOPSIS
    Registers Customer code in QP

    .DESCRIPTION
    Registers new customer code for catalog in QP:
    - Restores catalog database from backup
    - Brings catalog database up-to-date
    - Registers customer code in QP confirutation file

    .EXAMPLE
    .\InstallConsolidationCustomerCode.ps1 -databaseServer 'dbhost' -targetBackupPath 'c:\temp\catalog_consolidation.bak' -customerCode 'catalog_consolidation' -customerLogin 'login' -customerPassword '1q2w#E$R' 
    -currentSqlPath '\\storage\Developers_share\QP\current.sql' -frontHost 'http://localhost:8013' -syncApiHost 'http://localhost:8015' -elasticsearchHost 'http://node1:9200; http://node2:9200' -adminHost 'http://localhost:89/Dpc.Admin'
#>
param(
    ## Database Server
    [Parameter(Mandatory = $true)]
    [string] $databaseServer,
    ## Backup file for copying onto database Server
    [Parameter()]
    [ValidateScript({ if ($_) { Test-Path $_} })]
    [string] $sourceBackupPath,
    ## Backup file for restoring (server local - for SQL Server)
    [Parameter(Mandatory = $true)]
    [string] $targetBackupPath,
    ## Catalog customer code
    [Parameter(Mandatory = $true)]
    [string] $customerCode,
    ## Dpc.Front host
    [Parameter(Mandatory = $true)]
    [string] $frontHost,
    ## Dpc.SyncApi host
    [Parameter(Mandatory = $true)]
    [string] $syncApiHost,
    ## Dpc.Admin host
    [Parameter(Mandatory = $true)]
    [string] $adminHost,
    ## Elasticsearch cluster search
    [Parameter(Mandatory = $true)]
    [string] $elasticsearchHost,
    ## Sql script for bringing DB up-to-date
    [Parameter(Mandatory = $true)]
    [string] $currentSqlPath,
    ## Customer user name
    [Parameter()]
    [String] $customerLogin,
    ## Customer user password
    [Parameter()]
    [String] $customerPassword,
    ## Admin user name
    [Parameter()]
    [String] $login,
    ## Admin user password
    [Parameter()]
    [String] $password,
    ## Database type: 0 - SQL Server, 1 - Postgres
    [Parameter()]
    [int] $dbType
)

function PSqlToPsObject
{
    param(
        $lines
    )

    $result = New-Object PsObject
    foreach ($line in $lines) {
        $values = $line.Split('|')
        if ($values.Length -lt 2) {
            continue
        }
        $result | Add-Member NoteProperty $values[0] $values[1]
    }
    return $result

}


function GetFieldId
{
    param(
        [hashtable] $connectionParams,
        [string] $key,
        [string] $field
    )
    
    $isPg = $connectionParams.dbType -eq 1
    $keyField = if ($isPg) { "Key" } else { "[Key]" }
    $query = "select value from APP_SETTINGS where $keyField = '$key'"
    $result = Execute-Sql @connectionParams -Query $query  
    $contentId = if ($isPg) { (PSqlToPsObject -lines $result).Value } else { $result.Value }
    
    if (!$contentId) { throw "setting '$key' is not exists"}
    $query = "select ATTRIBUTE_ID from CONTENT_ATTRIBUTE where CONTENT_ID = $contentId and ATTRIBUTE_NAME = '$field'"
    $result = Execute-Sql @connectionParams -Query $query  
    $fieldId = if ($isPg) { (PSqlToPsObject -lines $result).ATTRIBUTE_ID } else { $result.ATTRIBUTE_ID } 

    if (!$fieldId) { throw "field '$field' is not found for content $contentId"}
    return $fieldId
}

function ReplaceFieldValues
{
    param(
        [hashtable] $connectionParams,
        [int] $fieldId,
        [string] $placeholder,
        [string] $value
    )  

    $query = "update content_data set data = replace(data, '$placeholder', '$value') where ATTRIBUTE_ID = $fieldId and data like '%$placeholder%'"
    Execute-Sql @connectionParams -Query $query   
}

$currentPath = Split-path -parent $MyInvocation.MyCommand.Definition

. (Join-Path $currentPath "Modules\Database.ps1")
. (Join-Path $currentPath "Modules\CustomerCode.ps1")

if (Get-CustomerCode -CustomerCode $customerCode)
{
    Write-Host "Customer code $customerCode already exists"
    return
}

$resetUserPassword = $false

if (-not $customerLogin){
    $resetUserPassword = $true
    $customerLogin = "consolidation_${customerCode}_login"
}
if (-not $customerPassword){

    $customerPassword = New-Guid
}

if (-not [string]::IsNullOrEmpty($sourceBackupPath))
{
    if ($dbType -eq 0) {
        $sharedTargetBackupPath =  "\\" + $databaseServer.Trim() + "\" + $targetBackupPath.Replace(":", "$")
    } else {
        $sharedTargetBackupPath = $targetBackupPath
    }
    Write-Host "Copy backup from $sourceBackupPath to $sharedTargetBackupPath"
    
    try
    {
        Copy-Item $sourceBackupPath -Destination $sharedTargetBackupPath -ErrorAction Stop
    }
    catch
    {
        throw $_.Exception
    }

    Write-Host "Backup copied" 
}

$dbParams = @{
    DatabaseServer = $databaseServer;
    DatabaseName = $customerCode;
    Login = $login;
    Password = $password;
    DbType = $dbType
}

Restore-Database @dbParams -BackupPath $targetBackupPath  
Add-DatabaseUser @dbParams -UserName $customerLogin -UserPassword $customerPassword -ResetUserPassword $resetUserPassword 

$cnnParams = @{
    Server = $databaseServer;
    Database = $customerCode;
    Name = $login;
    Pass = $password;
    DbType = $dbType;
}

Write-Host "Run Current.sql on $currentSqlPath"  
Execute-File @cnnParams -path $currentSqlPath 
Write-Host "Current.sql updated"  

Write-Host "Update database"  

$fieldId = GetFieldId -connectionParams $cnnParams -key "NOTIFICATION_SENDER_CHANNELS_CONTENT_ID" -field "Url"
ReplaceFieldValues -connectionParams $cnnParams -fieldId $fieldId -placeholder "{site_sync}" -value $frontHost
ReplaceFieldValues -connectionParams $cnnParams -fieldId $fieldId -placeholder "{elastic_sync}" -value $syncApiHost

$fieldId = GetFieldId -connectionParams $cnnParams -key "ELASTIC_INDEXES_CONTENT_ID" -field "Name"
ReplaceFieldValues -connectionParams $cnnParams -fieldId $fieldId -placeholder "{code}" -value $customerCode

$fieldId = GetFieldId -connectionParams $cnnParams -key "ELASTIC_INDEXES_CONTENT_ID" -field "Address"
ReplaceFieldValues -connectionParams $cnnParams -fieldId $fieldId -placeholder "{elasticsearch}" -value $elasticsearchHost

$validationPlaceholder = "adminhost"
if ($dbType -eq 0) {
    $replace = "cast(replace(cast(XAML_DICTIONARIES as nvarchar(max)), '$validationPlaceholder', '$adminHost') as ntext)"
} else {
    $replace = "replace(XAML_DICTIONARIES, '$validationPlaceholder', '$adminHost')"
}
$validationQuery = "update site set XAML_DICTIONARIES = $replace where XAML_DICTIONARIES like '%$validationPlaceholder%'"

Execute-Sql @cnnParams -query $validationQuery

$localhost = "localhost:5250"
$oldCustomerCode = "sber_pg"

if ($dbType -eq 0) {
    $replace = "cast(replace(cast(FORM_SCRIPT as nvarchar(max)), '$localhost', '$adminHost') as ntext)"
    $replace2 = "cast(replace(cast(FORM_SCRIPT as nvarchar(max)), '$oldCustomerCode', '$customerCode') as ntext)"    
} else {
    $replace = "replace(FORM_SCRIPT, '$localhost', '$adminHost')"
    $replace2 = "replace(FORM_SCRIPT, '$oldCustomerCode', '$customerCode')"
}
$formQuery = "update content set FORM_SCRIPT = $replace where FORM_SCRIPT like '%$localhost%'"
$formQuery2 = "update content set FORM_SCRIPT = $replace2 where FORM_SCRIPT like '%$oldCustomerCode%'"

Execute-Sql @cnnParams -query $formQuery

Execute-Sql @cnnParams -query $formQuery2
 
Write-Host "Database updated"  
$savedParams = @{
    Server = $databaseServer;
    Database = $customerCode;
    Name = $customerLogin;
    Pass = $customerPassword;
    DbType = $dbType;
}

$connectionString = Get-ConnectionString @savedParams -forConfig $true
Add-CustomerCode -CustomerCode $customerCode -ConnectionString $connectionString -dbType $dbType

Apply-QPConfigurationPathx64