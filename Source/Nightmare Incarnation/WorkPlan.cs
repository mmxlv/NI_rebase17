using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NightmareIncarnation
{
	// Token: 0x02000020 RID: 32
	public class WorkPlan : IExposable, ILoadReferenceable
	{
		// Token: 0x06000084 RID: 132 RVA: 0x00007130 File Offset: 0x00005330
		public WorkPlan()
		{
			bool flag = this.loadID < 0;
			if (flag)
			{
				int num = NI_WorldManager.loadIDs.Count + 1;
				Predicate<int> predicate = (int x) => x == num;
				for (;;)
				{
					List<int> loadIDs = NI_WorldManager.loadIDs;
					if (!GenCollection.Any<int>(loadIDs, predicate))
					{
						break;
					}
					int num2 = num;
					num = num2 + 1;
				}
				this.loadID = num;
				NI_WorldManager.loadIDs.Add(this.loadID);
			}
			else
			{
				NI_WorldManager.loadIDs.Add(this.loadID);
			}
		}

		// Token: 0x06000085 RID: 133 RVA: 0x000071F4 File Offset: 0x000053F4
		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.loadID, "loadID", 0, false);
			Scribe_Values.Look<RelationType>(ref this.targetType, "targetType", RelationType.Colonist, false);
			Scribe_Values.Look<bool>(ref this.Suspended, "suspended", false, false);
			Scribe_Values.Look<bool>(ref this.AllowJoy, "AllowJoy", false, false);
			Scribe_Values.Look<bool>(ref this.AllowOverSqueeze, "AllowOverSqueeze", false, false);
			Scribe_Values.Look<bool>(ref this.AllowDown, "AllowDown", false, false);
			Scribe_Values.Look<bool>(ref this.AllowKill, "AllowKill", false, false);
			Scribe_Values.Look<float>(ref this.SqueezeValue, "SqueezeValue", 0.8f, false);
			Scribe_Defs.Look<ThingDef>(ref this.MainDishThing, "MainDishThingDef");
			Scribe_References.Look<Pawn>(ref this.MainDishPawn, "MainDishPawn", false);
		}

		// Token: 0x06000086 RID: 134 RVA: 0x000072C4 File Offset: 0x000054C4
		public Rect DoInterface(float x, float y, float width, int index)
		{
			Rect rect= new Rect(x, y, width, 53f);
			rect.height += 24f;
			Color white = Color.white;
			bool suspended = this.Suspended;
			if (suspended)
			{
				white=new Color(1f, 0.7f, 0.7f, 0.7f);
			}
			GUI.color = white;
			Text.Font = GameFont.Small;
			bool flag = index % 2 == 0;
			if (flag)
			{
				Widgets.DrawAltRect(rect);
			}
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, 24f, 24f);
			bool flag2 = this.workPlanStack.IndexOf(this) > 0;
			if (flag2)
			{
				bool flag3 = Widgets.ButtonImage(rect2, TexButton.ReorderUp, white);
				if (flag3)
				{
					this.workPlanStack.Reorder(this, -1);
					SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, null);
				}
				TooltipHandler.TipRegion(rect2, Translator.Translate("ReorderBillUpTip"));
			}
			bool flag4 = this.workPlanStack.IndexOf(this) < this.workPlanStack.plans.Count - 1;
			if (flag4)
			{
				Rect rect3= new Rect(0f, 48f, 24f, 24f);
				bool flag5 = Widgets.ButtonImage(rect3, TexButton.ReorderDown, white);
				if (flag5)
				{
					this.workPlanStack.Reorder(this, 1);
					SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low, null);
				}
				TooltipHandler.TipRegion(rect3, Translator.Translate("ReorderBillDownTip"));
			}
			Rect rect4 = new Rect(28f, 0f, rect.width - 48f - 20f, rect.height + 5f);
			Widgets.Label(rect4, this.LabelCap);
			Rect rect5 = new Rect(rect.width - 24f, 0f, 24f, 24f);
			Rect rect6=new Rect(rect5);
			rect6.x -= rect6.width + 4f;
			bool flag6 = Widgets.ButtonImage(rect6, TexButton.Suspend, white);
			if (flag6)
			{
				this.Suspended = !this.Suspended;
				SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, null);
			}
			TooltipHandler.TipRegion(rect6, Translator.Translate("SuspendBillTip"));
			Rect rect7=new Rect(28f, 28f, 40f, 40f);
			bool flag7 = this.targetType != RelationType.Meal;
			if (flag7)
			{
				bool flag8 = Widgets.ButtonImage(rect7, this.AllowJoy ? IconTex.Policy_AllowJoy : IconTex.Policy_DisallowJoy, white);
				if (flag8)
				{
					this.AllowJoy = !this.AllowJoy;
					bool allowJoy = this.AllowJoy;
					if (allowJoy)
					{
						SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, null);
					}
					else
					{
						SoundStarter.PlayOneShotOnCamera(SoundDefOf.Designate_Cancel, null);
					}
				}
			}
			TooltipHandler.TipRegion(rect7, Translator.Translate("NItxt_AllowJoyTip"));
			Rect rect8 = new Rect(72f, 28f, 40f, 40f);
			bool flag9 = this.targetType != RelationType.Meal;
			if (flag9)
			{
				bool flag10 = Widgets.ButtonImage(rect8, this.AllowOverSqueeze ? IconTex.Policy_AllowOverSqueeze : IconTex.Policy_DisallowOverSqueeze, white);
				if (flag10)
				{
					this.AllowOverSqueeze = !this.AllowOverSqueeze;
					bool allowOverSqueeze = this.AllowOverSqueeze;
					if (allowOverSqueeze)
					{
						SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, null);
					}
					else
					{
						this.AllowKill = false;
						SoundStarter.PlayOneShotOnCamera(SoundDefOf.Designate_Cancel, null);
					}
				}
			}
			TooltipHandler.TipRegion(rect8, Translator.Translate("NItxt_AllowOverSqueezeTip"));
			Rect rect9 = new Rect(116f, 28f, 40f, 40f);
			bool flag11 = this.targetType != RelationType.Meal && this.targetType > RelationType.Colonist;
			if (flag11)
			{
				bool flag12 = Widgets.ButtonImage(rect9, this.AllowDown ? IconTex.Policy_AllowDown : IconTex.Policy_DisallowDown, white);
				if (flag12)
				{
					this.AllowDown = !this.AllowDown;
					bool allowDown = this.AllowDown;
					if (allowDown)
					{
						SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, null);
					}
					else
					{
						SoundStarter.PlayOneShotOnCamera(SoundDefOf.Designate_Cancel, null);
					}
				}
			}
			TooltipHandler.TipRegion(rect9, Translator.Translate("NItxt_AllowDownTip"));
			Rect rect10=new Rect(160f, 28f, 40f, 40f);
			bool flag13 = this.targetType != RelationType.Meal && this.targetType > RelationType.Colonist;
			if (flag13)
			{
				bool flag14 = Widgets.ButtonImage(rect10, this.AllowKill ? IconTex.Policy_AllowKill : IconTex.Policy_DisallowKill, white);
				if (flag14)
				{
					this.AllowKill = !this.AllowKill;
					bool allowKill = this.AllowKill;
					if (allowKill)
					{
						this.AllowOverSqueeze = true;
						this.AllowDown = true;
						SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, null);
					}
					else
					{
						SoundStarter.PlayOneShotOnCamera(SoundDefOf.Designate_Cancel, null);
					}
				}
			}
			TooltipHandler.TipRegion(rect10, Translator.Translate("NItxt_AllowKillTip"));
			bool flag15 = this.targetType == RelationType.Meal;
			if (flag15)
			{
				Func<List<FloatMenuOption>> func = delegate()
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					list.Add(new FloatMenuOption(Translator.Translate("NoneBrackets"), delegate()
					{
						this.MainDishThing = null;
					},MenuOptionPriority.Default, null, null, 0f, null, null));
					using (IEnumerator<ThingDef> enumerator = CommonData.ManaSourceDefs.AsEnumerable<ThingDef>().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ThingDef t = enumerator.Current;
							list.Add(new FloatMenuOption(t.LabelCap, delegate()
							{
								this.MainDishThing = t;
							}, MenuOptionPriority.Default, null, null, 29f, (Rect tmprect) => Widgets.InfoCardButton(tmprect.x + 5f, tmprect.y + (tmprect.height - 24f) / 2f, t), null));
						}
					}
					return list;
				};
				Rect rect11 = new Rect(204f, 32f, 150f, 29f);
				bool flag16 = Widgets.ButtonText(rect11, (this.MainDishThing != null) ? this.MainDishThing.LabelCap : Translator.Translate("NItxt_SelectMainDish"), true, false, true);
				if (flag16)
				{
					Find.WindowStack.Add(new FloatMenu(func()));
				}
				UIHighlighter.HighlightOpportunity(rect11, "NItxt_SelectMainDish");
				TooltipHandler.TipRegion(rect11, Translator.Translate("NItxt_SelectMainDishThingTip"));
			}
			bool flag17 = this.targetType == RelationType.Colonist || this.targetType == RelationType.Prisoner;
			if (flag17)
			{
				Func<List<FloatMenuOption>> func2 = delegate()
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					list.Add(new FloatMenuOption(Translator.Translate("NoneBrackets"), delegate()
					{
						this.MainDishPawn = null;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
					IEnumerable<Pawn> enumerable = from z in this.workPlanStack.compRace.parent.Map.mapPawns.AllPawnsSpawned
					where z.GetComp<CompSuccubusRace>() == null && z != this.workPlanStack.compRace.parent && commonUtl.IsManaConformant(z) && ((this.targetType == RelationType.Colonist && z.IsColonist) || (this.targetType == RelationType.Prisoner && z.IsPrisonerOfColony))
					select z;
					using (IEnumerator<Pawn> enumerator = enumerable.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Pawn p = enumerator.Current;
							list.Add(new FloatMenuOption(p.LabelCap, delegate()
							{
								this.MainDishPawn = p;
							}, MenuOptionPriority.Default, null, null, 29f, (Rect tmprect) => Widgets.InfoCardButton(tmprect.x + 5f, tmprect.y + (tmprect.height - 24f) / 2f, p), null));
						}
					}
					return list;
				};
				Rect rect12= new Rect(204f, 32f, 150f, 29f);
				bool flag18 = Widgets.ButtonText(rect12, (this.MainDishPawn != null) ? this.MainDishPawn.LabelCap : Translator.Translate("NItxt_SelectMainDish").ToString(), true, false, true);
				if (flag18)
				{
					Find.WindowStack.Add(new FloatMenu(func2()));
				}
				UIHighlighter.HighlightOpportunity(rect12, "NItxt_SelectMainDish");
				TooltipHandler.TipRegion(rect12, Translator.Translate("NItxt_SelectMainDishTip"));
			}
			bool flag19 = this.targetType != RelationType.Meal;
			if (flag19)
			{
				Rect rect13= new Rect(144f, 0f, 160f, 34f);
				float squeezeValue = this.SqueezeValue;
				this.SqueezeValue = Widgets.HorizontalSlider(rect13, squeezeValue, 0.4f, 2.4f, false, TranslatorFormattedStringExtensions.Translate("NItxt_SqueezeValue", squeezeValue.ToString("F1")), null, null, 0.1f);
				TooltipHandler.TipRegion(rect13, Translator.Translate("NItxt_SqueezeSliderTip"));
			}
			GUI.EndGroup();
			bool suspended2 = this.Suspended;
			if (suspended2)
			{
				Text.Font = GameFont.Medium;
				Text.Anchor = TextAnchor.MiddleCenter;
				Rect rect14 = new Rect(rect.x + rect.width / 2f - 70f, rect.y + rect.height / 2f - 20f, 140f, 40f);
				GUI.DrawTexture(rect14, TexUI.GrayTextBG);
				Widgets.Label(rect14, Translator.Translate("SuspendedCaps"));
				Text.Anchor = 0;
				Text.Font = GameFont.Small;
			}
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			return rect;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00007A68 File Offset: 0x00005C68
		public string GetUniqueLoadID()
		{
			return string.Concat(new object[]
			{
				"Plan_",
				this.targetType,
				"_",
				this.loadID
			});
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000088 RID: 136 RVA: 0x00007AB4 File Offset: 0x00005CB4
		public string LabelCap
		{
			get
			{
				switch (this.targetType)
				{
				case RelationType.Colonist:
					return Translator.Translate("NItxt_Colonist");
				case RelationType.Prisoner:
					return Translator.Translate("NItxt_Prisoner");
				case RelationType.Hostile:
					return Translator.Translate("NItxt_Enemy");
				case RelationType.Meal:
					return Translator.Translate("NItxt_Meal");
				}
				return Translator.Translate("NItxt_Other");
			}
		}

		// Token: 0x040000AB RID: 171
		public int loadID = -1;

		// Token: 0x040000AC RID: 172
		[Unsaved]
		public WorkPlanStack workPlanStack;

		// Token: 0x040000AD RID: 173
		public bool Suspended;

		// Token: 0x040000AE RID: 174
		public RelationType targetType;

		// Token: 0x040000AF RID: 175
		public bool AllowJoy;

		// Token: 0x040000B0 RID: 176
		public bool AllowOverSqueeze;

		// Token: 0x040000B1 RID: 177
		public bool AllowDown;

		// Token: 0x040000B2 RID: 178
		public bool AllowKill;

		// Token: 0x040000B3 RID: 179
		public float SqueezeValue = 0.8f;

		// Token: 0x040000B4 RID: 180
		public ThingDef MainDishThing;

		// Token: 0x040000B5 RID: 181
		public Pawn MainDishPawn;
	}
}
