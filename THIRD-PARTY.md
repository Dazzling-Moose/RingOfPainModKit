# Upstream tooling

Only our source and three compiled project DLLs are packaged. Setup downloads upstream distributions separately. Keep upstream license notices in the downloaded folders.

- [MelonLoader 0.7.3](https://github.com/LavaGang/MelonLoader/tree/v0.7.3): plugin loader; see upstream Apache-2.0 license and bundled notices.
- [BepInEx](https://github.com/BepInEx/BepInEx): investigated but not selected; see upstream LGPL-2.1 license.
- [Il2CppDumper](https://github.com/Perfare/Il2CppDumper): private analysis; MIT.
- [Cpp2IL](https://github.com/SamboyCoding/Cpp2IL) and [Il2CppInterop](https://github.com/BepInEx/Il2CppInterop): supplied/downloaded through selected loader for local wrapper generation; see upstream license and loader notices.
- [Harmony](https://github.com/pardeike/Harmony) and MelonLoader's bundled Harmony distribution: hook implementation; upstream MIT and bundled notices apply.
- [.NET](https://github.com/dotnet): portable SDK/runtime; MIT and third-party notices in the upstream distributions.
- [Valve Steam initialization documentation](https://partner.steamgames.com/doc/sdk/api#SteamAPI_RestartAppIfNecessary): explains steam_appid.txt behavior. No Steam API behavior was hooked or patched.

Game assets, executables, metadata, generated wrappers, and private decompiled artifacts are not licensed by this project's license and must not be redistributed with it. Download checksums in scripts/toolchain.json pin the distributions actually tested locally; they are integrity fingerprints, not an upstream code-signing guarantee.
