using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
	[StaticConstructorOnStartup]
	public class Need_Mana : Need
	{
		private static readonly Texture2D NeedUnitDividerTex = ContentFinder<Texture2D>.Get("UI/Misc/NeedUnitDivider", true);

		private static readonly Texture2D fillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.3f, 0.5f, 1f));

		private const float BaseManaRecoveryPerIntervalNonSuccubus = 0.001f;

		private const float BaseManaDeficiencyServerityPerInterval = 0.0012f;

		private int lastNonDryingoutManaTick;

		public Need_Mana(Pawn pawn) : base(pawn)
		{
		}

		public override void NeedInterval()
		{
			if (!CaravanUtility.IsCaravanMember(this.pawn))
			{
				CompSuccubusRace comp;
				bool isSuccubus = (comp = this.pawn.GetComp<CompSuccubusRace>()) != null;
				if (isSuccubus)
				{
					if (!base.IsFrozen)
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

						//changing curLevel changes the outcome of this.CurCategory, so it needs to be checked again
						this.CurLevel += 0.001f;
						if (this.CurCategory == ManaAmountCategory.Poverty)
						{
							HealthUtility.AdjustSeverity(this.pawn, NIHediffDefOf.VigorRavage, 0.0012f);
						}
						else
						{
							HealthUtility.AdjustSeverity(this.pawn, NIHediffDefOf.VigorRavage, -0.0012f);
						}
					}
					if (this.CurCategory != ManaAmountCategory.Poverty)
					{
						this.lastNonDryingoutManaTick = Find.TickManager.TicksGame;
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.lastNonDryingoutManaTick, "lastNonDryingoutManaTick", 0, false);
		}

		public override void SetInitialLevel()
		{
			base.CurLevelPercentage = 1f;
		}

		public override int GUIChangeArrow
		{
			get
			{
				return -1;
			}
		}

		public ManaAmountCategory CurCategory
		{
			get
			{
				if (base.CurLevelPercentage <= 0f)
					return ManaAmountCategory.Poverty;
				if(base.CurLevelPercentage < 0.15f)
					return ManaAmountCategory.Lacking;
				if(base.CurLevelPercentage < 0.3f)
					return ManaAmountCategory.Insatisfied;
				return ManaAmountCategory.Satisfied;
			}
		}

		public override float MaxLevel
		{
			get
			{
				bool isSuccubus = this.pawn.story != null && this.pawn.GetComp<CompSuccubusRace>() != null;
				if (isSuccubus)
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

		public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			if (this.threshPercents == null)
			{
				this.threshPercents = new List<float>();
			}
			this.threshPercents.Clear();
			this.threshPercents.Add(0.3f);
			this.threshPercents.Add(0.15f);
			this.DrawOnGUICustom(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
		}

		private void DrawOnGUICustom(Rect guiRect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			if (guiRect.height > 70f)
			{
				float num = (guiRect.height - 70f) / 2f;
				guiRect.height = 70f;
				guiRect.y += num;
			}
			if (Mouse.IsOver(guiRect))
			{
				Widgets.DrawHighlight(guiRect);
			}
			if (doTooltip)
			{
				TooltipHandler.TipRegion(guiRect, new TipSignal(() => this.GetTipString(), guiRect.GetHashCode()));
			}
			float someHeight = 14f;
			float margin = (customMargin < 0f) ? (someHeight + 15f) : customMargin;
			if (guiRect.height < 50f)
			{
				someHeight *= Mathf.InverseLerp(0f, 50f, guiRect.height);
			}
			Text.Font = ((guiRect.height <= 55f) ?GameFont.Tiny : GameFont.Small);
			Text.Anchor = TextAnchor.LowerLeft;

			Rect labelBox = new Rect(guiRect.x + margin + guiRect.width * 0.1f, guiRect.y, guiRect.width - margin - guiRect.width * 0.1f, guiRect.height / 2f);
			Widgets.Label(labelBox, base.LabelCap);
			Text.Anchor = 0;
			Rect instantMarkerBar = new Rect(guiRect.x, guiRect.y + guiRect.height / 2f, guiRect.width, guiRect.height / 2f);
			Rect manaBarRect = new Rect(instantMarkerBar.x + margin, instantMarkerBar.y, instantMarkerBar.width - margin * 2f, instantMarkerBar.height - someHeight);
			float scaleFactor = 1f;
			if (this.def.scaleBar && this.MaxLevel < 1f)
			{
				scaleFactor = this.MaxLevel;
			}
			manaBarRect.width *= scaleFactor;
			Rect barRect = Widgets.FillableBar(manaBarRect, base.CurLevelPercentage, Need_Mana.fillTex);
			if (drawArrows)
			{
				Widgets.FillableBarChangeArrows(manaBarRect, this.GUIChangeArrow);
			}
			if (this.threshPercents != null)
			{
				for (int i = 0; i < Mathf.Min(this.threshPercents.Count, maxThresholdMarkers); i++)
				{
					this.DrawBarThreshold(barRect, this.threshPercents[i] * scaleFactor);
				}
			}
			if (this.def.scaleBar)
			{
				int divisionThreshholdFactor = 1;
				while ((float)divisionThreshholdFactor < this.MaxLevel)
				{
					this.DrawBarDivision(barRect, (float)divisionThreshholdFactor / this.MaxLevel * scaleFactor);
					divisionThreshholdFactor++;
				}
			}
			float curInstantLevelPercentage = base.CurInstantLevelPercentage;
			if (curInstantLevelPercentage >= 0f)
			{
				base.DrawBarInstantMarkerAt(instantMarkerBar, curInstantLevelPercentage * scaleFactor);
			}
			if (!GenText.NullOrEmpty(this.def.tutorHighlightTag))
			{
				UIHighlighter.HighlightOpportunity(guiRect, this.def.tutorHighlightTag);
			}
			Text.Font = GameFont.Small;
		}

		private void DrawBarThreshold(Rect barRect, float threshPct)
		{
			float deviderWidth = (float)((barRect.width <= 60f) ? 1 : 2);
			//draw bar divider?
			Rect drawBar = new Rect(barRect.x + barRect.width * threshPct - (deviderWidth - 1f), barRect.y + barRect.height / 2f, deviderWidth, barRect.height / 2f);
			Texture2D texture2D;
			if (threshPct < base.CurLevelPercentage)
			{
				texture2D = BaseContent.BlackTex;
				GUI.color = new Color(1f, 1f, 1f, 0.9f);
			}
			else
			{
				texture2D = BaseContent.GreyTex;
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
			}
			GUI.DrawTexture(drawBar, texture2D);
			GUI.color = Color.white;
		}

		private void DrawBarDivision(Rect barRect, float threshPct)
		{
			float devisionDistance = 5f;
			Rect rect = new Rect(barRect.x + barRect.width * threshPct - (devisionDistance - 1f), barRect.y, devisionDistance, barRect.height);
			if (threshPct < base.CurLevelPercentage)
			{
				GUI.color = new Color(0f, 0f, 0f, 0.9f);
			}
			else
			{
				GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			}
			Rect unitDevider1 = rect;
			unitDevider1.yMax = unitDevider1.yMin + 4f;
			GUI.DrawTextureWithTexCoords(unitDevider1, Need_Mana.NeedUnitDividerTex, new Rect(0f, 0.5f, 1f, 0.5f));
			Rect unitDivider2 = rect;
			unitDivider2.yMin = unitDivider2.yMax - 4f;
			GUI.DrawTextureWithTexCoords(unitDivider2, Need_Mana.NeedUnitDividerTex, new Rect(0f, 0f, 1f, 0.5f));
			Rect unitDevider3 = rect;
			unitDevider3.yMin = unitDevider1.yMax;
			unitDevider3.yMax = unitDivider2.yMin;
			bool flag2 = unitDevider3.height > 0f;
			if (flag2)
			{
				GUI.DrawTextureWithTexCoords(unitDevider3, Need_Mana.NeedUnitDividerTex, new Rect(0f, 0.4f, 1f, 0.2f));
			}
			GUI.color = Color.white;
		}

	}
}
