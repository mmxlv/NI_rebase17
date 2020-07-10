using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
    /// <summary>
    /// Mana Policy window for succubi
    /// </summary>
    public class ITab_ManaPolicy : ITab
    {

        private float viewHeight = 1000f;

        private Vector2 scrollPosition = default(Vector2);

        private WorkPlan mouseoverPlan;

        private static readonly Vector2 WinSize = new Vector2(420f, 480f);

        [TweakValue("Interface", 0f, 128f)]
        private static float PasteX = 48f;

        [TweakValue("Interface", 0f, 128f)]
        private static float PasteY = 3f;

        [TweakValue("Interface", 0f, 32f)]
        private static float PasteSize = 24f;

        public ITab_ManaPolicy()
        {
            this.size = ITab_ManaPolicy.WinSize;
            this.labelKey = "Mana Policy";
            this.tutorTag = "Sucking Policy";
        }

        public CompSuccubusRace CompRace
        {
            get
            {
                return base.SelPawn.GetComp<CompSuccubusRace>();
            }
        }

        protected override void FillTab()
        {
            //Rect used = new Rect(ITab_ManaPolicy.WinSize.x - ITab_ManaPolicy.PasteX, ITab_ManaPolicy.PasteY, ITab_ManaPolicy.PasteSize, ITab_ManaPolicy.PasteSize);
            Rect rect2 = GenUI.ContractedBy(new Rect(0f, 0f, ITab_ManaPolicy.WinSize.x, ITab_ManaPolicy.WinSize.y), 10f);
            Func<List<FloatMenuOption>> recipeOptionsMaker = delegate()
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                bool flag = !GenCollection.Any<FloatMenuOption>(list);
                if (flag)
                {
                    list.Add(new FloatMenuOption(Translator.Translate("NoneBrackets"), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                return list;
            };
            this.mouseoverPlan = this.CompRace.policy.DoListing(rect2, recipeOptionsMaker, ref this.scrollPosition, ref this.viewHeight);
        }

        
    }
}
