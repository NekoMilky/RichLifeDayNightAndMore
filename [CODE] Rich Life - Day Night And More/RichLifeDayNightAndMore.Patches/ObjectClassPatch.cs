using System.Text;
using HarmonyLib;
using StardewValley;

namespace RichLifeDayNightAndMore.Patches;

internal static class ObjectClassPatch
{
	internal static void ApplyPatch(Harmony harmony)
	{
		harmony.Patch(
			original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.checkForAction)), 
			postfix: new(typeof(ObjectClassPatch), nameof(PostfixCheckForAction))
		);
	}

	private static void ShowHeliographReport()
	{
		StringBuilder Report = new();
		const string Blank = "   ";
		const string Divide = "--------------";
		const char NewLine = '^';
        Report.Append(
			string.Format(
				RichLifeDayNightAndMore.GetTranslation("heliograph.key.intro"), 
				Game1.year, 
				RichLifeDayNightAndMore.GetTranslation("heliograph.value.season-" + Game1.season switch
                {
                    Season.Spring => "spring",
                    Season.Summer => "summer",
                    Season.Fall => "fall",
                    Season.Winter => "winter",
                    _ => "undefined",
                }), 
				Game1.dayOfMonth
			)
		).Append(Blank);
		Report.Append(NewLine).Append(Divide).Append(Blank);
		Report.Append(NewLine).Append(
			string.Format(
				RichLifeDayNightAndMore.GetTranslation("heliograph.latitude"), 
				RichLifeDayNightAndMore.Config.Latitude
			)
		).Append(Blank);
		double CurrentTime = SolarCalculator.GetTimeFormatted(HourFix: true);
		Report.Append(NewLine).Append(
			RichLifeDayNightAndMore.GetTranslation("heliograph.key.current-time") 
			+ SolarCalculator.GetTimeFormatted24(CurrentTime)
		).Append(Blank);
		double DayLength = SolarCalculator.GetDayLength();
		if (DayLength > 0.0 && DayLength < 24.0)
		{
			Report.Append(NewLine).Append(
				RichLifeDayNightAndMore.GetTranslation("heliograph.key.day-length") 
				+ SolarCalculator.GetTimeFormatted24(DayLength)
			).Append(Blank);
			double Sunrise = 12.0 - DayLength / 2.0;
			double Sunset = 12.0 + DayLength / 2.0;
			Report.Append(NewLine).Append(
				RichLifeDayNightAndMore.GetTranslation("heliograph.key.sunrise") 
				+ SolarCalculator.GetTimeFormatted24(Sunrise)
			).Append(Blank);
			Report.Append(NewLine).Append(
				RichLifeDayNightAndMore.GetTranslation("heliograph.key.sunset") 
				+ SolarCalculator.GetTimeFormatted24(Sunset)
			).Append(Blank);
		}
		else if (DayLength > 12.0)
		{
			Report.Append(NewLine).Append(
				RichLifeDayNightAndMore.GetTranslation("heliograph.value.polar-day")
			).Append(Blank);
		}
		else
		{
			Report.Append(NewLine).Append(
				RichLifeDayNightAndMore.GetTranslation("heliograph.value.polar-night")
			).Append(Blank);
		}
		Report.Append(NewLine).Append(
			string.Format(
				RichLifeDayNightAndMore.GetTranslation("heliograph.ecliptic-obliquity"), 
				RichLifeDayNightAndMore.Config.EclipticObliquity
			)
		).Append(Blank);
		Report.Append(NewLine).Append(
			string.Format(
				RichLifeDayNightAndMore.GetTranslation("heliograph.solar-declination"), 
				SolarCalculator.GetSolarDeclination()
			)
		).Append(Blank);
		Report.Append(NewLine).Append(
			string.Format(
				RichLifeDayNightAndMore.GetTranslation("heliograph.solar-height"), 
				SolarCalculator.GetSolarHeight()
			)
		).Append(Blank);
		Game1.multipleDialogues(new[] { Report.ToString() });
	}

	private static void CheckForActionOnHeliograph(StardewValley.Object Obj, Farmer Who, bool JustCheckingForActivity = false)
	{
		if (!JustCheckingForActivity)
		{
			Obj.shakeTimer = 500;
			Obj.Location.localSound("DwarvishSentry");
			Who.freezePause = 500;
			DelayedAction.functionAfterDelay(delegate
			{
				ShowHeliographReport();
			}, 500);
		}
	}

	// 日光仪显示报告
	private static void PostfixCheckForAction(ref StardewValley.Object __instance, ref bool __result, Farmer who, bool justCheckingForActivity = false)
	{
		if (__instance.QualifiedItemId == "(BC)" + RichLifeDayNightAndMore.ModCPPrefix + "Heliograph")
		{
			CheckForActionOnHeliograph(__instance, who, justCheckingForActivity);
			__result = true;
		}
	}
}
