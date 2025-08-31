using System;
using StardewValley;

namespace RichLifeDayNightAndMore;

internal static class SolarCalculator
{
	// 获得当前时间并规范化
	internal static double GetTimeFormatted(bool HourFix)
	{
		int TimeHour = Game1.timeOfDay / 100;
		double TimeMinute = 
			Game1.timeOfDay % 100
			+ Game1.gameTimeInterval * 1.0 / Game1.realMilliSecondsPerGameTenMinutes * 10.0;
		double Time = TimeHour + TimeMinute / 60.0;
		return HourFix ? (Time % 24.0) : Time;
	}

	// 获得指定时间并规范化为游戏格式
	internal static int GetTimeFormatted(double Time)
	{
		int TimeHour = (int)Time;
		int TimeMinute = (int)(Time % 1.0 * 60.0);
		return TimeHour * 100 + TimeMinute;
	}

	// 计算当前时间的昼长
	internal static double GetDayLength()
	{
		double Phi = (double)(RichLifeDayNightAndMore.Config.Latitude / 180f) * Math.PI;
		double Delta = GetSolarDeclination() / 180.0 * Math.PI;
		if (GetSolarHeight(180.0) > 0.0)
		{
			return 24.0;
		}
		if (GetSolarHeight(0.0) < 0.0)
		{
			return 0.0;
		}
		double T = Math.Acos(
			- Math.Sin(Phi) * Math.Sin(Delta) 
			/ (Math.Cos(Phi) * Math.Cos(Delta))
		);
		return T / Math.PI * 24.0;
	}

	// 计算当前时间的太阳赤纬
	internal static double GetSolarDeclination()
	{
		double DayOffset = 
			Game1.Date.TotalDays 
			+ GetTimeFormatted(false) / 24.0 
			- 13.5; // 14日12时为二分二至点
		if (DayOffset < 0.0 && RichLifeDayNightAndMore.Config.ExtendEarlyDayDaylight)
		{
			return 0.0;
		}
		return RichLifeDayNightAndMore.Config.EclipticObliquity * Math.Sin(DayOffset / 112.0 * 2.0 * Math.PI);
	}

	// 计算当前时间的太阳高度角
	internal static double GetSolarHeight()
	{
		double Phi = RichLifeDayNightAndMore.Config.Latitude / 180.0 * Math.PI;
		double Delta = GetSolarDeclination() / 180.0 * Math.PI;
		double T = GetSolarHourAngle() / 180.0 * Math.PI;
		double SolarHeight = Math.Asin(
			Math.Sin(Phi) * Math.Sin(Delta) 
			+ Math.Cos(Phi) * Math.Cos(Delta) * Math.Cos(T)
		);
		return SolarHeight * 180.0 / Math.PI;
	}

	// 计算指定时角的太阳高度角
	internal static double GetSolarHeight(double HourAngle)
	{
		double Phi = RichLifeDayNightAndMore.Config.Latitude / 180.0 * Math.PI;
		double Delta = GetSolarDeclination() / 180.0 * Math.PI;
		double T = HourAngle / 180.0 * Math.PI;
		double SolarHeight = Math.Asin(
			Math.Sin(Phi) * Math.Sin(Delta) 
			+ Math.Cos(Phi) * Math.Cos(Delta) * Math.Cos(T)
		);
		return SolarHeight * 180.0 / Math.PI;
	}

	// 计算当前时间的时角
	internal static double GetSolarHourAngle()
	{
		return (GetTimeFormatted(true) - 12.0) * 15.0;
	}

	// 计算指定时间的时角
	internal static double GetSolarHourAngle(double Time)
	{
		return (Time % 24.0 - 12.0) * 15.0;
	}

	// 规范化时间
	internal static string GetTimeFormatted24(double Time)
	{
		int Hour = (int)Time;
		int Minute = (int)(Time % 1.0 * 60.0);
		return string.Format("{0:D2}:{1:D2}", Hour, Minute);
	}

	// 获取开始天黑、天黑了一半和完全天黑的游戏时
	internal static DarknessTime GetTimeOfDark()
	{
		float SolarHeight = (float)GetSolarHeight();
		double Time = GetTimeFormatted(false);
		int StartingToGetDarkTime;
		int ModeratelyDarkTime;
		int TrulyDarkTime;
		if (SolarHeight > RichLifeDayNightAndMore.Config.DayStartAtSolarHeight)
		{
			StartingToGetDarkTime = GetTimeFormatted(Time + 0.1);
			ModeratelyDarkTime = GetTimeFormatted(Time + 1.1);
			TrulyDarkTime = GetTimeFormatted(Time + 2.1);
		}
		else if (SolarHeight < RichLifeDayNightAndMore.Config.NightStartAtSolarHeight)
		{
			StartingToGetDarkTime = GetTimeFormatted(Time - 2.1);
			ModeratelyDarkTime = GetTimeFormatted(Time - 1.1);
			TrulyDarkTime = GetTimeFormatted(Time - 0.1);
		}
		else
		{
			float TwilightBarLength = 2f;
			float TotalLength = RichLifeDayNightAndMore.Config.NightStartAtSolarHeight - RichLifeDayNightAndMore.Config.DayStartAtSolarHeight;
			float Ratio = (SolarHeight - RichLifeDayNightAndMore.Config.DayStartAtSolarHeight) / TotalLength;
			StartingToGetDarkTime = GetTimeFormatted(Time - Ratio * TwilightBarLength);
			ModeratelyDarkTime = GetTimeFormatted(Time + (0.5f - Ratio) * TwilightBarLength);
			TrulyDarkTime = GetTimeFormatted(Time + (1f - Ratio) * TwilightBarLength);
		}
		return new DarknessTime(StartingToGetDarkTime, ModeratelyDarkTime, TrulyDarkTime);
	}
}
