using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
	// Token: 0x0200001B RID: 27
	public class ThoughtWorker_NINeedMana : ThoughtWorker
	{
		// Token: 0x06000073 RID: 115 RVA: 0x000065B0 File Offset: 0x000047B0
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			CompSuccubusRace comp = p.GetComp<CompSuccubusRace>();
			bool flag = comp == null;
			ThoughtState result;
			if (flag)
			{
				result = ThoughtState.Inactive;
			}
			else
			{
				List<Need> allNeeds = p.needs.AllNeeds;
				Need need = allNeeds.Find((Need x) => x.def == NINeedDefOf.NI_Need_Mana);
				Need_Mana need_Mana;
				bool flag2 = need != null && (need_Mana = (need as Need_Mana)) != null;
				if (flag2)
				{
					switch (need_Mana.CurCategory)
					{
					case ManaAmountCategory.Satisfied:
						result = ThoughtState.Inactive;
						break;
					case ManaAmountCategory.Insatisfied:
						result = ThoughtState.ActiveAtStage(0);
						break;
					case ManaAmountCategory.Lacking:
						result = ThoughtState.ActiveAtStage(1);
						break;
					case ManaAmountCategory.Poverty:
					{
						Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(NIHediffDefOf.ManaDeficiency, false);
						int num = (firstHediffOfDef != null) ? firstHediffOfDef.CurStageIndex : 0;
						result = ThoughtState.ActiveAtStage(2 + num);
						break;
					}
					default:
						throw new NotImplementedException();
					}
				}
				else
				{
					result = ThoughtState.Inactive;
				}
			}
			return result;
		}
	}
}
