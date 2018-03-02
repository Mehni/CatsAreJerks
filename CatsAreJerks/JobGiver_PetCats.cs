using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsAreJerks
{
    //we inherit from ThinkNode_JobGiver to save us some trouble
    public class JoyGiver_PetCats : JoyGiver
    {
        //the method that's defined in the inherited method. We override it here and return our job.
        public override Job TryGiveJob(Pawn pawn)
        {
            //find a cat
            Log.Message(pawn + " is seeking a cat!");
            Pawn cat = FindCatToPet(pawn);

            //if there is no cat, or we can't reach the cat, we don't do a job.
            if (cat == null || !pawn.CanReserveAndReach(cat, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
            {
                Log.Message(pawn + " did not find a cat. Sad day.");
                return null;
            }

            //If we find a cat, return a new Job called PetTheCat. Since we're using our own job, we're using the below syntax, including the class we gave it
            //If we were using a vanilla Job, the below command would be
            //return new Job(JobDefOf.Slaughter, cat);
            //for a slaughter job :(
            return new Job(CatsAreJerks_JobDefOf.PetTheCat, cat);
        }

        private static List<Pawn> tmpAnimals = new List<Pawn>();

        public static Pawn FindCatToPet(Pawn pawn)
        {
            if (!pawn.Spawned)
            {
                return null;
            }

            //find bonded animal. If no bonded animal, pick a pet-like creature.
            Pawn result = ColonyAnimalLister.FindBondedAnimal(pawn);
            if (result != null) return result;
            else
            {
                tmpAnimals.Clear();
                List<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < allPawnsSpawned.Count; i++)
                {
                    Pawn pawn2 = allPawnsSpawned[i];
                    if (pawn2.RaceProps.Animal && pawn2.Faction == pawn.Faction
                        && pawn2.RaceProps.petness >= 0.1
                        && !(pawn2.CurJob.def == JobDefOf.LayDown)
                        && pawn2 != pawn && !pawn2.IsBurning()
                        && !pawn2.InAggroMentalState
                        && pawn.CanReserveAndReach(pawn2, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                    {
                        tmpAnimals.Add(pawn2);
                    }
                }
                if (!tmpAnimals.Any<Pawn>())
                {
                    return null;
                }
                result = tmpAnimals.RandomElement<Pawn>();
                tmpAnimals.Clear();

                return result;
            }
        }
    }
}
