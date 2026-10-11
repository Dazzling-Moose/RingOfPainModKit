param([switch]$Enable)
$ErrorActionPreference='Stop'
$kitRoot=Split-Path -Parent $PSScriptRoot
$copyRoot=(Resolve-Path -LiteralPath (Join-Path $kitRoot 'melon-test-game')).Path
if(Get-CimInstance Win32_Process | Where-Object {$_.ExecutablePath -eq (Join-Path $copyRoot 'Ring Of Pain.exe')}){throw 'Close the framework copy first.'}
$active=Join-Path $copyRoot 'version.dll'
$inactive=Join-Path $copyRoot 'version.dll.ropmk-disabled'
if($Enable){if(Test-Path -LiteralPath $active){throw 'Loader already enabled'};Move-Item -LiteralPath $inactive -Destination $active}
else {if(Test-Path -LiteralPath $inactive){throw 'Loader already disabled'};Move-Item -LiteralPath $active -Destination $inactive}
Write-Output 'Loader toggle complete in the copied game only. Saves are not restored or changed by this script.'
