using System.Collections.Generic;
using RichLifeDayNightAndMore.PassiveFestivals;
using StardewModdingAPI.Events;
using StardewValley;

namespace RichLifeDayNightAndMore;

internal static class ModFestival
{
	private static readonly List<PassiveFestival> PassiveFestivals = new();

	internal static PassiveFestival? GetModPassiveFestivalToday()
	{
		if (!Utility.IsPassiveFestivalDay())
		{
			return null;
		}
		foreach (PassiveFestival PassiveFestival in PassiveFestivals)
		{
			if (PassiveFestival.IsThisFestivalToday())
			{
				return PassiveFestival;
			}
		}
		return null;
	}

	internal static PassiveFestival? GetModPassiveFestivalTomorrow()
	{
		foreach (PassiveFestival PassiveFestival in PassiveFestivals)
		{
			if (PassiveFestival.IsThisFestivalTomorrow())
			{
				return PassiveFestival;
			}
		}
		return null;
	}

	internal static void InitFestival()
	{
		// 夏至节
		PassiveFestivals.Add(new SummerSolsticeFestival());
		// 冬至节
		PassiveFestivals.Add(new WinterSolsticeFestival());
	}

	// 检查节日当天和前一天的状态
	internal static void OnDayStarted(object? sender, DayStartedEventArgs e)
	{
		GetModPassiveFestivalToday()?.PerformNewDay();
		GetModPassiveFestivalTomorrow()?.SendReminder();
	}

	// 更新节日状态
	internal static void OnTimeChanged(object? sender, TimeChangedEventArgs e)
	{
		GetModPassiveFestivalToday()?.PerformTenMinuteUpdate();
	}
}
