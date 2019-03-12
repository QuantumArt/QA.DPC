
function Get-QPConfigurationPath
{
    <#
    .SYNOPSIS
    Get QP configuration path

    .DESCRIPTION
    Get QP configuration path
    #>

    if ([IntPtr]::size -eq 8)
    {
        $registryPath = "Registry::HKLM\Software\Wow6432Node\Quantum Art\Q-Publishing"
    }
    else
    {
        $registryPath = "Registry::HKLM\Software\Quantum Art\Q-Publishing"
    }

    $item = Get-ItemProperty -Path $registryPath -ErrorAction SilentlyContinue
    $path = $item."Configuration file"

    if ([string]::IsNullOrEmpty($path)) { throw "QP is not installed" }
    if (-not(Test-Path $path))  { throw "QP configuration $path is missing" }

    return $path
}

function Get-CustomerCode
{
    <#
    .SYNOPSIS
    Get QP customer code settings

    .DESCRIPTION
    Allows to read QP customer code settings
    
    .PARAMETER customerCode
    QP customer code
  #>
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string] $customerCode
  )

  $configurationPath = Get-QPConfigurationPath  
  [xml]$xml = Get-Content $configurationPath  
  $customer = ($xml.configuration.customers.customer | ? { $_.customer_name -eq $customerCode })
  return $customer
}

function Add-CustomerCode
{
  <#
    .SYNOPSIS
    Add QP customer code

    .DESCRIPTION
    Allows to register QP customer codes
    
    .PARAMETER customerCode
    QP customer code

    .PARAMETER connectionString
    Database connection string associated with customer code
  #>
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string] $customerCode,  
    [Parameter(Mandatory = $true)]
    [string] $connectionString    
  )

  Write-Verbose "Set customer code $customerCode in $configurationPath"

  $configurationPath = Get-QPConfigurationPath  
  [xml]$xml = Get-Content $configurationPath  
  $customers = $xml.SelectSingleNode('/configuration/customers')

  if ($customers)
  {
    $customer = ($customers.customer | ? { $_.customer_name -eq $customerCode })

    if ($customer)
    {
      if ($customer -is [system.array])
      {
          throw "tags configuration/customers/customer has duplicate attributes customer_name = $customerCode"
      }
      else
      {
          $customer.db = $connectionString
      }
    }
    else
    {
      [xml]$el =
          "<customer customer_name='$customerCode'>
              <db>$connectionString</db>
          </customer>"
      $node = $xml.ImportNode($el.DocumentElement, $true)
      $customers.AppendChild($node);
    }

    $xml.Save($configurationPath)
    Write-Verbose "Customer code $customerCode is updated"
  }
  else
  {
    throw "tags configuration/customers are missed in QP configuration file"
  }
}

function Remove-CustomerCode
{
    <#
    .SYNOPSIS
    Remove QP customer code settings

    .DESCRIPTION
    Allows to remove QP customer code settings
    
    .PARAMETER customerCode
    QP customer code
  #>
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string] $customerCode
  )

  Write-Verbose "Remove customer code $customerCode in $configurationPath"

  $configurationPath = Get-QPConfigurationPath  
  [xml]$xml = Get-Content $configurationPath  
  $customers = $xml.SelectSingleNode('/configuration/customers')

  if ($customers)
  {
    $customer = ($customers.customer | ? { $_.customer_name -eq $customerCode })

    if ($customer)
    {
      if ($customer -is [system.array])
      {
          throw "tags configuration/customers/customer has duplicate attributes customer_name = $customerCode"
      }
      else
      {        
          $customers.RemoveChild($customer)
          $xml.Save($configurationPath)
          Write-Verbose "Customer code $customerCode is removed"
      }
    }
    else
    {        
      Write-Verbose "Customer code $customerCode is missed"
    }
  }
  else
  {
    throw "tags configuration/customers are missed in QP configuration file"
  }
}