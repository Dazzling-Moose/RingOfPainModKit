$ErrorActionPreference='Stop'
$kitRoot=Split-Path -Parent $PSScriptRoot
$artifacts=Join-Path $kitRoot 'artifacts'
New-Item -ItemType Directory -Path $artifacts -Force|Out-Null
$stage=Join-Path $artifacts ('package-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $stage|Out-Null
foreach($name in @('README.md','LICENSE','THIRD-PARTY.md','Launch-Framework.cmd','docs')){Copy-Item -LiteralPath (Join-Path $kitRoot $name) -Destination $stage -Recurse}
$scriptStage=Join-Path $stage 'scripts'
New-Item -ItemType Directory -Path $scriptStage|Out-Null
foreach($name in @('Setup.ps1','Build.ps1','Launch.ps1','Set-Example.ps1','Disable-Framework.ps1','Package.ps1','toolchain.json','game-fingerprint.json')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $scriptStage}
foreach($project in @('SDK','Host','DoubleCandleAllItems','Tests','MelonSmokeTest','InspectInterop')){
    $out=Join-Path $stage ('src\'+$project);New-Item -ItemType Directory -Path $out -Force|Out-Null
    Get-ChildItem -LiteralPath (Join-Path $kitRoot ('src\'+$project)) -File | Where-Object {$_.Extension -in @('.cs','.csproj','.json')} | ForEach-Object {Copy-Item -LiteralPath $_.FullName -Destination $out}
}
$bin=Join-Path $stage 'bin';New-Item -ItemType Directory -Path $bin|Out-Null
foreach($file in @('Host\bin\Release\net6.0\RingOfPainModKit.dll','SDK\bin\Release\net6.0\RingOfPainModKit.SDK.dll','DoubleCandleAllItems\bin\Release\net6.0\DoubleCandleAllItems.dll')){Copy-Item -LiteralPath (Join-Path $kitRoot ('src\'+$file)) -Destination $bin}
# Explicit allowlist excludes game files, wrapper DLLs, downloads, decompiled artifacts, and save backups.
$zip=Join-Path $artifacts 'RingOfPainModKit-0.1.0.zip'
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -Force
Write-Output $zip

