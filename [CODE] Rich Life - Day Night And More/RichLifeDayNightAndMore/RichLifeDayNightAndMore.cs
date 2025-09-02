using HarmonyLib;
using RichLifeDayNightAndMore.Integrations;
using RichLifeDayNightAndMore.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace RichLifeDayNightAndMore;

public class RichLifeDayNightAndMore : Mod
{
	internal static readonly string ModPrefix = "NaiveMilky.RichLifeDayNightAndMore_";
	internal static readonly string ModCPPrefix = "NaiveMilky.RichLifeDayNightAndMoreCP_";
	internal static ModConfig Config;
	internal static IModHelper ModHelper;
	internal static IMonitor ModMonitor;
	internal static bool HasSVE = false;

	// 补丁
	private void ApplyHarmonyPatch()
	{
		Harmony ModHarmony = new Harmony(ModManifest.UniqueID);
		FarmerClassPatch.ApplyPatch(ModHarmony);
        FarmHouseClassPatch.ApplyPatch(ModHarmony);
        ForestClassPatch.ApplyPatch(ModHarmony);
        FurnitureClassPatch.ApplyPatch(ModHarmony);
        Game1ClassPatch.ApplyPatch(ModHarmony);
		GameLocationClassPatch.ApplyPatch(ModHarmony);
        IslandFarmHouseClassPatch.ApplyPatch(ModHarmony);
        NPCClassPatch.ApplyPatch(ModHarmony);
		ObjectClassPatch.ApplyPatch(ModHarmony);
		UtilityClassPatch.ApplyPatch(ModHarmony);
	}

	// 向内容包提供令牌
	private void InitCP()
	{
		IContentPatcherAPI CPapi = ModHelper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
		if (CPapi == null)
		{
			return;
		}
		CPapi.RegisterToken(
			mod: ModManifest,
			name: "ChangeClockTexture",
			getValue: () => new string[1] { Config.ChangeClockTexture.ToString() }
		);
		for (int i = 6; i <= 25; i++)
		{
			string[] Letters = {
				"", "", "", "", "",
				"", "Six", "Seven", "Eight", "Nine",
				"Ten", "Eleven", "Twelve", "Thirteen", "Fourteen",
				"Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen",
				"Twenty", "TwentyOne", "TwentyTwo", "TwentyThree", "TwentyFour",
				"TwentyFive"
			};
			int CurrentI = i;
			CPapi.RegisterToken(
				mod: ModManifest,
				name: Letters[i] + "DayOrNight",
				getValue: () => {
					double solarHourAngle = SolarCalculator.GetSolarHourAngle(CurrentI + 0.5);
					double solarHeight = SolarCalculator.GetSolarHeight(solarHourAngle);
					if (solarHeight > (double)Config.DayStartAtSolarHeight)
					{
						return new string[1] { CurrentI + "30_Day" };
					}
					else if (solarHeight > 0.0)
					{
						return new string[1] { CurrentI + "30_AboveHorizon" };
					}
					else if (solarHeight > (double)Config.NightStartAtSolarHeight)
					{
						return new string[1] { CurrentI + "30_BelowHorizon" };
					}
					else
					{
						return new string[1] { CurrentI + "30_Night" };

					}
				}
			);
		}
		CPapi.RegisterToken(
			mod: ModManifest,
			name: "ShowClockSun",
			getValue: () => new string[1] { (SolarCalculator.GetSolarHeight(45.0) > (double)Config.DayStartAtSolarHeight).ToString() }
		);
		CPapi.RegisterToken(
			mod: ModManifest,
			name: "ShowClockMoon",
			getValue: () => new string[1] { (SolarCalculator.GetSolarHeight(135.0) < (double)Config.NightStartAtSolarHeight).ToString() }
		);
	}

	// 配合GMCM进行模组配置
	private void InitGMCM()
	{
		IGenericModConfigMenuApi GMCMapi = ModHelper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
		if (GMCMapi == null)
		{
			return;
		}
        GMCMapi.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => ModHelper.WriteConfig(Config)
            );
        GMCMapi.AddBoolOption(
            mod: ModManifest,
            getValue: () => Config.ChangeClockTexture,
            setValue: (value) => Config.ChangeClockTexture = value,
            name: () => GetTranslation("config.change-clock-texture.name"),
            tooltip: () => GetTranslation("config.change-clock-texture.tooltip")
        );
        GMCMapi.AddBoolOption(
            mod: ModManifest,
            getValue: () => Config.ExtendEarlyDayDaylight,
            setValue: (value) => Config.ExtendEarlyDayDaylight = value,
            name: () => GetTranslation("config.extend-early-day-daylight.name"),
            tooltip: () => GetTranslation("config.extend-early-day-daylight.tooltip")
        );
        GMCMapi.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.Latitude,
            setValue: (value) => Config.Latitude = value,
            name: () => GetTranslation("config.latitude.name"),
            tooltip: () => GetTranslation("config.latitude.tooltip"),
            min: 0f,
            max: 90f,
            interval: 0.001f
        );
        GMCMapi.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.EclipticObliquity,
            setValue: (value) => Config.EclipticObliquity = value,
            name: () => GetTranslation("config.ecliptic-obliquity.name"),
            tooltip: () => GetTranslation("config.ecliptic-obliquity.tooltip"),
            min: 0f,
            max: 90f,
            interval: 0.001f
        );
        GMCMapi.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.DayStartAtSolarHeight,
            setValue: (value) => Config.DayStartAtSolarHeight = value,
            name: () => GetTranslation("config.day-start-at-solar-height.name"),
            tooltip: () => GetTranslation("config.day-start-at-solar-height.tooltip"),
            min: 3f,
            max: 9f,
            interval: 0.1f
        );
        GMCMapi.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.NightStartAtSolarHeight,
            setValue: (value) => Config.NightStartAtSolarHeight = value,
            name: () => GetTranslation("config.night-start-at-solar-height.name"),
            tooltip: () => GetTranslation("config.night-start-at-solar-height.tooltip"),
            min: -9f,
            max: -3f,
            interval: 0.1f
        );
        GMCMapi.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.RainingDarkness,
            setValue: (value) => Config.RainingDarkness = value,
            name: () => GetTranslation("config.raining-darkness.name"),
            tooltip: () => GetTranslation("config.raining-darkness.tooltip"),
            min: 0f,
            max: 0.3f,
            interval: 0.1f
        );
        GMCMapi.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.NightDarkness,
            setValue: (value) => Config.NightDarkness = value,
            name: () => GetTranslation("config.night-darkness.name"),
            tooltip: () => GetTranslation("config.night-darkness.tooltip"),
            min: 0.3f,
            max: 1f,
            interval: 0.1f
        );
    }

	private void OnDayStarted(object? sender, DayStartedEventArgs e)
	{
		ModFestival.OnDayStarted(sender, e);
	}

	private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
	{
		InitCP();
		InitGMCM();
		ModFestival.InitFestival();
	}

	private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
	{
		ModFestival.OnTimeChanged(sender, e);
	}

	internal static string GetTranslation(string Key)
	{
		return ModHelper.Translation.Get(Key);
	}

	public override void Entry(IModHelper Helper)
	{
		ModHelper = Helper;
		ModMonitor = Monitor;
		// 读取配置
		Config = ModHelper.ReadConfig<ModConfig>();
		// 应用补丁
		ApplyHarmonyPatch();
		// 事件
		ModHelper.Events.GameLoop.DayStarted += OnDayStarted;
		ModHelper.Events.GameLoop.GameLaunched += OnGameLaunched;
		ModHelper.Events.GameLoop.TimeChanged += OnTimeChanged;
		// 检测其他需要被兼容的模组
		HasSVE = ModHelper.ModRegistry.IsLoaded("FlashShifter.SVECode");
	}
}
