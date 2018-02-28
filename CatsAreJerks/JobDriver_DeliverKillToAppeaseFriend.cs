using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse;
using RimWorld;

namespace CatsAreJerks
{
    //most of this is a variation of vanilla's JobDriver_HaulCorpseToPublicPlace
    public class JobDriver_DeliverKillToAppeaseFriend : JobDriver
    {
        private const TargetIndex CorpseInd = TargetIndex.A;

        private const TargetIndex GraveInd = TargetIndex.B;

        private const TargetIndex CellInd = TargetIndex.C;

        private static List<IntVec3> tmpCells = new List<IntVec3>();

        private Corpse Corpse
        {
            get
            {
                return (Corpse)this.job.GetTarget(CorpseInd).Thing;
            }
        }

        private Building_Grave Grave
        {
            get
            {
                return (Building_Grave)this.job.GetTarget(GraveInd).Thing;
            }
        }

        private bool InGrave
        {
            get
            {
                return this.Grave != null;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(Grave, this.job, 1, -1, null) || this.pawn.Reserve(Corpse, this.job, 1, -1, null);
        }

        public override string GetReport()
        {
            if (this.InGrave && this.Grave.def == ThingDefOf.Grave)
            {
                return "ReportDiggingUpCorpse".Translate();
            }
            return base.GetReport();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CorpseInd);
            Toil gotoCorpse = Toils_Goto.GotoThing(CorpseInd, PathEndMode.Touch).FailOnDespawnedOrNull(CorpseInd);
            yield return Toils_Jump.JumpIfTargetInvalid(GraveInd, gotoCorpse);
            yield return Toils_Goto.GotoThing(GraveInd, PathEndMode.InteractionCell).FailOnDespawnedOrNull(GraveInd);
            yield return Toils_General.Wait(300).WithProgressBarToilDelay(GraveInd, false, -0.5f).FailOnDespawnedOrNull(GraveInd).FailOnCannotTouch(GraveInd, PathEndMode.InteractionCell);
            yield return Toils_General.Open(GraveInd);
            yield return Toils_Reserve.Reserve(CorpseInd, 1, -1, null);
            yield return gotoCorpse;
            yield return Toils_Haul.StartCarryThing(CorpseInd, false, false, false);
            yield return this.FindCellToDropCorpseToil();
            yield return Toils_Reserve.Reserve(CellInd, 1, -1, null);
            yield return Toils_Goto.GotoCell(CellInd, PathEndMode.Touch);
            yield return Toils_Haul.PlaceHauledThingInCell(CellInd, null, false);
            yield return this.Unforbid();
            foreach (Toil t in this.Nibble())
            {
                yield return t;
            }
        }

        private Toil Unforbid()
        {
            return new Toil
            {
                initAction = delegate
                {
                    Corpse corpse = this.Corpse;
                    if (corpse != null)
                    {
                        corpse.SetForbidden(false, true);
                    }
                },
                atomicWithPrevious = true
            };
        }

        private IEnumerable<Toil> Nibble()
        {
            Toil gotoCorpse = Toils_Goto.GotoThing(CorpseInd, PathEndMode.Touch);
            yield return gotoCorpse;
            float durationMultiplier = 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true);
            yield return Toils_Ingest.ChewIngestible(this.pawn, durationMultiplier, CorpseInd, TargetIndex.None).FailOnDespawnedOrNull(CorpseInd).FailOnCannotTouch(CorpseInd, PathEndMode.Touch);
            yield return Toils_Ingest.FinalizeIngest(this.pawn, CorpseInd);
            yield return Toils_Jump.JumpIf(gotoCorpse, () => this.pawn.needs.food.CurLevelPercentage < 0.9f);
        }

        private Toil FindCellToDropCorpseToil()
        {
            return new Toil
            {
                initAction = delegate
                {
                    IntVec3 c = IntVec3.Invalid;
                    if (!this.TryFindBedroom(out c) && !this.TryFindTableCell(out c))
                    {
                        bool cellFound = false;
                        if (RCellFinder.TryFindRandomSpotJustOutsideColony(this.pawn, out IntVec3 root) && CellFinder.TryRandomClosewalkCellNear(root, this.pawn.Map, 5, out c, (IntVec3 x) => this.pawn.CanReserve(x, 1, -1, null, false) && x.GetFirstItem(this.pawn.Map) == null))
                        {
                            cellFound = true;
                        }
                        if (!cellFound)
                        {
                            c = CellFinder.RandomClosewalkCellNear(this.pawn.Position, this.pawn.Map, 10, (IntVec3 x) => this.pawn.CanReserve(x, 1, -1, null, false) && x.GetFirstItem(this.pawn.Map) == null);
                        }
                    }
                    this.job.SetTarget(CellInd, c);
                },
                atomicWithPrevious = true
            };
        }

        private bool TryFindTableCell(out IntVec3 cell)
        {
            tmpCells.Clear();
            List<Building> allBuildingsColonist = this.pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < allBuildingsColonist.Count; i++)
            {
                Building building = allBuildingsColonist[i];
                if (building.def.IsTable)
                {
                    CellRect.CellRectIterator iterator = building.OccupiedRect().GetIterator();
                    while (!iterator.Done())
                    {
                        IntVec3 current = iterator.Current;
                        if (this.pawn.CanReserveAndReach(current, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false) && current.GetFirstItem(this.pawn.Map) == null)
                        {
                            tmpCells.Add(current);
                        }
                        iterator.MoveNext();
                    }
                }
            }
            if (!tmpCells.Any<IntVec3>())
            {
                cell = IntVec3.Invalid;
                return false;
            }
            cell = tmpCells.RandomElement<IntVec3>();
            return true;
        }

        private bool TryFindBedroom(out IntVec3 cell)
        {
            Pawn mostImpactful = GetMostImportantColonyRelationship(Corpse.InnerPawn);
            if (mostImpactful?.ownership?.OwnedRoom != null)
            {
                Room masterBedroom = mostImpactful.ownership.OwnedRoom;

                IEnumerable<IntVec3> intVec3 = from c in masterBedroom.Cells
                                               where c.Standable(this.pawn.Map) && !c.IsForbidden(this.pawn) && this.pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None, 1, -1, null, false)
                                               select c;

                intVec3.TryRandomElement(out IntVec3 vec3);
                cell = vec3;
                return true;
            }
            cell = IntVec3.Invalid;
            return false;
        }

        public static Pawn GetMostImportantColonyRelationship(Pawn pawn)
        {

            IEnumerable<Pawn> freeFriendsOfDeadColonist = from x in pawn.MapHeld.mapPawns.FreeColonistsAndPrisonersSpawned
                                                           where x.relations.everSeenByPlayer
                                                            select x;

            float num = 0f;
            Pawn pawn2 = null;
            foreach (Pawn current in freeFriendsOfDeadColonist)
            {
                PawnRelationDef mostImportantRelation = pawn.GetMostImportantRelation(current);
                if (mostImportantRelation != null)
                {
                    if (pawn2 == null || mostImportantRelation.importance > num)
                    {
                        num = mostImportantRelation.importance;
                        pawn2 = current;
                        Log.Message(pawn2.Label);
                    }
                }
            }
            return pawn2;
        }
    }
}
