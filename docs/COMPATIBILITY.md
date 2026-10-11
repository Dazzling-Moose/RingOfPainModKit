# Compatibility

| Component | Result | Evidence |
|---|---|---|
| Ring of Pain local Windows x64 1.6.02f2; Unity 2019.4.29f1 | Exact four-file fingerprint supported | Source and live host hash checks |
| MelonLoader 0.7.3 x64 | Plugin loading and game hooks passed | Smoke, startup and chooser log markers |
| Portable .NET 6.0.36 core + WindowsDesktop | Works with selected loader | Game stayed running through user gameplay |
| Portable SDK 8.0.425, net6.0 targets | Builds succeeded | Managed test runner and host/example builds |
| BepInEx 6 build 788+5b766a3 | Failed before plugin initialization | SEH/AccessViolation in IL2CPPChainloader.OnInvokeMethod |
| BepInEx build 787+7cb1246 | Same startup failure | Preserved error log |
| BepInEx v6.0.0-pre.2 (internal build 697) | Same startup failure | Local runtime trial |
| Different game build/platform/store/fingerprint | Unsupported | Host fails closed before patching |

The exact cause of the BepInEx failures was not established. MelonLoader was selected after local testing, not merely from a compatibility claim. The current loader targets .NET 6; SDK 8 emits lifecycle warnings for net6 targets. Changing the target framework without checking loader compatibility is not a supported fix.

A version string alone is insufficient. Fingerprints intentionally reject modified binaries (including the earlier candle byte patch) or a different distribution. Do not remove the guard to force compatibility. Regenerate analysis and repeat small tests for a new build.
