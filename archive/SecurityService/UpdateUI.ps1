$source = "https://github.com/DuendeSoftware/IdentityServer.Quickstart.UI.AspNetIdentity/archive/main.zip"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Invoke-WebRequest $source -OutFile ui.zip

Expand-Archive ui.zip

if (!(Test-Path -Path QuickStart)) { mkdir QuickStart }
if (!(Test-Path -Path Views))       { mkdir Views }
if (!(Test-Path -Path wwwroot))     { mkdir wwwroot }

copy .\ui\IdentityServer.Quickstart.UI.AspNetIdentity-main\Quickstart\* QuickStart -recurse -force
copy .\ui\IdentityServer.Quickstart.UI.AspNetIdentity-main\Views\* Views -recurse -force
copy .\ui\IdentityServer.Quickstart.UI.AspNetIdentity-main\wwwroot\* wwwroot -recurse -force

# Now split the Quickstart into Controllers and View models
if (!(Test-Path -Path Controllers)) { mkdir Controllers }
if (!(Test-Path -Path ViewModels)) { mkdir ViewModels }

$controllerDir = Join-Path $PSScriptRoot "\Controllers"
$viewModelDir = Join-Path $PSScriptRoot "\ViewModels\"

foreach ($d in $dir){
    $controllerFilter = @('*Controller*.cs')
    if (!(Test-Path -Path $controllerDir\$d)) { mkdir Controllers\$d }
    if (!(Test-Path -Path $viewModelDir\$d)) { mkdir ViewModels\$d }
    
    Get-ChildItem -Path QuickStart\$d -Include *Controller*.cs -Recurse | ForEach-Object { 
    Copy-Item -Path $_.FullName -Destination $controllerDir\$d
    }

    if (!(Test-Path -Path $controllerDir\$d)) { mkdir ViewModels\$d }
    Get-ChildItem -Path QuickStart\$d -Exclude *Controller*.cs -Recurse | ForEach-Object { 
    Copy-Item -Path $_.FullName -Destination $viewModelDir\$d
    }
}
copy .\ui\IdentityServer.Quickstart.UI.AspNetIdentity-main\Quickstart\*.cs -force

del ui.zip
del ui -recurse
del QuickStart -recurse