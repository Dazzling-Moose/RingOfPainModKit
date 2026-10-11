param([Parameter(Mandatory=$true)][string]$SourceGame,[switch]$ValidateOnly)
$ErrorActionPreference='Stop'
$kitRoot=Split-Path -Parent $PSScriptRoot
$sourceRoot=(Resolve-Path -LiteralPath $SourceGame).Path
$fingerprint=Get-Content (Join-Path $PSScriptRoot 'game-fingerprint.json') -Raw | ConvertFrom-Json
foreach($entry in $fingerprint.PSObject.Properties){
    $file=Join-Path $sourceRoot $entry.Name
    if((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $entry.Value){throw "Unsupported/modified source: $($entry.Name)"}
}
if($ValidateOnly){Write-Output 'Four original source hashes verified. No files changed.';return}
$copyRoot=Join-Path $kitRoot 'melon-test-game'
if(Test-Path -LiteralPath $copyRoot){throw 'Test-copy directory already exists. Reuse it; Setup never overwrites an existing game copy.'}
$tools=Join-Path $kitRoot 'tools'
New-Item -ItemType Directory -Path $tools -Force | Out-Null
$toolchain=Get-Content (Join-Path $PSScriptRoot 'toolchain.json') -Raw | ConvertFrom-Json
foreach($entry in $toolchain.PSObject.Properties){
    $archive=Join-Path $tools ($entry.Name+'.zip')
    if(!(Test-Path -LiteralPath $archive)){Invoke-WebRequest -Uri $entry.Value.url -OutFile $archive}
    if((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $entry.Value.sha256){throw "Tool download fingerprint mismatch: $($entry.Name)"}
}
New-Item -ItemType Directory -Path $copyRoot | Out-Null
foreach($name in @('Ring Of Pain.exe','UnityPlayer.dll','GameAssembly.dll','UnityCrashHandler64.exe')){
    Copy-Item -LiteralPath (Join-Path $sourceRoot $name) -Destination $copyRoot
}
Copy-Item -LiteralPath (Join-Path $sourceRoot 'Ring Of Pain_Data') -Destination $copyRoot -Recurse
# The original directory remains untouched; the clean copy is also a binary backup.
$backup=Join-Path $kitRoot ('private-save-backup\'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
$saveSource=Join-Path $env:USERPROFILE 'AppData\LocalLow\Twice Different\Ring Of Pain'
if(Test-Path -LiteralPath $saveSource){New-Item -ItemType Directory -Path $backup -Force|Out-Null;Copy-Item -LiteralPath $saveSource -Destination $backup -Recurse}
Expand-Archive -LiteralPath (Join-Path $tools 'melonloader.zip') -DestinationPath $copyRoot
Expand-Archive -LiteralPath (Join-Path $tools 'dotnet-sdk.zip') -DestinationPath (Join-Path $tools 'dotnet') -Force
foreach($archive in @('core-runtime.zip','desktop-runtime.zip')){
    Expand-Archive -LiteralPath (Join-Path $tools $archive) -DestinationPath (Join-Path $tools 'dotnet6') -Force
}
Copy-Item -LiteralPath (Join-Path $tools 'dotnet6') -Destination (Join-Path $copyRoot 'dotnet') -Recurse
Set-Content -LiteralPath (Join-Path $copyRoot 'steam_appid.txt') -Value '998740' -Encoding ascii
Write-Output 'Prepared clean loader copy and private save backup. Start Steam, run Launch.ps1 once, wait for the menu, then close the game and run Build.ps1.'
