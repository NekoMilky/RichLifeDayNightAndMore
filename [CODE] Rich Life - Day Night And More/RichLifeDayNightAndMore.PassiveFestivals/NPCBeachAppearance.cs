using Microsoft.Xna.Framework;

namespace RichLifeDayNightAndMore.PassiveFestivals;

internal class NPCBeachAppearance
{
	internal string NPCName;
	internal string ChangingRoomLocation;
	internal Point ChangingRoomTile;
	internal bool BeachAppearanceToggled;
	internal bool EnteredChangingRoom;

	internal NPCBeachAppearance(string Name, string Location, Point Tile)
	{
		NPCName = Name;
		ChangingRoomLocation = Location;
		ChangingRoomTile = Tile;
		ResetStatus();
	}

	internal void ResetStatus()
	{
		BeachAppearanceToggled = false;
		EnteredChangingRoom = false;
	}
}
