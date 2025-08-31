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
		// ������
		PassiveFestivals.Add(new SummerSolsticeFestival());
		// ������
		PassiveFestivals.Add(new WinterSolsticeFestival());
	}

	// �����յ����ǰһ���״̬
	internal static void OnDayStarted(object? sender, DayStartedEventArgs e)
	{
		GetModPassiveFestivalToday()?.PerformNewDay();
		GetModPassiveFestivalTomorrow()?.SendReminder();
	}

	// ���½���״̬
	internal static void OnTimeChanged(object? sender, TimeChangedEventArgs e)
	{
		GetModPassiveFestivalToday()?.PerformTenMinuteUpdate();
	}
}
