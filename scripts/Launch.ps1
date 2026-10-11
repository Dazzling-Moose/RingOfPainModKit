$ErrorActionPreference='Stop'
$kitRoot=Split-Path -Parent $PSScriptRoot
$copyRoot=Join-Path $kitRoot 'melon-test-game'
$exe=Join-Path $copyRoot 'Ring Of Pain.exe'
if(!(Test-Path -LiteralPath $exe)){throw 'Run Setup.ps1 first.'}
if(!(Get-Process steam -ErrorAction SilentlyContinue)){throw 'Open Steam and sign in before launching the copied game.'}
if(Get-CimInstance Win32_Process | Where-Object {$_.ExecutablePath -eq $exe}){throw 'Framework copy is already running.'}
New-Item -ItemType Directory -Path (Join-Path $kitRoot 'evidence') -Force | Out-Null
$log=Join-Path $kitRoot 'evidence\unity-current.log'
$p=Start-Process -FilePath $exe -WorkingDirectory $copyRoot -ArgumentList @('-logFile',('"'+$log+'"'),'--melonloader.disablestartscreen') -PassThru
Write-Output "Started copy process $($p.Id). Loader log: $copyRoot\MelonLoader\Latest.log"
