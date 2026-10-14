using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaTradingExpandedItemPickerFix
{
    internal static class ItemPickerPatch
    {
        private const string HarmonyId =
            "vorsel.vanillatradingexpanded.itempickerfix";
        private const string WindowTypeName =
            "VanillaTradingExpanded.Window_SelectItemForContract";
        private const string ContractTypeName =
            "VanillaTradingExpanded.Contract";
        private const float RowHeight = 35f;
        private const float RowContentsHeight = 32f;

        private static Type windowType;
        private static Type contractType;
        private static MethodInfo doWindowContentsMethod;
        private static FieldInfo allItemsField;
        private static FieldInfo searchKeyField;
        private static FieldInfo scrollPositionField;
        private static FieldInfo contractField;
        private static FieldInfo contractItemField;
        private static FieldInfo contractStuffField;
        private static ConditionalWeakTable<object, WindowState> windowStates =
            new ConditionalWeakTable<object, WindowState>();
        private static bool runtimeDisabled;

        private sealed class WindowState
        {
            internal readonly List<ThingDef> SourceItems;
            internal readonly List<ThingDef> SortedItems;
            internal List<ThingDef> FilteredItems;
            internal string SearchKey;

            internal WindowState(List<ThingDef> sourceItems)
            {
                SourceItems = sourceItems;
                SortedItems = sourceItems
                    .Where(item => item != null)
                    .OrderBy(GetCategoryIndex)
                    .ThenBy(item => item.label)
                    .ToList();
                FilteredItems = SortedItems;
            }

            internal bool UpdateSearch(string searchKey)
            {
                if (string.Equals(SearchKey, searchKey, StringComparison.Ordinal))
                {
                    return false;
                }

                SearchKey = searchKey;
                if (string.IsNullOrEmpty(searchKey))
                {
                    FilteredItems = SortedItems;
                    return true;
                }

                FilteredItems = SortedItems
                    .Where(item => item.label.IndexOf(
                        searchKey,
                        StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .ToList();
                return true;
            }
        }

        internal static void Install()
        {
            windowType = AccessTools.TypeByName(WindowTypeName);
            contractType = AccessTools.TypeByName(ContractTypeName);
            if (windowType == null || contractType == null)
            {
                Log.Error(
                    "[VTE Item Picker Fix] Supported Vanilla Trading Expanded "
                    + "types were not found; no patch was applied.");
                return;
            }

            doWindowContentsMethod = AccessTools.Method(
                windowType,
                "DoWindowContents",
                new[] { typeof(Rect) });
            allItemsField = AccessTools.Field(windowType, "allItems");
            searchKeyField = AccessTools.Field(windowType, "searchKey");
            scrollPositionField = AccessTools.Field(windowType, "scrollPosition");
            contractField = AccessTools.Field(windowType, "contract");
            contractItemField = AccessTools.Field(contractType, "item");
            contractStuffField = AccessTools.Field(contractType, "stuff");

            if (!RecognizesTarget())
            {
                Log.Error(
                    "[VTE Item Picker Fix] The installed Vanilla Trading Expanded "
                    + "item-picker layout is not recognized; no patch was applied.");
                return;
            }

            Patches patchInfo = Harmony.GetPatchInfo(doWindowContentsMethod);
            if (patchInfo != null && patchInfo.Owners.Contains(HarmonyId))
            {
                return;
            }

            new Harmony(HarmonyId).Patch(
                doWindowContentsMethod,
                prefix: new HarmonyMethod(
                    typeof(ItemPickerPatch),
                    nameof(DoWindowContentsPrefix)));

            Log.Message(
                "[VTE Item Picker Fix] Optimized contract item picker enabled.");
        }

        private static bool RecognizesTarget()
        {
            return typeof(Window).IsAssignableFrom(windowType)
                && doWindowContentsMethod != null
                && doWindowContentsMethod.ReturnType == typeof(void)
                && allItemsField != null
                && allItemsField.FieldType == typeof(List<ThingDef>)
                && searchKeyField != null
                && searchKeyField.FieldType == typeof(string)
                && scrollPositionField != null
                && scrollPositionField.FieldType == typeof(Vector2)
                && contractField != null
                && contractField.FieldType == contractType
                && contractItemField != null
                && contractItemField.FieldType == typeof(ThingDef)
                && contractStuffField != null
                && contractStuffField.FieldType == typeof(ThingDef);
        }

        private static bool DoWindowContentsPrefix(object __instance, Rect __0)
        {
            if (runtimeDisabled)
            {
                return true;
            }

            try
            {
                List<ThingDef> sourceItems =
                    (List<ThingDef>)allItemsField.GetValue(__instance);
                object contract = contractField.GetValue(__instance);
                if (sourceItems == null || contract == null)
                {
                    throw new InvalidOperationException(
                        "The target window did not provide its item list or contract.");
                }

                WindowState state;
                if (!windowStates.TryGetValue(__instance, out state)
                    || !ReferenceEquals(state.SourceItems, sourceItems))
                {
                    if (state != null)
                    {
                        windowStates.Remove(__instance);
                    }

                    state = new WindowState(sourceItems);
                    windowStates.Add(__instance, state);
                }

                DrawWindow(
                    (Window)__instance,
                    __instance,
                    contract,
                    state,
                    __0);
                return false;
            }
            catch (Exception exception)
            {
                runtimeDisabled = true;
                windowStates = new ConditionalWeakTable<object, WindowState>();
                Log.ErrorOnce(
                    "[VTE Item Picker Fix] Optimization failed safely; the "
                    + "original VTE window will be used for this session. "
                    + exception,
                    1685941037);
                return true;
            }
        }

        private static void DrawWindow(
            Window window,
            object windowInstance,
            object contract,
            WindowState state,
            Rect inRect)
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            Rect searchLabelRect = new Rect(inRect.x, inRect.y, 60f, 24f);
            Widgets.Label(searchLabelRect, "VTE.Search".Translate());

            Rect searchFieldRect = new Rect(
                searchLabelRect.xMax + 5f,
                searchLabelRect.y,
                200f,
                24f);
            string searchKey = (string)searchKeyField.GetValue(windowInstance);
            string editedSearchKey = Widgets.TextField(searchFieldRect, searchKey);
            searchKeyField.SetValue(windowInstance, editedSearchKey);
            Text.Anchor = TextAnchor.UpperLeft;

            if (state.UpdateSearch(editedSearchKey))
            {
                scrollPositionField.SetValue(windowInstance, Vector2.zero);
            }

            Rect outRect = new Rect(inRect);
            outRect.y = searchFieldRect.yMax + 5f;
            outRect.yMax -= 70f;
            outRect.width -= 16f;

            Vector2 scrollPosition =
                (Vector2)scrollPositionField.GetValue(windowInstance);
            Rect viewRect = new Rect(
                0f,
                0f,
                outRect.width - 16f,
                state.FilteredItems.Count * RowHeight);

            Widgets.BeginScrollView(
                outRect,
                ref scrollPosition,
                viewRect,
                true);
            scrollPositionField.SetValue(windowInstance, scrollPosition);

            int firstVisible = Mathf.Max(
                0,
                Mathf.FloorToInt(scrollPosition.y / RowHeight));
            int lastVisibleExclusive = Mathf.Min(
                state.FilteredItems.Count,
                Mathf.CeilToInt(
                    (scrollPosition.y + outRect.height) / RowHeight) + 1);

            try
            {
                for (int index = firstVisible;
                    index < lastVisibleExclusive;
                    index++)
                {
                    DrawItemRow(
                        window,
                        contract,
                        state.FilteredItems[index],
                        viewRect,
                        index * RowHeight);
                }
            }
            finally
            {
                Widgets.EndScrollView();
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        private static void DrawItemRow(
            Window window,
            object contract,
            ThingDef item,
            Rect viewRect,
            float y)
        {
            Rect iconRect = new Rect(0f, y, 24f, RowContentsHeight);
            Widgets.InfoCardButton(iconRect, item);
            iconRect.x += 24f;
            Widgets.ThingIcon(
                iconRect,
                item,
                null,
                null,
                1f,
                null,
                null,
                1f);

            Rect labelRect = new Rect(
                iconRect.xMax + 5f,
                y,
                viewRect.width * 0.7f,
                RowContentsHeight);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, item.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            Rect selectRect = new Rect(
                labelRect.xMax + 10f,
                y,
                100f,
                RowContentsHeight);
            if (!Widgets.ButtonText(
                selectRect,
                "VTE.Select".Translate().ToString()))
            {
                return;
            }

            contractItemField.SetValue(contract, item);
            contractStuffField.SetValue(contract, null);
            SoundDefOf.Click.PlayOneShotOnCamera(null);
            window.Close(true);
        }

        private static ushort GetCategoryIndex(ThingDef item)
        {
            ThingCategoryDef category = item.FirstThingCategory;
            return category == null ? (ushort)0 : category.index;
        }
    }
}
