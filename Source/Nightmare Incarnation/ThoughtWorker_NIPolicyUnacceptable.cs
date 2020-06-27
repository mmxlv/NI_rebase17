using System;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
	// Token: 0x0200001C RID: 28
	public class ThoughtWorker_NIPolicyUnacceptable : ThoughtWorker
	{
		// Token: 0x06000075 RID: 117 RVA: 0x000066AC File Offset: 0x000048AC
		protected override ThoughtState CurrentStateInternal(Pawn pawn)
		{
			bool flag = pawn.story == null;
			ThoughtState inactive;
			if (flag)
			{
				inactive = ThoughtState.Inactive;
			}
			else
			{
				CompSuccubusRace comp = pawn.GetComp<CompSuccubusRace>();
				bool flag2 = comp == null;
				if (flag2)
				{
					inactive = ThoughtState.Inactive;
				}
				else
				{
					int num = pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute);
					bool flag3 = num == 1 || pawn.story.traits.HasTrait(TraitDefOf.Kind) || pawn.story.traits.HasTrait(TraitDefOf.Ascetic);
					if (flag3)
					{
						bool flag4 = GenCollection.Any<WorkPlan>(comp.policy.plans, (WorkPlan x) => x.AllowJoy);
						if (flag4)
						{
							return ThoughtState.ActiveAtStage(0);
						}
					}
					bool flag5 = num == 3;
					if (flag5)
					{
						bool flag6 = !GenCollection.Any<WorkPlan>(comp.policy.plans, (WorkPlan x) => x.AllowJoy);
						if (flag6)
						{
							return ThoughtState.ActiveAtStage(1);
						}
						bool flag7 = GenCollection.Any<WorkPlan>(comp.policy.plans, (WorkPlan x) => x.MainDishPawn != null);
						if (flag7)
						{
							return ThoughtState.ActiveAtStage(2);
						}
					}
					bool flag8 = num == 5;
					if (flag8)
					{
						bool flag9 = !GenCollection.Any<WorkPlan>(comp.policy.plans, (WorkPlan x) => x.AllowOverSqueeze);
						if (flag9)
						{
							return ThoughtState.ActiveAtStage(3);
						}
					}
					bool flag10 = pawn.story.traits.HasTrait(TraitDefOf.Bloodlust);
					if (flag10)
					{
						bool flag11 = !GenCollection.Any<WorkPlan>(comp.policy.plans, (WorkPlan x) => x.targetType != RelationType.Colonist && x.AllowKill);
						if (flag11)
						{
							return ThoughtState.ActiveAtStage(4);
						}
					}
					inactive = ThoughtState.Inactive;
				}
			}
			return inactive;
		}
	}
}
