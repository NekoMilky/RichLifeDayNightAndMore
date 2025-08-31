using System;
using HarmonyLib;
using StardewValley.Locations;

namespace RichLifeDayNightAndMore.Patches;

internal static class ForestClassPatch
{
	internal static void ApplyPatch(Harmony harmony)
	{
		harmony.Patch(
			original: AccessTools.Method(typeof(Forest), nameof(Forest.ShouldTravelingMerchantVisitToday)),
			postfix: new(typeof(ForestClassPatch), nameof(PostfixShouldTravelingMerchantVisitToday))
		);
	}

	// ���������ڱ������յ��첻����
	private static void PostfixShouldTravelingMerchantVisitToday(ref bool __result)
	{
		if (__result && ModFestival.GetModPassiveFestivalToday() != null)
		{
			__result = false;
		}
	}
}
