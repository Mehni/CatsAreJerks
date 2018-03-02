using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsAreJerks
{
    public static class ColonyAnimalLister
    {
        private static List<Pawn> bondedAnimals = new List<Pawn>();
        private static List<Pawn> bondedMaster = new List<Pawn>();

        public static Pawn FindBondedAnimal(Pawn pawn)
        {
            bondedAnimals.Clear();
            List<Pawn> allPlayerPawnsSpawned = pawn.Map.mapPawns.PawnsInFaction(Faction.OfPlayer).ToList();
            for (int i = 0; i < allPlayerPawnsSpawned.Count; i++)
            {
                Pawn pawn2 = allPlayerPawnsSpawned[i];
                if (pawn2.RaceProps.Animal && pawn2.Faction == pawn.Faction
                    && pawn2 != pawn && !pawn2.IsBurning()
                    && !pawn2.InAggroMentalState
                    && !(pawn2.CurJob.def == JobDefOf.LayDown)
                    && (pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, pawn2) || (pawn2.playerSettings.RespectedMaster == pawn))
                    && pawn.CanReserveAndReach(pawn2, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                {
                    bondedAnimals.Add(pawn2);
                }
            }
            if (!bondedAnimals.Any<Pawn>())
            {
                return null;
            }
            Pawn result = bondedAnimals.RandomElement<Pawn>();
            bondedAnimals.Clear();
            return result;
        }

        public static Pawn FindBondedMaster(Pawn pawn)
        {
            bondedMaster.Clear();
            List<Pawn> allPlayerPawnsSpawned = pawn.Map.mapPawns.PawnsInFaction(Faction.OfPlayer).ToList();
            for (int i = 0; i < allPlayerPawnsSpawned.Count; i++)
            {
                Pawn pawn2 = allPlayerPawnsSpawned[i];
                if (pawn2.RaceProps.Humanlike && pawn2.Faction == pawn.Faction
                    && pawn2 != pawn && !pawn2.IsBurning()
                    && !pawn2.InAggroMentalState
                    && (pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, pawn2) || (pawn2.playerSettings.RespectedMaster == pawn))
                    && pawn.CanReserveAndReach(pawn2, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                {
                    bondedMaster.Add(pawn2);
                }
            }
            if (!bondedMaster.Any<Pawn>())
            {
                return null;
            }
            Pawn result = bondedMaster.RandomElement<Pawn>();
            bondedMaster.Clear();
            return result;
        }        
    }
}
