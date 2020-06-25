using System;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
	// Token: 0x0200001A RID: 26
	public class ThoughtWorker_NIManaTrait : ThoughtWorker
	{
		// Token: 0x06000071 RID: 113 RVA: 0x00006468 File Offset: 0x00004668
		protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
		{
			bool flag = !other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other);
			ThoughtState result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = RelationsUtility.IsDisfigured(other);
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight);
					if (flag3)
					{
						result = false;
					}
					else
					{
						bool flag4 = other.story == null || !commonUtl.IsSuccubus(other);
						if (flag4)
						{
							result = false;
						}
						else
						{
							bool flag5 = commonUtl.IsSuccubus(pawn) && commonUtl.IsSuccubus(other);
							if (flag5)
							{
								result = ThoughtState.ActiveAtStage(4);
							}
							else
							{
								switch (other.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute))
								{
								case 1:
									result = ThoughtState.ActiveAtStage(0);
									break;
								case 2:
									result = ThoughtState.ActiveAtStage(1);
									break;
								case 3:
									result = ThoughtState.ActiveAtStage(1);
									break;
								case 4:
									result = ThoughtState.ActiveAtStage(2);
									break;
								case 5:
									result = ThoughtState.ActiveAtStage(2);
									break;
								case 6:
									result = ThoughtState.ActiveAtStage(3);
									break;
								default:
									result = false;
									break;
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
}
