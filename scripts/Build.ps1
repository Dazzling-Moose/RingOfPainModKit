param([switch]$NoInstall)
$ErrorActionPreference='Stop'
$kitRoot=Split-Path -Parent $PSScriptRoot
$env:DOTNET_CLI_HOME=Join-Path $kitRoot 'tools\cli-home'
$env:NUGET_PACKAGES=Join-Path $kitRoot 'tools\nuget-packages'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false'
$dotnet=Join-Path $kitRoot 'tools\dotnet\dotnet.exe'
if(!(Test-Path (Join-Path $kitRoot 'melon-test-game\MelonLoader\Il2CppAssemblies\Il2CppRingOfPainRuntime.dll'))){throw 'Start the loader copy once to generate game interop assemblies, then close it.'}
foreach($project in @('SDK','Host','DoubleCandleAllItems','Tests')){
    & $dotnet build (Join-Path $kitRoot "src\$project\$project.csproj") -c Release --nologo
    if($LASTEXITCODE -ne 0){throw "Build failed: $project"}
}
& (Join-Path $kitRoot 'tools\dotnet6\dotnet.exe') (Join-Path $kitRoot 'src\Tests\bin\Release\net6.0\Tests.dll')
if($LASTEXITCODE -ne 0){throw 'Managed tests failed'}
if($NoInstall){return}
$copyRoot=Join-Path $kitRoot 'melon-test-game'
$exe=Join-Path $copyRoot 'Ring Of Pain.exe'
if(Get-CimInstance Win32_Process | Where-Object {$_.ExecutablePath -eq $exe}){throw 'Close the framework copy before installing DLLs. Build succeeded.'}
$modDir=Join-Path $copyRoot 'UserData\RingOfPainModKit\Mods\double-candle-all-items'
foreach($dir in @((Join-Path $copyRoot 'Mods'),(Join-Path $copyRoot 'UserLibs'),$modDir)){New-Item -ItemType Directory -Path $dir -Force|Out-Null}
Copy-Item (Join-Path $kitRoot 'src\Host\bin\Release\net6.0\RingOfPainModKit.dll') (Join-Path $copyRoot 'Mods')
Copy-Item (Join-Path $kitRoot 'src\SDK\bin\Release\net6.0\RingOfPainModKit.SDK.dll') (Join-Path $copyRoot 'UserLibs')
Copy-Item (Join-Path $kitRoot 'src\DoubleCandleAllItems\bin\Release\net6.0\DoubleCandleAllItems.dll') $modDir
if(!(Test-Path (Join-Path $modDir 'manifest.json'))){Copy-Item (Join-Path $kitRoot 'src\DoubleCandleAllItems\manifest.json') $modDir}
Write-Output 'Framework installed into clean copy. Existing example enabled setting preserved; new installations default disabled.'
