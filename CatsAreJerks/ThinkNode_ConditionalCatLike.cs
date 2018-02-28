using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace CatsAreJerks
{
    public class ThinkNode_ConditionalCatLike : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.RaceProps.predator && pawn.RaceProps.wildness <= 0.3 && pawn.BodySize <= 0.3 && pawn.RaceProps.petness >= 0.1)
            {
                return true;
            }
            return false;
        }
    }
}