function Get-SiteOrApplication
{
    <#
    .SYNOPSIS
    Returns Web-site or Web-application object

    .DESCRIPTION
    Returns Web-site or Web-application object
    Site should be specified with 'name' parameter, application - with 'name' and 'application' parameters.
    Throws if site or application is not found.

    .PARAMETER name
    Site name (in IIS).

    .PARAMETER application
    Application name (in IIS) for site specified with parameter 'name'.

    .OUTPUTS
    Site or application object
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [String] $name,
        [String] $application
    )

    $siteName = Get-SiteOrApplicationName $name $application

    try {
        $s = Get-Item "IIS:\sites\$siteName" -ErrorAction SilentlyContinue
    } catch {
        # http://help.octopusdeploy.com/discussions/problems/5172-error-using-get-website-in-predeploy-because-of-filenotfoundexception
        $s = Get-Item "IIS:\sites\$siteName" -ErrorAction SilentlyContinue
    }
    return $s
}


function Get-SiteOrApplicationName
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [String] $name,
        [String] $application
    )

    if ([string]::IsNullOrEmpty($application)) {
        $siteName = $name
    }
    else {
        $siteName = "$name\$application"
    }

    return $siteName
}

function Get-SiteOrApplicationBackupName
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [String] $name,
        [String] $application
    )

    $siteName = Get-SiteOrApplicationName $name $application
    return $siteName.Replace("\", "_")
}


if (-not(Get-Module -Name WebAdministration)) {
    Import-Module WebAdministration
}



