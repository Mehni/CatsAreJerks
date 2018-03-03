using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsAreJerks
{
    class JobDriver_PetTheCat : JobDriver
    {
        //we'll be defining CatToPet as TargetIndex.A
        //It shows up as TargetIndex.A in decompiled code, but this is easy to mistype
        //This also prevents confusion when our TargetIndex.A contains both a List and a Thing.
        
        //Likewise, we define our NuzzleDuration in here too, since that'll be easier to change.
        private const TargetIndex CatToPet = TargetIndex.A;
        private const int NuzzleDuration = 500;

        //reserve the cat, so other pawns can't pet it. This is OUR cat!
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.GetTarget(CatToPet), this.job, 1, -1, null);
        }

        //protected override IEnumerable<Toil> MakeNewToils()
        //{

        //    //these are the fail conditions. If at any time during this toil the cat becomes unavailable, our toil ends.
        //    this.FailOnDespawnedNullOrForbidden(CatToPet);
        //    this.FailOnDowned(CatToPet);
        //    //this.FailOnNotCasualInterruptible(CatToPet);

        //    //go to our cat. These are all vanilla Toils, which is easy to use.
        //    yield return Toils_Goto.GotoThing(CatToPet, PathEndMode.Touch);
        //    yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
        //    yield return Toils_Interpersonal.GotoInteractablePosition(CatToPet);

        //    //the RandomSocialMode is a special case for this part of our toil.
        //    //While patting the cat, don't chat with others. We set this per toil.

        //    //It's not possible to set the social mode in the yield return statement so,
        //    //first we define the Toil, and then assign the mode while we return it.
        //    Toil gotoTarget = Toils_Goto.GotoThing(CatToPet, PathEndMode.Touch);
        //    gotoTarget.socialMode = RandomSocialMode.Off;

        //    //There are two wait toils: Wait simply stops the pawn for an amount of ticks. 
        //    //The fancy WaitWith we're using has optional parameters like a progress bar and a TargetIndex.
        //    Toil wait = Toils_General.WaitWith(CatToPet, NuzzleDuration, false, true);
        //    wait.socialMode = RandomSocialMode.Off;

        //    //sometimes a Toil must be converted into a delegate. 
        //    //Since we can't directly call non-Toil functions or methods inside the IEnumerable,
        //    //we call on Toils_General.Do. There are other options for this, see the next chapter for more.

        //    //yield return Toils_General.Do(delegate
        //    //{
        //    //    Pawn petter = this.pawn;
        //    //    Pawn pettee = (Pawn)this.pawn.CurJob.targetA.Thing;
        //    //    pettee.interactions.TryInteractWith(petter, InteractionDefOf.Nuzzle);
        //    //});
        //    yield return new Toil
        //    {
        //        initAction = delegate
        //        {
        //            Pawn petter = this.pawn;
        //            Pawn pettee = (Pawn)this.pawn.CurJob.targetA.Thing;
        //            pettee.interactions.TryInteractWith(petter, InteractionDefOf.Nuzzle);
        //        },
        //        defaultCompleteMode = ToilCompleteMode.Delay,
        //        defaultDuration = this.job.def.joyDuration
        //    };
        //}

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(CatToPet);
            this.FailOnDowned(CatToPet);

            Toil gotoTarget = Toils_Goto.GotoThing(CatToPet, PathEndMode.Touch);
            yield return gotoTarget;
            yield return Toils_Reserve.Reserve(CatToPet, 1, -1, null);
            Toil play = new Toil
            {
                initAction = delegate
                {
                    this.job.locomotionUrgency = LocomotionUrgency.Jog;
                },
                tickAction = delegate
                {
                    if (Find.TickManager.TicksGame > this.startTick + this.job.def.joyDuration)
                    {
                        Pawn petter = this.pawn;
                        Pawn pettee = (Pawn)this.pawn.CurJob.targetA.Thing;
                        pettee.interactions.TryInteractWith(petter, InteractionDefOf.Nuzzle);
                        this.EndJobWith(JobCondition.Succeeded);
                        return;
                    }
                    JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob);
                },
                socialMode = RandomSocialMode.Off,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 60
            };
            play.AddFinishAction(delegate
            {
                JoyUtility.TryGainRecRoomThought(this.pawn);
            });
            yield return play;
            yield return Toils_Reserve.Release(CatToPet);
            yield return Toils_Jump.Jump(gotoTarget);
        }
    }
}
