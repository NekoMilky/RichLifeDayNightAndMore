namespace RichLifeDayNightAndMore;

internal static class ModFurniture
{
	// �ڹ�������
	internal static bool CheckWindowWithCurtain(string ItemId)
	{
		return ItemId.Contains("WindowWith") && ItemId.Contains("Curtain");
	}
}
