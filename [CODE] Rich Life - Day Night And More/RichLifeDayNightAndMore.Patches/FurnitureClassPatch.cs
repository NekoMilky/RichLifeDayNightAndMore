using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Objects;

namespace RichLifeDayNightAndMore.Patches;

internal static class FurnitureClassPatch
{
	internal static void ApplyPatch(Harmony harmony)
	{
		harmony.Patch(
			original: AccessTools.Method(typeof(Furniture), nameof(Furniture.addLights)), 
			prefix: new(typeof(FurnitureClassPatch), nameof(PrefixAddLights))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(Furniture), nameof(Furniture.removeLights)), 
			prefix: new(typeof(FurnitureClassPatch), nameof(PrefixRemoveLights))
		);
	}

	// ÇÐ»»Ò¹ÍíÌùÍ¼
	private static bool PrefixAddLights(ref Furniture __instance)
	{
		if (__instance.furniture_type.Value == 13 && ModFurniture.CheckWindowWithCurtain(__instance.ItemId))
		{
			NetInt SourceIndexOffset = RichLifeDayNightAndMore.ModHelper.Reflection.GetField<NetInt>(__instance, "sourceIndexOffset", true).GetValue();
			__instance.sourceRect.Value = __instance.defaultSourceRect.Value;
			if (Game1.timeOfDay >= 2200 && SolarCalculator.GetDayLength() >= 16.0)
			{
				SourceIndexOffset.Value = 2;
			}
			else
			{
				SourceIndexOffset.Value = 1;
			}
			__instance.RemoveLightGlow();
			return false;
		}
		return true;
	}

	// ÇÐ»»°×ÖçÌùÍ¼
	private static bool PrefixRemoveLights(ref Furniture __instance)
	{
		if (__instance.furniture_type.Value == 13 && ModFurniture.CheckWindowWithCurtain(__instance.ItemId))
		{
			NetInt SourceIndexOffset = RichLifeDayNightAndMore.ModHelper.Reflection.GetField<NetInt>(__instance, "sourceIndexOffset", true).GetValue();
			GameLocation Location = __instance.Location;
			__instance.sourceRect.Value = __instance.defaultSourceRect.Value;
			if (Location != null && Game1.timeOfDay >= 2200)
			{
				SourceIndexOffset.Value = 2;
				__instance.RemoveLightGlow();
			}
			else if (Location != null && Location.IsRainingHere())
			{
				SourceIndexOffset.Value = 1;
				__instance.RemoveLightGlow();
			}
			else
			{
				SourceIndexOffset.Value = 0;
				__instance.AddLightGlow();
			}
			return false;
		}
		return true;
	}
}
