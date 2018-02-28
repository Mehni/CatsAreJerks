using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsAreJerks
{
    public class ThinkNode_ConditionalHasFullHealth : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.Downed || HealthAIUtility.ShouldSeekMedicalRest(pawn) || pawn.IsBurning())
            {
                return false;
            }
            return true;
        }
    }
}