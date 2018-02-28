//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RimWorld;
//using Verse;
//using Harmony;
//using UnityEngine;
//using Verse.AI;
////using Kitchen.Sink;

//namespace CatsAreJerks
//{
//    [StaticConstructorOnStartup]
//    static class HarmonyPatches
//    {
//        static HarmonyPatches()
//        {
//            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.catsarejerks.main");

//            harmony.Patch(AccessTools.Method(typeof(Pawn_JobTracker), "CheckForJobOverride"), null,
//                new HarmonyMethod(typeof(HarmonyPatches), nameof(Joboverride_Postfix)), null);
//        }

//        static void Joboverride_Postfix(Pawn_JobTracker __instance)
//        {
//            if (__instance != null) { }


//        }
//    }
//}
