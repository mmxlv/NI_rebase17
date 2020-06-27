using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
	// Token: 0x02000009 RID: 9
	[StaticConstructorOnStartup]
	public class Need_Mana : Need
	{
		// Token: 0x0600002C RID: 44 RVA: 0x00003AB4 File Offset: 0x00001CB4
		public Need_Mana(Pawn pawn) : base(pawn)
		{
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003AC0 File Offset: 0x00001CC0
		public override void NeedInterval()
		{
			bool flag = CaravanUtility.IsCaravanMember(this.pawn);
			if (!flag)
			{
				CompSuccubusRace comp;
				bool flag2 = (comp = this.pawn.GetComp<CompSuccubusRace>()) != null;
				if (flag2)
				{
					bool flag3 = !base.IsFrozen;
					if (flag3)
					{
						switch (this.CurCategory)
						{
						case ManaAmountCategory.Satisfied:
							this.CurLevel -= comp.ManaFallPerTick * 150f * comp.ManaFallFactor;
							break;
						case ManaAmountCategory.Insatisfied:
							this.CurLevel -= comp.ManaFallPerTick * 150f * comp.ManaFallFactor * 0.5f;
							break;
						case ManaAmountCategory.Lacking:
							this.CurLevel -= comp.ManaFallPerTick * 150f * comp.ManaFallFactor * 0.25f;
							break;
						}
					}
					bool flag4 = this.CurCategory != ManaAmountCategory.Poverty;
					if (flag4)
					{
						this.lastNonDryingoutManaTick = Find.TickManager.TicksGame;
					}
					bool flag5 = !base.IsFrozen;
					if (flag5)
					{
						bool flag6 = this.CurCategory == ManaAmountCategory.Poverty;
						if (flag6)
						{
							HealthUtility.AdjustSeverity(this.pawn, NIHediffDefOf.ManaDeficiency, 0.0012f);
						}
						else
						{
							HealthUtility.AdjustSeverity(this.pawn, NIHediffDefOf.ManaDeficiency, -0.0012f);
						}
					}
				}
				else
				{
					bool flag7 = !base.IsFrozen;
					if (flag7)
					{
						this.CurLevel += 0.001f;
						bool flag8 = this.CurCategory == ManaAmountCategory.Poverty;
						if (flag8)
						{
							HealthUtility.AdjustSeverity(this.pawn, NIHediffDefOf.VigorRavage, 0.0012f);
						}
						else
						{
							HealthUtility.AdjustSeverity(this.pawn, NIHediffDefOf.VigorRavage, -0.0012f);
						}
					}
				}
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003C7F File Offset: 0x00001E7F
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.lastNonDryingoutManaTick, "lastNonDryingoutManaTick", 0, false);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003C9C File Offset: 0x00001E9C
		public override void SetInitialLevel()
		{
			base.CurLevelPercentage = 1f;
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000030 RID: 48 RVA: 0x00003CAC File Offset: 0x00001EAC
		public override int GUIChangeArrow
		{
			get
			{
				return -1;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000031 RID: 49 RVA: 0x00003CC0 File Offset: 0x00001EC0
		public ManaAmountCategory CurCategory
		{
			get
			{
				bool flag = base.CurLevelPercentage <= 0f;
				ManaAmountCategory result;
				if (flag)
				{
					result = ManaAmountCategory.Poverty;
				}
				else
				{
					bool flag2 = base.CurLevelPercentage < 0.15f;
					if (flag2)
					{
						result = ManaAmountCategory.Lacking;
					}
					else
					{
						bool flag3 = base.CurLevelPercentage < 0.3f;
						if (flag3)
						{
							result = ManaAmountCategory.Insatisfied;
						}
						else
						{
							result = ManaAmountCategory.Satisfied;
						}
					}
				}
				return result;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000032 RID: 50 RVA: 0x00003D18 File Offset: 0x00001F18
		public override float MaxLevel
		{
			get
			{
				bool flag = this.pawn.story != null && this.pawn.GetComp<CompSuccubusRace>() != null;
				if (flag)
				{
					switch (this.pawn.story.traits.DegreeOfTrait(NITraitDefOf.NI_ManaAttribute))
					{
					case 1:
					case 2:
					case 3:
						return 1f;
					case 4:
						return 2f;
					case 5:
						return 2f;
					case 6:
						return 5f;
					}
				}
				return 1f;
			}
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00003DBC File Offset: 0x00001FBC
		public override string GetTipString()
		{
			return string.Concat(new string[]
			{
				base.LabelCap,
				": ",
				GenText.ToStringPercent(base.CurLevelPercentage),
				" (",
				this.CurLevel.ToString("0.##"),
				" / ",
				this.MaxLevel.ToString("0.##"),
				")\n",
				this.def.description
			});
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00003E4C File Offset: 0x0000204C
		public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			bool flag = this.threshPercents == null;
			if (flag)
			{
				this.threshPercents = new List<float>();
			}
			this.threshPercents.Clear();
			this.threshPercents.Add(0.3f);
			this.threshPercents.Add(0.15f);
			this.DrawOnGUICustum(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00003EB0 File Offset: 0x000020B0
		private void DrawOnGUICustum(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			bool flag = rect.height > 70f;
			if (flag)
			{
				float num = (rect.height - 70f) / 2f;
				rect.height = 70f;
				rect.y += num;
			}
			bool flag2 = Mouse.IsOver(rect);
			if (flag2)
			{
				Widgets.DrawHighlight(rect);
			}
			if (doTooltip)
			{
				TooltipHandler.TipRegion(rect, new TipSignal(() => this.GetTipString(), rect.GetHashCode()));
			}
			float num2 = 14f;
			float num3 = (customMargin < 0f) ? (num2 + 15f) : customMargin;
			bool flag3 = rect.height < 50f;
			if (flag3)
			{
				num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
			}
			Text.Font = ((rect.height <= 55f) ?GameFont.Tiny : GameFont.Small);
			Text.Anchor = TextAnchor.LowerLeft;

			Rect rect2;
			rect2 = new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f);
			Widgets.Label(rect2, base.LabelCap);
			Text.Anchor = 0;
			Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
			Rect rect4 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
			float num4 = 1f;
			bool flag4 = this.def.scaleBar && this.MaxLevel < 1f;
			if (flag4)
			{
				num4 = this.MaxLevel;
			}
			rect4.width *= num4;
			Rect barRect = Widgets.FillableBar(rect4, base.CurLevelPercentage, Need_Mana.fillTex);
			if (drawArrows)
			{
				Widgets.FillableBarChangeArrows(rect4, this.GUIChangeArrow);
			}
			bool flag5 = this.threshPercents != null;
			if (flag5)
			{
				for (int i = 0; i < Mathf.Min(this.threshPercents.Count, maxThresholdMarkers); i++)
				{
					this.DrawBarThreshold(barRect, this.threshPercents[i] * num4);
				}
			}
			bool scaleBar = this.def.scaleBar;
			if (scaleBar)
			{
				int num5 = 1;
				while ((float)num5 < this.MaxLevel)
				{
					this.DrawBarDivision(barRect, (float)num5 / this.MaxLevel * num4);
					num5++;
				}
			}
			float curInstantLevelPercentage = base.CurInstantLevelPercentage;
			bool flag6 = curInstantLevelPercentage >= 0f;
			if (flag6)
			{
				base.DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage * num4);
			}
			bool flag7 = !GenText.NullOrEmpty(this.def.tutorHighlightTag);
			if (flag7)
			{
				UIHighlighter.HighlightOpportunity(rect, this.def.tutorHighlightTag);
			}
			Text.Font = GameFont.Small;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x000041E0 File Offset: 0x000023E0
		private void DrawBarThreshold(Rect barRect, float threshPct)
		{
			float num = (float)((barRect.width <= 60f) ? 1 : 2);
			Rect rect = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
			bool flag = threshPct < base.CurLevelPercentage;
			Texture2D texture2D;
			if (flag)
			{
				texture2D = BaseContent.BlackTex;
				GUI.color = new Color(1f, 1f, 1f, 0.9f);
			}
			else
			{
				texture2D = BaseContent.GreyTex;
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
			}
			GUI.DrawTexture(rect, texture2D);
			GUI.color = Color.white;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000042B8 File Offset: 0x000024B8
		private void DrawBarDivision(Rect barRect, float threshPct)
		{
			float num = 5f;
			Rect rect = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y, num, barRect.height);
			bool flag = threshPct < base.CurLevelPercentage;
			if (flag)
			{
				GUI.color = new Color(0f, 0f, 0f, 0.9f);
			}
			else
			{
				GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			}
			Rect rect2 = rect;
			rect2.yMax = rect2.yMin + 4f;
			GUI.DrawTextureWithTexCoords(rect2, Need_Mana.NeedUnitDividerTex, new Rect(0f, 0.5f, 1f, 0.5f));
			Rect rect3 = rect;
			rect3.yMin = rect3.yMax - 4f;
			GUI.DrawTextureWithTexCoords(rect3, Need_Mana.NeedUnitDividerTex, new Rect(0f, 0f, 1f, 0.5f));
			Rect rect4 = rect;
			rect4.yMin = rect2.yMax;
			rect4.yMax = rect3.yMin;
			bool flag2 = rect4.height > 0f;
			if (flag2)
			{
				GUI.DrawTextureWithTexCoords(rect4, Need_Mana.NeedUnitDividerTex, new Rect(0f, 0.4f, 1f, 0.2f));
			}
			GUI.color = Color.white;
		}

		// Token: 0x04000050 RID: 80
		private static readonly Texture2D NeedUnitDividerTex = ContentFinder<Texture2D>.Get("UI/Misc/NeedUnitDivider", true);

		// Token: 0x04000051 RID: 81
		private static readonly Texture2D fillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.3f, 0.5f, 1f));

		// Token: 0x04000052 RID: 82
		private const float BaseManaRecoveryPerIntervalNonSuccubus = 0.001f;

		// Token: 0x04000053 RID: 83
		private const float BaseManaDeficiencyServerityPerInterval = 0.0012f;

		// Token: 0x04000054 RID: 84
		private int lastNonDryingoutManaTick;
	}
}
