namespace RichLifeDayNightAndMore;

internal static class ModFurniture
{
	// ÕÚ¹âÁ±´°»§
	internal static bool CheckWindowWithCurtain(string ItemId)
	{
		return ItemId.Contains("WindowWith") && ItemId.Contains("Curtain");
	}
}
