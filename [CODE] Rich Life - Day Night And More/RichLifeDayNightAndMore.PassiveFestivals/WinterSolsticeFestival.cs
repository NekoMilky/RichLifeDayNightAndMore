using StardewValley;
using xTile.Dimensions;

namespace RichLifeDayNightAndMore.PassiveFestivals;

internal class WinterSolsticeFestival : PassiveFestival
{
	internal WinterSolsticeFestival()
		: base("WinterSolsticeFestival", RichLifeDayNightAndMore.ModCPPrefix + "FirstSnow")
	{
		AddHoldLocation("RailroadWinterSolsticeFestival");
	}

	internal override bool? AnswerDialogueAction(GameLocation Location, string questionAndAnswer)
	{
		if (questionAndAnswer == null)
		{
			return false;
		}
		if (questionAndAnswer == "WarperQuestion_Yes")
		{
			if (Game1.player.Money < 250)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
			}
			else
			{
				Game1.player.Money -= 250;
				Game1.player.CanMove = true;
				ItemRegistry.Create<Object>("(O)688").performUseAction(Location);
				Game1.player.freezePause = 5000;
			}
			return true;
		}
		return null;
	}

	internal override bool? CheckAction(GameLocation Location, Location Tile)
	{
		switch (Location.getTileIndexAt(Tile, "Buildings", "zRailroad_WinterSolsticeFestival_1"))
		{
		case 94:
			Utility.TryOpenShopMenu("Traveler", Location);
			break;
		case 105:
		case 106:
		case 107:
			Utility.TryOpenShopMenu(RichLifeDayNightAndMore.ModCPPrefix + "WinterSolsticeFestivalPierre", Location, RichLifeDayNightAndMore.HasSVE ? new(23, 45, 3, 3) : new(34, 45, 3, 3));
			break;
		case 161:
		case 162:
		case 163:
			Utility.TryOpenShopMenu(RichLifeDayNightAndMore.ModCPPrefix + "WinterSolsticeFestivalGus", Location, RichLifeDayNightAndMore.HasSVE ? new(26, 45, 3, 3) : new(39, 45, 3, 3));
			break;
		case 302:
			Location.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_WarperQuestion"), Location.createYesNoResponses(), "WarperQuestion");
			break;
		}
		return null;
	}
}
