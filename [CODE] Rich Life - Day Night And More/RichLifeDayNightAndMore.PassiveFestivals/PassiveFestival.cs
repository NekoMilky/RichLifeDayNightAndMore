using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Dimensions;

namespace RichLifeDayNightAndMore.PassiveFestivals;

internal class PassiveFestival
{
	internal string Name;
	internal List<string> HoldLocations = new();
	internal List<NPCBeachAppearance> NPCBeachAppearances = new();
	internal string MusicPlayingWhenOpen;

	internal PassiveFestival(string N, string M)
	{
		if (!N.StartsWith(RichLifeDayNightAndMore.ModCPPrefix))
		{
			N = RichLifeDayNightAndMore.ModCPPrefix + N;
		}
		Name = N;
		MusicPlayingWhenOpen = M;
	}

	// Ϊ�������һ���ٰ�ص�
	protected void AddHoldLocation(string LocationName)
	{
		if (!LocationName.StartsWith(RichLifeDayNightAndMore.ModCPPrefix))
		{
			LocationName = RichLifeDayNightAndMore.ModCPPrefix + LocationName;
		}
		HoldLocations.Add(LocationName);
	}

	// Ϊָ��NPC�趨���¼�λ��
	protected void AddNPCBeachAppearance(string NPCName, string LocationName, Point Tile)
	{
		if (Tile.X >= 0 && Tile.Y >= 0)
		{
			if (!LocationName.StartsWith(RichLifeDayNightAndMore.ModCPPrefix))
			{
				LocationName = RichLifeDayNightAndMore.ModCPPrefix + LocationName;
			}
			NPCBeachAppearances.Add(new NPCBeachAppearance(NPCName, LocationName, Tile));
		}
	}

	internal virtual bool? AnswerDialogueAction(GameLocation Location, string questionAndAnswer)
	{
		return null;
	}

	internal virtual bool? CheckAction(GameLocation Location, Location Tile)
	{
		return null;
	}

	internal bool IsNPCBeachAppearanceToggled(string NPCName)
	{
		foreach (NPCBeachAppearance NPCBeachAppearance in NPCBeachAppearances)
		{
			if (NPCBeachAppearance.NPCName == NPCName)
			{
				return NPCBeachAppearance.BeachAppearanceToggled;
			}
		}
		return false;
	}

	internal bool IsThisFestivalOpen()
	{
		return Utility.IsPassiveFestivalOpen(Name);
	}

	internal bool IsThisFestivalToday()
	{
		return Utility.IsPassiveFestivalDay(Name);
	}

	internal bool IsThisFestivalTomorrow()
	{
		if (Utility.TryGetPassiveFestivalDataForDay(Game1.dayOfMonth + 1, Game1.season, null, out var Id, out var _))
		{
			return Id == Name;
		}
		return false;
	}

	// ���÷�װ��д״̬
	internal void PerformNewDay()
	{
		foreach (NPCBeachAppearance NPCAppearance in NPCBeachAppearances)
		{
			NPCAppearance.ResetStatus();
		}
	}

	// ���������ʼ�
	internal void SendReminder()
	{
		Game1.player.mailbox.Add(Name + "Reminder");
	}

	internal void PerformTenMinuteUpdate()
	{
		// ��������
		if (HoldLocations.Contains(Game1.currentLocation.Name) && IsThisFestivalOpen())
		{
			Game1.changeMusicTrack(MusicPlayingWhenOpen);
		}
		// ��װ��д
		foreach (NPCBeachAppearance NPCBeachAppearance in NPCBeachAppearances)
		{
			NPC UpdateNPC = Game1.getCharacterFromName(NPCBeachAppearance.NPCName);
			string Location = UpdateNPC.currentLocation.Name;
			string ChangingRoomLocation = NPCBeachAppearance.ChangingRoomLocation;
			Point Tile = UpdateNPC.TilePoint;
			Point ChangingRoomTile = NPCBeachAppearance.ChangingRoomTile;
			if (NPCBeachAppearance.EnteredChangingRoom)
			{
				if (Location != ChangingRoomLocation || Tile != ChangingRoomTile)
				{
					NPCBeachAppearance.EnteredChangingRoom = false;
				}
			}
			else if (Location == ChangingRoomLocation && Tile == ChangingRoomTile)
			{
				NPCBeachAppearance.EnteredChangingRoom = true;
				NPCBeachAppearance.BeachAppearanceToggled = !NPCBeachAppearance.BeachAppearanceToggled;
				UpdateNPC.ChooseAppearance();
			}
		}
	}
}
