# RimWorld Mod Fixes

A source archive containing five small, standalone RimWorld performance, behavior, and compatibility fixes:

- **MoeLotl: Rigor Mortis - Float Menu Fix** — reduces repeated expensive float-menu checks and preloads affected incantation graphics. [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3783214874)
- **Milira: Wings of Democracy - Float Menu Fix** — filters unrelated WorkGiver checks, removes duplicate menu work, and caches repeated storage searches. [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3783936979)
- **Milira Race - Float Menu Fix** — avoids unnecessary solar-fuel WorkGiver scans and repeated storage searches. [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3783931796)
- **MoeLotl Settlement Trade Faction Fix** — assigns traded MoeLotls to the correct faction and includes optional Real Faction Guest compatibility. [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3783743602)
- **Vanilla Trading Expanded - Item Picker Fix** — caches sorting and search results and draws only visible rows in the futures contract item picker. [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3803663970)

Each subdirectory is a standalone RimWorld mod. See its `About/About.xml` for dependencies, load order, and full details.

## Float-menu fixes: RimWorld 1.7 support

The Rigor Mortis, Milira Race, and Wings of Democracy float-menu fixes declare support for RimWorld 1.6 and 1.7. Their existing DLLs were retained after manual 1.7 gameplay checks; this update changes the version declarations and load routing, not the patch code or binaries.

On either game version, each fix loads its root package when Float Menu Performance Optimizer is absent. When the optimizer is active, the standalone fix selects `Inactive` and does not load its DLL, allowing the integrated module to take over without duplicate patches. Keep `Inactive/Strings/Inactive.txt` in the package.

## VTE item picker: RimWorld 1.6 and 1.7

The item-picker fix supports RimWorld 1.6 and 1.7. Both load the tested Release DLL from `1.6/Assemblies`, compiled against the preserved RimWorld 1.6 references and verified in-game on both versions. Its stable package ID is `vorsel.vanillatradingexpanded.itempickerfix`.

The `Inactive` content root is reserved for per-version deactivation after an upstream repair has been verified; the fix is currently active on both versions. A newer upstream DLL date alone does not deactivate it. Temporary performance monitors and test logs are not included in this repository.

## Workstation workflow

Repository mirror: `D:\sourcefiles\mod\RimWorld\RimWorldModFixes`. Remote: `https://github.com/vorsell/RimWorldModFixes.git`.

On the current workstation, the corresponding directories under `G:\SteamLibrary\steamapps\common\RimWorld\Mods` are the authoritative development, build-output, deployment, and game-test copies. This repository is the backup and Git/GitHub synchronization mirror. For a backup or release, compare and copy the intended files from each G-drive Mod directory into its matching subdirectory here, verify relative paths and hashes, inspect the complete repository diff, then commit and push. Do not develop in this mirror first, and never treat Steam Workshop copies as authoritative source.
