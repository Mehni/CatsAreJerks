using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;
using UnityEngine;
using Verse.AI;
//using Kitchen.Sink;

namespace CatsAreJerks
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.catsarejerks.main");

            //harmony.Patch(AccessTools.Method(typeof(JobDriver_PredatorHunt), "MakeNewToils"), null,
            //    new HarmonyMethod(typeof(HarmonyPatches), nameof(PredatorHunt_Postfix)), null);

            //TODO: End MakeNewToils so animals haul their kill back

            harmony.Patch(AccessTools.Method(typeof(FoodUtility), "IsAcceptablePreyFor"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(IsAcceptablePreyForBugFix_Prefix)), null, null);

        }

        private static bool IsAcceptablePreyForBugFix_Prefix(ref Pawn predator, ref Pawn prey, ref bool __result)
        {
            if (!prey.RaceProps.canBePredatorPrey || !prey.RaceProps.IsFlesh || prey.BodySize > predator.RaceProps.maxPreyBodySize)
            {
                __result = false;
            }

            if (!prey.Downed)
            {
                if (prey.kindDef.combatPower > 2f * predator.kindDef.combatPower)
                {
                    __result = false;
                }
                //vanilla bug
                float num = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * (prey.ageTracker.CurLifeStage.bodySizeFactor * prey.RaceProps.baseBodySize);
                float num2 = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * (predator.ageTracker.CurLifeStage.bodySizeFactor * predator.RaceProps.baseBodySize);
                if (num > 0.85f * num2)
                {
                    __result = false;
                }
            }
            __result = (predator.Faction == null || prey.Faction == null || predator.HostileTo(prey)) && (predator.Faction != Faction.OfPlayer || prey.Faction != Faction.OfPlayer) && (!predator.RaceProps.herdAnimal || predator.def != prey.def);
            return false;
        }
    }
}