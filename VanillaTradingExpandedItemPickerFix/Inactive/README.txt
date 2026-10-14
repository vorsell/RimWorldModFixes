Inactive content root reserved for a future verified upstream repair.
Strings/Inactive.txt is harmless placeholder content; no DLL is loaded here.

The fix is currently ACTIVE on both RimWorld 1.6 and 1.7.
After confirming that upstream has fixed the item picker, change only the
affected version's LoadFolders.xml entry from <li>1.6</li> to <li>Inactive</li>.
Keep versions that still need the fix routed to 1.6. Fully restart the game.
Record the reason, affected versions and reactivation conditions when actually
retiring the fix. A newer upstream DLL date alone is not a retirement criterion.

此目录为未来确认上游修复后预留的停用内容根，不包含 DLL。
当前 1.6、1.7 均继续加载修复。需要弃用时，仅把 LoadFolders.xml 中相应版本
的 <li>1.6</li> 改为 <li>Inactive</li>，并完整重启游戏。
不要仅因上游 DLL 日期变新就停用；实际弃用时再记录原因及重新启用的条件。
