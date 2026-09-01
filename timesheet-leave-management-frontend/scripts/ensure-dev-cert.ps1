$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$certDir = Join-Path $projectRoot '.cert'
$certPath = Join-Path $certDir 'localhost.pem'
$keyPath = Join-Path $certDir 'localhost.key'

New-Item -ItemType Directory -Force -Path $certDir | Out-Null

$trustCheck = dotnet dev-certs https --check --trust
if ($trustCheck -notmatch 'A trusted certificate was found') {
  throw 'A trusted localhost development certificate was not found. Run "dotnet dev-certs https --trust" once, then start the frontend again.'
}

if (-not (Test-Path $certPath) -or -not (Test-Path $keyPath)) {
  dotnet dev-certs https -ep $certPath --format Pem --no-password
}

Write-Host "Using HTTPS certificate at $certPath"
