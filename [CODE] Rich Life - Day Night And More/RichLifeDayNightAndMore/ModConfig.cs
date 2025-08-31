namespace RichLifeDayNightAndMore;

public class ModConfig
{
	public bool ExtendEarlyDayDaylight { get; set; } = true;
	public bool ChangeClockTexture { get; set; } = true;
	public float DayStartAtSolarHeight { get; set; } = 6f;
	public float EclipticObliquity { get; set; } = 23.436f;
	public float Latitude { get; set; } = 52f;
	public float NightDarkness { get; set; } = 0.93f;
	public float NightStartAtSolarHeight { get; set; } = -6f;
	public float RainingDarkness { get; set; } = 0.3f;
}
