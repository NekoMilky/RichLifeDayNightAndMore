using System;
using HarmonyLib;
using StardewValley;

namespace RichLifeDayNightAndMore.Patches;

internal static class UtilityClassPatch
{
	internal static void ApplyPatch(Harmony harmony)
	{
		harmony.Patch(
			original: AccessTools.Method(typeof(Utility), nameof(Utility.CalculateMinutesBetweenTimes)), 
			postfix: new(typeof(UtilityClassPatch), nameof(PostfixCalculateMinutesBetweenTimes))
		);
	}

	// 计算差值返回差的绝对值
	private static void PostfixCalculateMinutesBetweenTimes(ref int __result)
	{
		__result = Math.Abs(__result);
	}
}
