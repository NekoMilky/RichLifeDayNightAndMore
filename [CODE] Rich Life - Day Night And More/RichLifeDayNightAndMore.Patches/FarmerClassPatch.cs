using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RichLifeDayNightAndMore.Patches;

internal static class FarmerClassPatch
{
    internal static void ApplyPatch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.dayupdate)),
            transpiler: new(typeof(FarmerClassPatch), nameof(TranspilerDayUpdate))
        );
    }

    internal static bool IsSleptWell(GameLocation BedLocation, Point BedSpot, out int FriendshipLoss, out float StaminaLossRatio, int MaxFriendshipLoss = -15, float MaxStaminaLossRatio = 0.5f, double StartingToSleepBadlyDayLength = 16.0, double TrulySleepBadlyDayLength = 18.0)
    {
        double DayLength = SolarCalculator.GetDayLength();
        FriendshipLoss = 0;
        StaminaLossRatio = 0f;
        if (DayLength < StartingToSleepBadlyDayLength || BedLocation.IsRainingHere())
        {
            return true;
        }
        for (int TileX = BedSpot.X - 5; TileX <= BedSpot.X + 5; TileX++)
        {
            for (int TileY = BedSpot.Y - 5; TileY <= BedSpot.Y; TileY++)
            {
                Furniture ThisFurniture = BedLocation.GetFurnitureAt(new(TileX, TileY));
                // 如果当前位置没有家具、家具不为窗户、窗户为遮光帘窗户，就跳过
                if (ThisFurniture == null || ThisFurniture.furniture_type.Value != 13 || ModFurniture.CheckWindowWithCurtain(ThisFurniture.ItemId))
                {
                    continue;
                }
                // 计算失去的友谊值和能量值比率
                double SleepBadlyChance = DayLength >= TrulySleepBadlyDayLength ? 1.0 : (DayLength - StartingToSleepBadlyDayLength) / (TrulySleepBadlyDayLength - StartingToSleepBadlyDayLength);
                if (Game1.random.NextDouble() < SleepBadlyChance)
                {
                    FriendshipLoss = (int)(MaxFriendshipLoss * SleepBadlyChance);
                    StaminaLossRatio = (float)(MaxStaminaLossRatio * SleepBadlyChance);
                    return false;
                }
                return true;
            }
        }
        return true;
    }

    private static float SleptBadlyStamina(Farmer Player, float Stamina)
    {
        if (!Player.isInBed.Value)
        {
            return 0f;
        }
        GameLocation BedLocation = Player.currentLocation;
        Point BedSpot = Player.TilePoint;
        IsSleptWell(BedLocation, BedSpot, out int _, out float StaminaLossRatio);
        return Stamina * (1f - StaminaLossRatio);
    }

    // 体力值计算考虑睡不好
    private static IEnumerable<CodeInstruction> TranspilerDayUpdate(IEnumerable<CodeInstruction> Instructions, ILGenerator IL)
    {
        var Codes = new List<CodeInstruction>(Instructions);
        for (int i = 33; i < Codes.Count - 2; i++)
        {
            if (
                Codes[i - 33].opcode == OpCodes.Ble_S
                && Codes[i].opcode == OpCodes.Ldarg_1
                && Codes[i + 1].opcode == OpCodes.Ldc_I4
                && Codes[i + 1].operand as int? == 2700
                && Codes[i + 2].opcode == OpCodes.Bge_S
            )
            {
                var Target = IL.DefineLabel();
                Codes[i - 33].operand = Target;
                Codes.InsertRange(i, new List<CodeInstruction>
                {
                    // Stamina = SleptBadlyStamina(this, Stamina);
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Call, AccessTools.Method(typeof(Farmer), "get_Stamina")),
                    new(OpCodes.Call, AccessTools.Method(typeof(FarmerClassPatch), nameof(SleptBadlyStamina))),
                    new(OpCodes.Call, AccessTools.Method(typeof(Farmer), "set_Stamina"))
                });
                Codes[i].labels.Add(Target);
                break;
            }
        }
        return Codes;
    }
}