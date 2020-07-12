using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NightmareIncarnation
{
    public class WorkPlan : IExposable, ILoadReferenceable
    {
        public int loadID = -1;
        [Unsaved]
        public WorkPlanStack workPlanStack;

        public bool Suspended;

        public RelationType targetType;

        public bool AllowJoy;

        public bool AllowOverSqueeze;

        public bool AllowDown;

        public bool AllowKill;

        public float SqueezeValue = 0.8f;

        public ThingDef MainDishThing;

        public Pawn MainDishPawn;

        public WorkPlan()
        {
            if (this.loadID < 0)
            {
                int nextFreeLoadID = NI_WorldManager.loadIDs.Count + 1;
                Predicate<int> predicate = (int x) => x == nextFreeLoadID;
                for (; ; )
                {
                    List<int> loadIDs = NI_WorldManager.loadIDs;
                    if (!GenCollection.Any<int>(loadIDs, predicate))
                    {
                        break;
                    }
                    nextFreeLoadID++;
                }
                this.loadID = nextFreeLoadID;
                NI_WorldManager.loadIDs.Add(this.loadID);
            }
            else
            {
                NI_WorldManager.loadIDs.Add(this.loadID);
            }
        }

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

        public Rect DoInterface(float x, float y, float width, int index)
        {
            Rect workPlanInterface = new Rect(x, y, width, 53f);
            workPlanInterface.height += 24f;
            Color white = Color.white;
            if (this.Suspended)
            {
                white = new Color(1f, 0.7f, 0.7f, 0.7f);
            }
            GUI.color = white;
            Text.Font = GameFont.Small;
            if (index % 2 == 0)
            {
                Widgets.DrawAltRect(workPlanInterface);
            }
            GUI.BeginGroup(workPlanInterface);
            Rect moveBillUpButton = new Rect(0f, 0f, 24f, 24f);
            if (this.workPlanStack.IndexOf(this) > 0)
            {
                if (Widgets.ButtonImage(moveBillUpButton, TexButton.ReorderUp, white))
                {
                    this.workPlanStack.Reorder(this, -1);
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, null);
                }
                TooltipHandler.TipRegion(moveBillUpButton, Translator.Translate("ReorderBillUpTip"));
            }
            if (this.workPlanStack.IndexOf(this) < this.workPlanStack.plans.Count - 1)
            {
                Rect moveBillDownButton = new Rect(0f, 48f, 24f, 24f);
                bool flag5 = Widgets.ButtonImage(moveBillDownButton, TexButton.ReorderDown, white);
                if (flag5)
                {
                    this.workPlanStack.Reorder(this, 1);
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low, null);
                }
                TooltipHandler.TipRegion(moveBillDownButton, Translator.Translate("ReorderBillDownTip"));
            }
            Rect labelBox = new Rect(28f, 0f, workPlanInterface.width - 48f - 20f, workPlanInterface.height + 5f);
            Widgets.Label(labelBox, this.LabelCap);
            Rect suspendBillButton = new Rect(workPlanInterface.width - 24f, 0f, 24f, 24f);
            suspendBillButton.x -= suspendBillButton.width + 4f;
            if (Widgets.ButtonImage(suspendBillButton, TexButton.Suspend, white))
            {
                this.Suspended = !this.Suspended;
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, null);
            }
            TooltipHandler.TipRegion(suspendBillButton, Translator.Translate("SuspendBillTip"));
            Rect allowJoyButton = new Rect(28f, 28f, 40f, 40f);
            if (this.targetType != RelationType.Meal)
            {
                if (Widgets.ButtonImage(allowJoyButton, this.AllowJoy ? IconTex.Policy_AllowJoy : IconTex.Policy_DisallowJoy, white))
                {
                    this.AllowJoy = !this.AllowJoy;
                    if (this.AllowJoy)
                    {
                        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, null);
                    }
                    else
                    {
                        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Designate_Cancel, null);
                    }
                }
            }
            TooltipHandler.TipRegion(allowJoyButton, Translator.Translate("NItxt_AllowJoyTip"));
            Rect allowOverSqueezeButton = new Rect(72f, 28f, 40f, 40f);
            if (this.targetType != RelationType.Meal)
            {
                if (Widgets.ButtonImage(allowOverSqueezeButton, this.AllowOverSqueeze ? IconTex.Policy_AllowOverSqueeze : IconTex.Policy_DisallowOverSqueeze, white))
                {
                    this.AllowOverSqueeze = !this.AllowOverSqueeze;
                    if (this.AllowOverSqueeze)
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
            TooltipHandler.TipRegion(allowOverSqueezeButton, Translator.Translate("NItxt_AllowOverSqueezeTip"));
            Rect allowDownButton = new Rect(116f, 28f, 40f, 40f);
            if (this.targetType != RelationType.Meal && this.targetType > RelationType.Colonist)
            {
                if (Widgets.ButtonImage(allowDownButton, this.AllowDown ? IconTex.Policy_AllowDown : IconTex.Policy_DisallowDown, white))
                {
                    this.AllowDown = !this.AllowDown;
                    if (this.AllowDown)
                    {
                        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, null);
                    }
                    else
                    {
                        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Designate_Cancel, null);
                    }
                }
            }
            TooltipHandler.TipRegion(allowDownButton, Translator.Translate("NItxt_AllowDownTip"));
            Rect allowKillButton = new Rect(160f, 28f, 40f, 40f);
            if (this.targetType != RelationType.Meal && this.targetType > RelationType.Colonist)
            {
                if (Widgets.ButtonImage(allowKillButton, this.AllowKill ? IconTex.Policy_AllowKill : IconTex.Policy_DisallowKill, white))
                {
                    this.AllowKill = !this.AllowKill;
                    if (this.AllowKill)
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
            TooltipHandler.TipRegion(allowKillButton, Translator.Translate("NItxt_AllowKillTip"));
            if (this.targetType == RelationType.Meal)
            {
                Func<List<FloatMenuOption>> dishOptionFinder = delegate ()
                {
                    List<FloatMenuOption> dishOptions = new List<FloatMenuOption>();
                    dishOptions.Add(new FloatMenuOption(Translator.Translate("NoneBrackets"), delegate ()
                    {
                        this.MainDishThing = null;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    using (IEnumerator<ThingDef> enumerator = CommonData.ManaSourceDefs.AsEnumerable<ThingDef>().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            ThingDef foodSource = enumerator.Current;
                            dishOptions.Add(new FloatMenuOption(foodSource.LabelCap, delegate ()
                            {
                                this.MainDishThing = foodSource;
                            }, MenuOptionPriority.Default, null, null, 29f, (Rect tmprect) => Widgets.InfoCardButton(tmprect.x + 5f, tmprect.y + (tmprect.height - 24f) / 2f, foodSource), null));
                        }
                    }
                    return dishOptions;
                };
                Rect selectDishButton = new Rect(204f, 32f, 150f, 29f);
                bool dishButtonAdded = Widgets.ButtonText(selectDishButton, (this.MainDishThing != null) ? this.MainDishThing.LabelCap : Translator.Translate("NItxt_SelectMainDish"), true, false, true);
                if (dishButtonAdded)
                {
                    Find.WindowStack.Add(new FloatMenu(dishOptionFinder()));
                }
                UIHighlighter.HighlightOpportunity(selectDishButton, "NItxt_SelectMainDish");
                TooltipHandler.TipRegion(selectDishButton, Translator.Translate("NItxt_SelectMainDishThingTip"));
            }
            if (this.targetType == RelationType.Colonist || this.targetType == RelationType.Prisoner)
            {
                Func<List<FloatMenuOption>> pawnOptionFinder = delegate ()
                {
                    List<FloatMenuOption> ediblePawnOptions = new List<FloatMenuOption>();
                    ediblePawnOptions.Add(new FloatMenuOption(Translator.Translate("NoneBrackets"), delegate ()
                    {
                        this.MainDishPawn = null;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    IEnumerable<Pawn> enumerable = from z in this.workPlanStack.compRace.parent.Map.mapPawns.AllPawnsSpawned
                                                   where z.GetComp<CompSuccubusRace>() == null && z != this.workPlanStack.compRace.parent && ((this.targetType == RelationType.Colonist && z.IsColonist) || (this.targetType == RelationType.Prisoner && z.IsPrisonerOfColony))
                                                   select z;
                    using (IEnumerator<Pawn> enumerator = enumerable.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Pawn ediblePawn = enumerator.Current;
                            ediblePawnOptions.Add(new FloatMenuOption(ediblePawn.LabelCap, delegate ()
                            {
                                this.MainDishPawn = ediblePawn;
                            }, MenuOptionPriority.Default, null, null, 29f, (Rect tmprect) => Widgets.InfoCardButton(tmprect.x + 5f, tmprect.y + (tmprect.height - 24f) / 2f, ediblePawn), null));
                        }
                    }
                    return ediblePawnOptions;
                };
                Rect selectPawnToEatButton = new Rect(204f, 32f, 150f, 29f);
                bool selectPawnButtonWasAdded = Widgets.ButtonText(selectPawnToEatButton, (this.MainDishPawn != null) ? this.MainDishPawn.LabelCap : Translator.Translate("NItxt_SelectMainDish").ToString(), true, false, true);
                if (selectPawnButtonWasAdded)
                {
                    Find.WindowStack.Add(new FloatMenu(pawnOptionFinder()));
                }
                UIHighlighter.HighlightOpportunity(selectPawnToEatButton, "NItxt_SelectMainDish");
                TooltipHandler.TipRegion(selectPawnToEatButton, Translator.Translate("NItxt_SelectMainDishTip"));
            }
            if (this.targetType != RelationType.Meal)
            {
                Rect squeezeAmountSelector = new Rect(144f, 0f, 160f, 34f);
                float squeezeValue = this.SqueezeValue;
                this.SqueezeValue = Widgets.HorizontalSlider(squeezeAmountSelector, squeezeValue, 0.4f, 2.4f, false, TranslatorFormattedStringExtensions.Translate("NItxt_SqueezeValue", squeezeValue.ToString("F1")), null, null, 0.1f);
                TooltipHandler.TipRegion(squeezeAmountSelector, Translator.Translate("NItxt_SqueezeSliderTip"));
            }
            GUI.EndGroup();
            if (this.Suspended)
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Rect suspendedWork = new Rect(workPlanInterface.x + workPlanInterface.width / 2f - 70f, workPlanInterface.y + workPlanInterface.height / 2f - 20f, 140f, 40f);
                GUI.DrawTexture(suspendedWork, TexUI.GrayTextBG);
                Widgets.Label(suspendedWork, Translator.Translate("SuspendedCaps"));
                Text.Anchor = 0;
                Text.Font = GameFont.Small;
            }
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            return workPlanInterface;
        }

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
    }
}
