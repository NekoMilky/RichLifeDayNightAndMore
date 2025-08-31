using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using RichLifeDayNightAndMore.PassiveFestivals;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Objects;
using xTile.Dimensions;
using xTile.Tiles;

namespace RichLifeDayNightAndMore.Patches;

internal static class GameLocationClassPatch
{
	internal static void ApplyPatch(Harmony harmony)
	{
		harmony.Patch(
			original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)), 
			prefix: new(typeof(GameLocationClassPatch), nameof(PrefixAnswerDialogueAction))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)), 
			prefix: new(typeof(GameLocationClassPatch), nameof(PrefixCheckAction))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.OnDayStarted)), 
			postfix: new(typeof(GameLocationClassPatch), nameof(PostfixOnDayStarted))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTenMinuteUpdate)), 
			postfix: new(typeof(GameLocationClassPatch), nameof(PostfixPerformTenMinuteUpdate))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(GameLocation), "_updateAmbientLighting"), 
			postfix: new(typeof(GameLocationClassPatch), nameof(PostfixUpdateAmbientLighting))
		);
	}

	private static void IceReplacesWater(Chest Fridge)
	{
		if (Fridge == null)
		{
			return;
		}
		Inventory Inventory = Fridge.Items;
		IList<Item> Items = Inventory.GetRange(0, Inventory.Count);
		for (int i = 0; i < Items.Count; i++)
		{
			Item Item = Items[i];
			if (!(Item.ItemId != RichLifeDayNightAndMore.ModCPPrefix + "Water"))
			{
				int Count = Item.Stack;
				int Quality = Item.Quality;
				Items[i] = ItemRegistry.Create(RichLifeDayNightAndMore.ModCPPrefix + "Ice", Count, Quality);
			}
		}
		Inventory.OverwriteWith(Items);
	}

	private static void SwitchTileState(GameLocation Location, string Tile)
	{
		string[] MapProperty = Location.GetMapPropertySplitBySpaces(Tile);
		for (int i = 0; i < MapProperty.Length; i += 4)
		{
			if (!ArgUtility.TryGet(MapProperty, i, out var LayerId, out var Error, allowBlank: true, "string layerId") || !ArgUtility.TryGetPoint(MapProperty, i + 1, out var TilePoint, out Error, "Point position") || !ArgUtility.TryGetInt(MapProperty, i + 3, out var TileIndex, out Error, "int tileIndex"))
			{
				Location.LogMapPropertyError(Tile, MapProperty, Error);
			}
			else if ((TileIndex != 720 && TileIndex != 726) || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				Tile tile = Location.map.RequireLayer(LayerId).Tiles[TilePoint.X, TilePoint.Y];
				if (tile != null)
				{
					tile.TileIndex = TileIndex;
				}
			}
		}
	}

	// 回答特定对话行为
	private static bool PrefixAnswerDialogueAction(ref GameLocation __instance, ref bool __result, string questionAndAnswer)
	{
		PassiveFestival Festival = ModFestival.GetModPassiveFestivalToday();
		if (Festival == null)
		{
			return true;
		}
		bool? OverrideResult = Festival.AnswerDialogueAction(__instance, questionAndAnswer);
		if (!OverrideResult.HasValue)
		{
			return true;
		}
		__result = OverrideResult.Value;
		return false;
	}

	// 检查地图特定地块行为
	private static bool PrefixCheckAction(ref GameLocation __instance, ref bool __result, Location tileLocation)
	{
		PassiveFestival Festival = ModFestival.GetModPassiveFestivalToday();
		if (Festival == null)
		{
			return true;
		}
		bool? OverrideResult = Festival.CheckAction(__instance, tileLocation);
		if (!OverrideResult.HasValue)
		{
			return true;
		}
		__result = OverrideResult.Value;
		return false;
	}

	// 冰箱内水转换为冰块
	private static void PostfixOnDayStarted(ref GameLocation __instance)
	{
		if (__instance is not FarmHouse && __instance is not IslandFarmHouse)
		{
			return;
		}
		IceReplacesWater(__instance.GetFridge());
		foreach (StardewValley.Object Object in __instance.objects.Values)
		{
			if (Object != null && Object.bigCraftable.Value && Object is Chest Container && Container.fridge.Value)
			{
				IceReplacesWater(Container);
			}
		}
	}

	// 昼夜地块转换
	private static void PostfixPerformTenMinuteUpdate(ref GameLocation __instance)
	{
		if (Game1.currentLocation.Equals(__instance))
		{
			SwitchTileState(__instance, (!Game1.isTimeToTurnOffLighting(__instance) && (!__instance.IsRainingHere() || __instance.Name == "SandyHouse")) ? "DayTiles" : "NightTiles");
		}
	}

	// 光照覆写
	private static void PostfixUpdateAmbientLighting(ref GameLocation __instance)
	{
		if (!__instance.IsOutdoors || __instance.ignoreOutdoorLighting.Value)
		{
			Color IndoorLightingColor = RichLifeDayNightAndMore.ModHelper.Reflection.GetField<Color>(__instance, "indoorLightingColor", true).GetValue();
			Color IndoorLightingNightColor = RichLifeDayNightAndMore.ModHelper.Reflection.GetField<Color>(__instance, "indoorLightingNightColor", true).GetValue();
			Game1.ambientLight = Game1ClassPatch.GetCurrentLight(__instance, IndoorLightingColor, IndoorLightingNightColor);
		}
		else
		{
			Game1.ambientLight = Game1ClassPatch.GetCurrentLight(__instance, Color.White, Game1.eveningColor, new(255, 200, 80));
		}
	}
}
