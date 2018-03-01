using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;
using Verse.AI;
using System.Collections;

namespace CatsAreJerks
{
    public class JobGiver_DisturbThePeace : JobGiver_Wander
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (Rand.RangeInclusive(0, 1000) > 15)
            {
                return null;
            }
            IntVec3 exactWanderDest = this.GetExactWanderDest(pawn);
            if (!exactWanderDest.IsValid)
            {
                pawn.mindState.nextMoveOrderIsWait = false;
                return null;
            }
            Job job = new Job(JobDefOf.GotoWander, exactWanderDest);
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, exactWanderDest);
            job.locomotionUrgency = LocomotionUrgency.Sprint;
            this.wanderRadius = 5f;
            this.ticksBetweenWandersRange = new IntRange(10, 50);
            this.wanderDestValidator = ((Pawn actor, IntVec3 loc) => WanderRoomUtility.IsValidWanderDest(actor, loc, this.GetWanderRoot(actor)));
            return job;
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            Pawn pawnToHarass = ColonyAnimalLister.FindBondedMaster(pawn);

            if (pawnToHarass == null || pawnToHarass.CurJob.def != JobDefOf.LayDown || pawnToHarass.CurrentBed()?.GetCurOccupant(1) != pawnToHarass)
            {
                pawn.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(Faction.OfPlayer).TryRandomElement(out Pawn randomColonist);
                pawnToHarass = randomColonist;
            }

            if (pawnToHarass?.ownership?.OwnedRoom != null)
            {
                Room masterBedroom = pawnToHarass.ownership.OwnedRoom;

                IEnumerable<IntVec3> intVec3 = from c in masterBedroom.Cells
                                               where c.Standable(pawn.Map) && !c.IsForbidden(pawn) && pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None, 1, -1, null, false)
                                               select c;

                intVec3.TryRandomElement(out IntVec3 vec3);
                return vec3;
            }
            else return IntVec3.Invalid;
        }
    }
}
