using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace RichLifeDayNightAndMore.Patches;

internal static class Game1ClassPatch
{
	internal static void ApplyPatch(Harmony harmony)
	{
		harmony.Patch(
			original: AccessTools.Method(typeof(Game1), nameof(Game1.changeMusicTrack)), 
			prefix: new(typeof(Game1ClassPatch), nameof(PrefixChangeMusicTrack))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(Game1), nameof(Game1.getModeratelyDarkTime)), 
			postfix: new(typeof(Game1ClassPatch), nameof(PostfixGetModeratelyDarkTime))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(Game1), nameof(Game1.getStartingToGetDarkTime)), 
			postfix: new(typeof(Game1ClassPatch), nameof(PostfixGetStartingToGetDarkTime))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(Game1), nameof(Game1.getTrulyDarkTime)), 
			postfix: new(typeof(Game1ClassPatch), nameof(PostfixGetTrulyDarkTime))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(Game1), nameof(Game1.UpdateGameClock)), 
			postfix: new(typeof(Game1ClassPatch), nameof(PostfixUpdateGameClock))
		);
	}

	// 计算光照
	internal static Color GetCurrentLight(GameLocation Location, Color Day, Color Night, Color? RainColor = null)
	{
        const float DayPercent = 0.8f;
        Color Raining = RainColor ?? Day;
		float DayStartSolarHeight = RichLifeDayNightAndMore.Config.DayStartAtSolarHeight;
        float NightStartSolarHeight = RichLifeDayNightAndMore.Config.NightStartAtSolarHeight;
		float NightDarkness = RichLifeDayNightAndMore.Config.NightDarkness;
        float RainingDarkness = RichLifeDayNightAndMore.Config.RainingDarkness;
		if (!Location.IsOutdoors)
		{
			NightDarkness = 1f;
			RainingDarkness = 1f;
		}
        float SolarHeight = (float)SolarCalculator.GetSolarHeight();
		if (SolarHeight < NightStartSolarHeight)
		{
			return (Location.IsRainingHere() ? Raining : Night) * NightDarkness;
		}
		else if (SolarHeight < DayStartSolarHeight)
		{
			float Ratio = SolarHeight >= 0f ? 
				MathHelper.Lerp(0f, DayPercent, 1f - SolarHeight / DayStartSolarHeight) : 
				MathHelper.Lerp(DayPercent, 1f, SolarHeight / NightStartSolarHeight)
			;
			Color LightColor = Location.IsRainingHere() ? 
				Raining : 
				Color.Lerp(Day, Night, Ratio)
			;
			float Transparency = Location.IsRainingHere() ? 
				MathHelper.Lerp(RainingDarkness, NightDarkness, Ratio) :
                NightDarkness * Ratio
            ;
			return LightColor * Transparency;
		}
		else
		{
			return Location.IsRainingHere() ? Raining * RainingDarkness : Day;
        }
	}

	// 禁止游戏在天黑时停止播放bgm
	private static bool PrefixChangeMusicTrack(string newTrackName)
	{
		if (newTrackName == "none" && !Game1.eventUp && Game1.isDarkOut(Game1.currentLocation) && Game1.IsPlayingBackgroundMusic)
		{
			return false;
		}
		return true;
	}

	private static void PostfixGetModeratelyDarkTime(ref int __result)
	{
		__result = SolarCalculator.GetTimeOfDark().ModeratelyDarkTime;
	}

	private static void PostfixGetStartingToGetDarkTime(ref int __result)
	{
		__result = SolarCalculator.GetTimeOfDark().StartingToGetDarkTime;
	}

	private static void PostfixGetTrulyDarkTime(ref int __result)
	{
		__result = SolarCalculator.GetTimeOfDark().TrulyDarkTime;
	}

	// 光照覆写
	private static void PostfixUpdateGameClock()
	{
		Game1.outdoorLight = GetCurrentLight(Game1.currentLocation, Game1.ambientLight, Game1.eveningColor);
	}
}
