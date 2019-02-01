param(
    [String] $notificationSender = 'DPC.NotificationSender',
    [String] $actionsService = 'DPC.ActionsService',
    [String] $admin = 'Dpc.Admin',
    [String] $siteSync = 'Dpc.SiteSync',
    [String] $installRoot = 'C:\QA',
    [string] $customerCode
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

Import-Module WebAdministration

function DeleteService    
{
   param(
     [string] $name,
     [string] $installRoot
   )

   $s = Get-Service $name -ErrorAction SilentlyContinue

    if ($s){
        if ( $s.Status -eq "Running"){
            Write-Output "* stoping $name"
            $s.Stop()
            $s.WaitForStatus("Stopped", "00:03:00")
            Write-Output "* stoped"
        }

        $sobj = Get-WmiObject -Class Win32_Service -Filter "Name='$name'" 
        $sobj.Delete()    
        Write-Output "$name deleted"   
    }
    else{
        Write-Output "$name is not installed"
    }

    $path = Join-Path $installRoot $name

    if (Test-Path $path){
        Remove-Item $path -Recurse
        Write-Output "$name files removed"
    }
    else{
        Write-Output "$name files is not exists"
    }
}

function DeleteSite
{
  param(
     [string] $qp,
     [string] $name
  )


  if ($qp){
    $alias = "IIS:\sites\$qp\$name"
  }
  else{
    $alias = "IIS:\sites\$name"
  }
  
  $app = Get-Item $alias -ErrorAction SilentlyContinue

  if ($app) {      
    $path =  $app.PhysicalPath

    Remove-Item $alias -Recurse -Force
    Write-Output "$qp\$name deleted"

    if (Test-Path $path){
        Remove-Item $path -Recurse
        Write-Output "$qp\$name files removed"
    }
  }
}

DeleteService -name $notificationSender -installRoot $installRoot
DeleteService -name $actionsService -installRoot $installRoot
DeleteSite -qp "QP8" -name $admin
DeleteSite -name $siteSync
Remove-CustomerCode -CustomerCode $customerCode