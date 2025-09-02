using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using RichLifeDayNightAndMore.PassiveFestivals;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace RichLifeDayNightAndMore.Patches;

internal static class NPCClassPatch
{
	internal static void ApplyPatch(Harmony harmony)
	{
		harmony.Patch(
			original: AccessTools.Method(typeof(NPC), nameof(NPC.ChooseAppearance)), 
			postfix: new(typeof(NPCClassPatch), nameof(PostfixChooseAppearance))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(NPC), nameof(NPC.resetForNewDay)), 
			postfix: new(typeof(NPCClassPatch), nameof(PostfixResetForNewDay))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)), 
			transpiler: new(typeof(NPCClassPatch), nameof(TranspilerCheckAction))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)), 
			transpiler: new(typeof(NPCClassPatch), nameof(TranspilerMarriageDuties))
		);
	}

	private static bool GetSleptWell(NPC ThisNPC)
	{
		bool SleptWell = true;
		if (ThisNPC.modData.TryGetValue(RichLifeDayNightAndMore.ModPrefix + "SleptWell", out var Value))
		{
			bool.TryParse(Value, out SleptWell);
		}
		return SleptWell;
	}

	// NPC服装覆写
	private static void PostfixChooseAppearance(ref NPC __instance, LocalizedContentManager content)
	{
		PassiveFestival Festival = ModFestival.GetModPassiveFestivalToday();
		if (Festival == null)
		{
			return;
		}
		string NPCName = __instance.Name;
		if (Festival.IsNPCBeachAppearanceToggled(NPCName))
		{
			string Portraits = "Portraits\\" + NPCName + "_Beach";
			if (!__instance.TryLoadPortraits(Portraits, out var PortraitsError, content))
			{
				RichLifeDayNightAndMore.ModMonitor.Log($"NPC {NPCName} can't load portraits from '{Portraits}': {PortraitsError}. Falling back to default portraits.", (LogLevel)3);
			}
			string Sprites = "Characters\\" + NPCName + "_Beach";
			if (!__instance.TryLoadSprites(Sprites, out var SpritesError, content))
			{
				RichLifeDayNightAndMore.ModMonitor.Log($"NPC {NPCName} can't load sprites from '{Sprites}': {SpritesError}. Falling back to default sprites.", (LogLevel)3);
			}
		}
	}

	// 新一天判断配偶睡眠质量
	private static void PostfixResetForNewDay(ref NPC __instance)
	{
		if (!__instance.isMarried() || __instance.isRoommate() || !__instance.sleptInBed.Value)
		{
			return;
		}
		Farmer Farmer = __instance.getSpouse();
		FarmHouse House = Utility.getHomeOfFarmer(Farmer);
		Point SpouseBedSpot = House.GetSpouseBed().GetBedSpot();
        __instance.modData[RichLifeDayNightAndMore.ModPrefix + "SleptWell"] = FarmerClassPatch.IsSleptWell(House, SpouseBedSpot, out int FriendshipLoss, out float _).ToString();
        Farmer.changeFriendship(FriendshipLoss, __instance);
	}

	// 配偶因睡不好拒绝接吻
	private static IEnumerable<CodeInstruction> TranspilerCheckAction(IEnumerable<CodeInstruction> Instructions)
	{
		var Codes = new List<CodeInstruction>(Instructions);
		for (int i = 3; i < Codes.Count; i++)
		{
			if (
				Codes[i - 3].opcode == OpCodes.Ldfld 
				&& Codes[i - 3].operand as FieldInfo == AccessTools.Field(typeof(NPC), nameof(NPC.sleptInBed))
			)
			{
				var Target = Codes[i - 1].operand;
				Codes.InsertRange(i, new List<CodeInstruction>() {
					// && GetSleptWell(this)
					new(OpCodes.Ldarg_0),
					new(OpCodes.Call, AccessTools.Method(typeof(NPCClassPatch), nameof(GetSleptWell))),
					new(OpCodes.Brfalse_S, Target)
				});
				break;
			}
		}
		return Codes;
	}

	// 配偶因睡不好重置对话
	private static IEnumerable<CodeInstruction> TranspilerMarriageDuties(IEnumerable<CodeInstruction> Instructions, ILGenerator IL)
	{
        var Codes = new List<CodeInstruction>(Instructions);
        for (int i = 0; i < Codes.Count - 1; i++)
		{
			if (
				Codes[i + 1].opcode == OpCodes.Ldfld 
				&& Codes[i + 1].operand as FieldInfo == AccessTools.Field(typeof(NPC), nameof(NPC.sleptInBed))
			)
			{
                var Target = IL.DefineLabel();
                var LocalInt = IL.DeclareLocal(typeof(int));
                Codes[i].labels.Add(Target);
                Codes.InsertRange(i, new List<CodeInstruction> {
					// if (!GetSleptWell(this))
                    new(OpCodes.Ldarg_0),
					new(OpCodes.Call, AccessTools.Method(typeof(NPCClassPatch), nameof(GetSleptWell))),
					new(OpCodes.Brtrue_S, Target),
					// currentMarriageDialogue.Clear();
                    new(OpCodes.Ldarg_0),
					new(OpCodes.Ldfld, AccessTools.Field(typeof(NPC), nameof(NPC.currentMarriageDialogue))),
					new(OpCodes.Callvirt, AccessTools.Method(typeof(NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>>), "Clear")),
                    // addMarriageDialogue("MarriageDialogue", "SleptBadly_" + r.Next(3), false);
					new(OpCodes.Ldarg_0),
					new(OpCodes.Ldstr, "MarriageDialogue"),
					new(OpCodes.Ldstr, "SleptBadly_"),
					new(OpCodes.Ldloc_2),
					new(OpCodes.Ldc_I4_3),
					new(OpCodes.Callvirt, AccessTools.Method(typeof(Random), "Next", new[] { typeof(int) })),
					new(OpCodes.Stloc, LocalInt.LocalIndex),
					new(OpCodes.Ldloca, LocalInt.LocalIndex),
					new(OpCodes.Call, AccessTools.Method(typeof(int), "ToString", Type.EmptyTypes)),
					new(OpCodes.Call, AccessTools.Method(typeof(string), "Concat", new[] { typeof(string), typeof(string) })),
					new(OpCodes.Ldc_I4_0),
					new(OpCodes.Call, AccessTools.Method(typeof(Array), "Empty", (Type[])null, (Type[])null).MakeGenericMethod(typeof(string))),
					new(OpCodes.Call, AccessTools.Method(typeof(NPC), nameof(NPC.addMarriageDialogue))),
                    // return;
					new(OpCodes.Ret)
				});
                break;
			}
		}
		return Codes;
	}
}
