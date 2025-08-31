using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace RichLifeDayNightAndMore.Patches;

internal static class FarmHouseClassPatch
{
	internal static void ApplyPatch(Harmony harmony)
	{
		harmony.Patch(
			original: AccessTools.Method(typeof(FarmHouse), "_updateAmbientLighting"), 
			postfix: new(typeof(FarmHouseClassPatch), nameof(PostfixUpdateAmbientLighting))
		);
	}

	// π‚’’∏≤–¥
	private static void PostfixUpdateAmbientLighting(ref FarmHouse __instance)
	{
		Color NightLightingColor = RichLifeDayNightAndMore.ModHelper.Reflection.GetField<Color>(__instance, "nightLightingColor", true).GetValue();
		Color RainLightingColor = RichLifeDayNightAndMore.ModHelper.Reflection.GetField<Color>(__instance, "rainLightingColor", true).GetValue();
		if (Game1.isStartingToGetDarkOut(__instance) || __instance.LightLevel > 0f)
		{
			Game1.ambientLight = Game1ClassPatch.GetCurrentLight(__instance, Color.Black, NightLightingColor, RainLightingColor);
		}
		else
		{
            Game1.ambientLight = Game1ClassPatch.GetCurrentLight(__instance, Color.White, NightLightingColor, RainLightingColor);
		}
	}
}
