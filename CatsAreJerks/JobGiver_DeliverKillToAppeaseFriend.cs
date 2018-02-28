using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsAreJerks
{
    public class JobGiver_DeliverKillToAppeaseFriend : ThinkNode_JobGiver
    {
        public static Corpse GetClosestCorpseToDigUp(Pawn pawn)
        {
            if (!pawn.Spawned)
            {
                return null;
            }
            Building_Grave building_Grave = (Building_Grave)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Grave), PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate (Thing x)
            {
                Building_Grave building_Grave2 = (Building_Grave)x;
                return building_Grave2.HasCorpse && IsMyCorpseValid(building_Grave2.Corpse, pawn, true);
            }, null, 0, -1, false, RegionType.Set_Passable, false);
            return building_Grave?.Corpse;
        }

        public static bool IsMyCorpseValid(Corpse corpse, Pawn pawn, bool ignoreReachability = false)
        {
            if (corpse == null || corpse.Destroyed || !corpse.InnerPawn.RaceProps.Humanlike)
            {
                return false;
            }
            if (pawn.carryTracker.CarriedThing == corpse)
            {
                return true;
            }
            if (corpse.Spawned)
            {
                return pawn.CanReserve(corpse, 1, -1, null, false) && (ignoreReachability || pawn.CanReach(corpse, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn));
            }
            Building_Grave building_Grave = corpse.ParentHolder as Building_Grave;
            return building_Grave != null && building_Grave.Spawned && pawn.CanReserve(building_Grave, 1, -1, null, false) && (ignoreReachability || pawn.CanReach(building_Grave, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn));
        }
        
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (GetClosestCorpseToDigUp(pawn) == null)
            {
                return null;
            }
            Corpse corpse = GetClosestCorpseToDigUp(pawn);
            Building_Grave building_Grave = corpse.ParentHolder as Building_Grave;
            if (building_Grave != null)
            {
                if (!pawn.CanReserveAndReach(building_Grave, PathEndMode.InteractionCell, Danger.Deadly, 1, -1, null, false))
                {
                    return null;
                }
            }
            else if (!pawn.CanReserveAndReach(corpse, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
            {
                return null;
            }
            return new Job(CatsAreJerks_JobDefOf.DeliverKill, corpse, building_Grave)
            {
                count = 1
            };
        }
    }
}
