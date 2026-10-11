param([Parameter(Mandatory=$true)][ValidateSet('Enabled','Disabled')][string]$Mode)
$ErrorActionPreference='Stop'
$kitRoot=Split-Path -Parent $PSScriptRoot
$copyRoot=Join-Path $kitRoot 'melon-test-game'
if(Get-CimInstance Win32_Process | Where-Object {$_.ExecutablePath -eq (Join-Path $copyRoot 'Ring Of Pain.exe')}){throw 'Close the framework copy first. Settings are read at startup.'}
$path=Join-Path $copyRoot 'UserData\RingOfPainModKit\Mods\double-candle-all-items\manifest.json'
$m=Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
$m.enabled=($Mode -eq 'Enabled')
$m | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $path -Encoding utf8
Write-Output "Example $Mode. Launch the framework copy to apply."
