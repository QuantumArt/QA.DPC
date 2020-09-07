
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

    if (!$path) { throw "QP is not installed" }
    if (-not(Test-Path $path))  { throw "QP configuration $path is missing" }

    return $path
}

function Apply-QPConfigurationPathx64
{
    $registryPath = "Registry::HKLM\Software\Wow6432Node\Quantum Art\Q-Publishing"
    $registryPath2 = "Registry::HKLM\Software\Quantum Art\Q-Publishing"

    $item = Get-ItemProperty -Path $registryPath -ErrorAction SilentlyContinue
    $item2 = Get-ItemProperty -Path $registryPath2 -ErrorAction SilentlyContinue
    if ($item -and !$item2) {
        $path = $item."Configuration file"

        if (!$path) { throw "QP is not installed" }
        if (-not(Test-Path $path))  { throw "QP configuration $path is missing" }

        New-Item -Path $registryPath2 -Force | Out-Null
        Set-ItemProperty -Path $registryPath2 -Name "Configuration File" -Value $path
    }
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
    $customer = ($xml.configuration.customers.customer | Where-Object { $_.customer_name -eq $customerCode })
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
        [string] $connectionString,
        [int] $dbType = 0

    )

    $configurationPath = Get-QPConfigurationPath
    Write-Host "Set customer code $customerCode in $configurationPath"  
    [xml]$xml = Get-Content $configurationPath  
    $customers = $xml.SelectSingleNode('/configuration/customers')

    if ($customers)
    {
        $customer = ($customers.customer | Where-Object { $_.customer_name -eq $customerCode })

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
            $dbTypePart = if ($dbType -eq 1) { " db_type=""postgres"""} else {""}
            [xml]$el =
                "<customer customer_name='$customerCode'$dbTypePart>
                    <db>$connectionString</db>
                </customer>"
            $node = $xml.ImportNode($el.DocumentElement, $true)
            $node = $customers.AppendChild($node);
        }

        $xml.Save($configurationPath)
        Write-Host "Customer code $customerCode is updated"
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
                $customers.RemoveChild($customer) | Out-Null
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