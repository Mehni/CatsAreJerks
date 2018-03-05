using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsAreJerks
{
    public class ThinkNode_ConditionalNightTime : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (GenLocalDate.DayPercent(pawn) > 0.024 && GenLocalDate.DayPercent(pawn) < 0.048)
            {
                return true;
            }
            return false;
        }
    }
}