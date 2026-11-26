# Determines the package validation baseline from version.json and the versions published on nuget.org:
# the latest stable release of the major version in version.json that all packages share.
# Prints nothing while the major version has no release yet: a new major version may contain breaking changes.
# Usage: dotnet pack "-p:PackageValidationBaselineVersion=$(./eng/Get-PackageValidationBaseline.ps1)"

[CmdletBinding()]
param(
  [ValidateNotNullOrEmpty()]
  [string] $VersionFile = (Join-Path $PSScriptRoot '..' 'version.json'),

  [ValidateNotNullOrEmpty()]
  [string[]] $PackageIds = @(
    'MyCSharp.HttpUserAgentParser',
    'MyCSharp.HttpUserAgentParser.AspNetCore',
    'MyCSharp.HttpUserAgentParser.MemoryCache'
  ),

  [ValidateNotNullOrEmpty()]
  [string] $FeedUrl = 'https://api.nuget.org/v3-flatcontainer'
)

$ErrorActionPreference = 'Stop'

# ConvertFrom-Json accepts the comments of version.json; a prerelease suffix ("4.1-preview") does not change the major version
$versionJson = Get-Content -Path $VersionFile -Raw | ConvertFrom-Json
$major = ([version](($versionJson.version -split '-')[0])).Major

$shared = $null
foreach ($packageId in $PackageIds) {
  try {
    $published = @((Invoke-RestMethod -Uri "$FeedUrl/$($packageId.ToLowerInvariant())/index.json").versions)
  }
  catch [Microsoft.PowerShell.Commands.HttpResponseException] {
    if ($_.Exception.Response.StatusCode -ne [System.Net.HttpStatusCode]::NotFound) { throw }
    $published = @() # never published
  }

  $releases = @($published | Where-Object { $_ -notmatch '-' -and ([version]$_).Major -eq $major })
  $shared = if ($null -eq $shared) { $releases } else { @($shared | Where-Object { $releases -contains $_ }) }
}

$baseline = $shared | Sort-Object -Property { [version]$_ } -Descending | Select-Object -First 1
if ($baseline) {
  Write-Output $baseline
}
