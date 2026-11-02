using RimWorld;
using Verse;

namespace NightmareIncarnation
{
    public class CompProperties_AbilitySuccubusKnockDown : CompProperties_AbilityEffect
    {
        public float manaCost = 1f;

        public CompProperties_AbilitySuccubusKnockDown()
        {
            compClass = typeof(CompAbilityEffect_SuccubusKnockDown);
        }
    }

    public class CompAbilityEffect_SuccubusKnockDown : CompAbilityEffect
    {
        public new CompProperties_AbilitySuccubusKnockDown Props => (CompProperties_AbilitySuccubusKnockDown)props;

        public override bool GizmoDisabled(out string reason)
        {
            Need_Mana mana = parent.pawn.needs?.TryGetNeed<Need_Mana>();
            if (mana == null || mana.CurLevel < Props.manaCost)
            {
                reason = "Requires at least 1 mana.";
                return true;
            }

            reason = null;
            return false;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn targetPawn = target.Pawn;
            bool valid = targetPawn != null
                         && targetPawn != parent.pawn
                         && targetPawn.Spawned
                         && !targetPawn.Downed
                         && !targetPawn.Dead
                         && targetPawn.Map == parent.pawn.Map;
            if (!valid && throwMessages)
                Messages.Message("Must target a living, standing pawn.", targetPawn, MessageTypeDefOf.RejectInput);
            return valid;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn targetPawn = target.Pawn;
            Need_Mana mana = parent.pawn.needs?.TryGetNeed<Need_Mana>();
            if (targetPawn == null || mana == null || mana.CurLevel < Props.manaCost)
                return;

            mana.CurLevel -= Props.manaCost;
            Hediff anesthetic = HediffMaker.MakeHediff(HediffDefOf.Anesthetic, targetPawn);
            anesthetic.Severity = 1f;
            targetPawn.health.AddHediff(anesthetic);
        }
    }
}
