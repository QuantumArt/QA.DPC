param(
    [Parameter(Mandatory = $true)]
    [string] $databaseServer,
    [Parameter()]
    [ValidateScript({ if (-not [string]::IsNullOrEmpty($_)) { Test-Path $_}})]
    [string] $sourceBackupPath,
    [Parameter(Mandatory = $true)]
    [string] $targetBackupPath,
    [Parameter(Mandatory = $true)]
    [string] $customerCode,
    [Parameter(Mandatory = $true)]
    [string] $siteSyncHost,
    [Parameter(Mandatory = $true)]
    [string] $syncApiHost,
    [Parameter(Mandatory = $true)]
    [string] $adminHost,
    [Parameter(Mandatory = $true)]
    [string] $elasticsearchHost,
    [Parameter(Mandatory = $true)]
    [string] $currentSqlPath,
    [Parameter(Mandatory = $true)]
    [String] $customerLogin,
    [Parameter(Mandatory = $true)]
    [String] $customerPassword,
    [Parameter()]
    [String] $login,
    [Parameter()]
    [String] $password
)

function GetFieldId
{
  param(
     [string] $connectionString,
     [string] $key,
     [string] $field
  )

  $query = "select * from [APP_SETTINGS] where [Key] = '$key'"  
  $result = Invoke-Sqlcmd -Query $query  -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop
  $contentId = $result.Value

  if (!$contentId) { throw "setting '$key' is not exists"}

  $query = "select ATTRIBUTE_ID from CONTENT_ATTRIBUTE where CONTENT_ID = $contentId and ATTRIBUTE_NAME = '$field'"
  $result = Invoke-Sqlcmd -Query $query  -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop
  $fieldId = $result.ATTRIBUTE_ID

  if (!$fieldId) { throw "field '$field' is not found for content $contentId"}
  return $fieldId
}

function ReplaceFieldValues
{
  param(
     [string] $connectionString,
     [int] $fieldId,
     [string] $placeholder,
     [string] $value
  )  
  $query = "update content_data set [data] = replace([data], '$placeholder', '$value') where ATTRIBUTE_ID = $fieldId and [data] like '%$placeholder%'"
  Invoke-Sqlcmd -Query $query  -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop  
  Write-Host $query
}

Import-Module -Name SqlServer

if(Get-CustomerCode -CustomerCode $customerCode)
{
    Write-Verbose "Customer code $customerCode already exists"
    return
}

if (-not [string]::IsNullOrEmpty($sourceBackupPath))
{
    $sharedTargetBackupPath =  "\\" + $databaseServer.Trim() + "\" + $targetBackupPath.Replace(":", "$")
    Write-Verbose "Copy backup from $sourceBackupPath to $sharedTargetBackupPath"  -Verbose
    
    try
    {
        Copy-Item $sourceBackupPath -Destination $sharedTargetBackupPath -ErrorAction Stop
    }
    catch
    {
        throw $_.Exception
    }

    Write-Verbose "Backup copied"
}


Restore-Database -DatabaseServer $databaseServer -DatabaseName $customerCode -BackupPath $targetBackupPath -Login $login -Password $password
Add-DatabaseUser -DatabaseServer $databaseServer -DatabaseName $customerCode -UserName $customerLogin -Login $login -Password $password

$connectionString = Get-ConnectionString -ServerInstance $databaseServer -DatabaseName $customerCode -Username $login -Password $password

Write-Verbose "Run Current.sql on $currentSqlPath"  -Verbose
Invoke-Sqlcmd -InputFile $currentSqlPath  -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop
Write-Verbose "Current.sql updated"  -Verbose

Write-Verbose "Update database"  -Verbose

$fieldId = GetFieldId -connectionString $connectionString -key "NOTIFICATION_SENDER_CHANNELS_CONTENT_ID" -field "url"
ReplaceFieldValues -connectionString $connectionString -fieldId $fieldId -placeholder "{site_sync}" -value $siteSyncHost
ReplaceFieldValues -connectionString $connectionString -fieldId $fieldId -placeholder "{elastic_sync}" -value $syncApiHost

$fieldId = GetFieldId -connectionString $connectionString -key "ELASTIC_INDEXES_CONTENT_ID" -field "name"
ReplaceFieldValues -connectionString $connectionString -fieldId $fieldId -placeholder "{code}" -value $customerCode

$fieldId = GetFieldId -connectionString $connectionString -key "ELASTIC_INDEXES_CONTENT_ID" -field "address"
ReplaceFieldValues -connectionString $connectionString -fieldId $fieldId -placeholder "{elasticsearch}" -value $elasticsearchHost

$validationPlaceholder = "adminhost"
$validationQuery = "update [site] set XAML_DICTIONARIES = cast(replace(cast(XAML_DICTIONARIES as nvarchar(max)), '$validationPlaceholder', '$adminHost') as ntext) where XAML_DICTIONARIES like '%$validationPlaceholder%'"
Invoke-Sqlcmd -Query $validationQuery -ConnectionString $connectionString -Verbose -Querytimeout 0 -ErrorAction Stop  
 
Write-Verbose "updated"  -Verbose

$connectionString = "Provider=SQLOLEDB;Initial Catalog=$customerCode;Data Source=$databaseServer;User ID=$customerLogin;Password=$customerPassword"
Add-CustomerCode -CustomerCode $customerCode -ConnectionString $connectionString