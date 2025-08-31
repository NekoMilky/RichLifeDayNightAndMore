using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace RichLifeDayNightAndMore.Patches;

internal static class IslandFarmHouseClassPatch
{
	internal static void ApplyPatch(Harmony harmony)
	{
		harmony.Patch(
			AccessTools.Method(typeof(IslandFarmHouse), "_updateAmbientLighting"),
			postfix: new(typeof(IslandFarmHouseClassPatch), nameof(PostfixUpdateAmbientLighting))
		);
	}

	// π‚’’∏≤–¥
	private static void PostfixUpdateAmbientLighting(ref IslandFarmHouse __instance)
	{
		Color NightLightingColor = RichLifeDayNightAndMore.ModHelper.Reflection.GetField<Color>((object)__instance, "nightLightingColor", true).GetValue();
		Color RainLightingColor = RichLifeDayNightAndMore.ModHelper.Reflection.GetField<Color>((object)__instance, "rainLightingColor", true).GetValue();
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
