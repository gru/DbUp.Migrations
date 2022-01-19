function BuildProjects([string] $dir, [string] $filter, [string] $properties = "") {
  $projects = Get-ChildItem $dir $filter -Recurse
  foreach ($project in $projects) {
    dotnet build $project $properties
  }  
}

function PushPackages([string] $dir, [string] $filter) {
  $packages = Get-ChildItem $dir $filter -Recurse
  foreach ($package in $packages) {
    nuget push $package $ApiKey -Source $Server -SkipDuplicate
  }  
}

function DeletePackages([string] $dir) {
  $packages = Get-ChildItem $dir '*.nupkg' -Recurse
  foreach ($package in $packages) {
    "Removing package $package"
    Remove-Item $package 
  }  
}

$Server = 'http://localhost'
$ApiKey = '112233'

DeletePackages $PSScriptRoot

BuildProjects $PSScriptRoot "Service.Migrations.csproj" "/p:Version=1.0.0"
PushPackages $PSScriptRoot "Service.Migrations.*.nupkg"

BuildProjects $PSScriptRoot "Service2.Migrations.csproj" "/p:Version=1.0.0"
PushPackages $PSScriptRoot "Service2.Migrations.*.nupkg"