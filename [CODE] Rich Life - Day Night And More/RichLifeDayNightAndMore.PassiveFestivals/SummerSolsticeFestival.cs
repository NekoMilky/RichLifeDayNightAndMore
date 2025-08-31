using System.Collections.Generic;
using StardewValley;
using xTile.Dimensions;

namespace RichLifeDayNightAndMore.PassiveFestivals;

internal class SummerSolsticeFestival : PassiveFestival
{
	internal SummerSolsticeFestival()
		: base("SummerSolsticeFestival", "night_market")
	{
		string HoldLocation = "BeachSummerSolsticeFestival";
		AddHoldLocation(HoldLocation);
		List<string> NPCFemale = new() { "Abigail", "Emily", "Haley", "Leah", "Penny" };
		foreach (string NPC in NPCFemale)
		{
			AddNPCBeachAppearance(NPC, HoldLocation, new(26, 2));
		}
		List<string> NPCMale = new() { "Alex", "Sam", "Sebastian" };
		foreach (string NPC2 in NPCMale)
		{
			AddNPCBeachAppearance(NPC2, HoldLocation, new(28, 2));
		}
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
		switch (Location.getTileIndexAt(Tile, "Buildings", "zBeach_SummerSolsticeFestival_1"))
		{
		case 179:
			Location.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_WarperQuestion"), Location.createYesNoResponses(), "WarperQuestion");
			break;
		case 261:
		case 262:
		case 263:
			Utility.TryOpenShopMenu(RichLifeDayNightAndMore.ModCPPrefix + "SummerSolsticeFestivalGus", Location, new(31, 14, 3, 3));
			break;
		case 379:
			Utility.TryOpenShopMenu(RichLifeDayNightAndMore.ModCPPrefix + "SummerSolsticeFestivalDecoration", Location);
			break;
		case 389:
			Utility.TryOpenShopMenu("Traveler", Location);
			break;
		case 503:
		case 504:
		case 505:
			Utility.TryOpenShopMenu(RichLifeDayNightAndMore.ModCPPrefix + "SummerSolsticeFestivalPierre", Location, new(14, 4, 3, 3));
			break;
		case 572:
		case 573:
			Utility.TryOpenShopMenu(RichLifeDayNightAndMore.ModCPPrefix + "SummerSolsticeFestivalMagic", Location);
			break;
		case 707:
			Utility.TryOpenShopMenu(RichLifeDayNightAndMore.ModCPPrefix + "SummerSolsticeFestivalMermaid", Location);
			break;
		}
		return null;
	}
}
