using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NightmareIncarnation
{
    public class PlantGrowthStageExtension : DefModExtension
    {
        public List<GrowthStageGraphic> stages = new List<GrowthStageGraphic>();
    }

    public class GrowthStageGraphic
    {
        public float minGrowth = 0f;
        public string texPath;
    }

    public class Plant_GrowthStages : Plant
    {
        private Graphic cachedGraphic;
        private int cachedStageIndex = -1;

        public override Graphic Graphic
        {
            get
            {
                int stageIndex = CurrentStageIndex();

                if (cachedGraphic == null || stageIndex != cachedStageIndex)
                {
                    cachedStageIndex = stageIndex;
                    cachedGraphic = BuildGraphicForStage(stageIndex);
                }

                return cachedGraphic;
            }
        }

        public override void Print(SectionLayer layer)
        {
            Graphic graphic = Graphic;
            if (graphic != null)
            {
                graphic.Print(layer, this, 0f);
            }
            else
            {
                base.Print(layer);
            }
        }

        protected override void Tick()
        {
            base.Tick();

            int stageIndex = CurrentStageIndex();
            if (stageIndex != cachedStageIndex)
            {
                cachedStageIndex = stageIndex;
                cachedGraphic = BuildGraphicForStage(stageIndex);

                if (Spawned && Map != null)
                {
                    Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
                }
            }
        }

        private int CurrentStageIndex()
        {
            PlantGrowthStageExtension extension = def.GetModExtension<PlantGrowthStageExtension>();
            if (extension == null || extension.stages.NullOrEmpty())
                return 0;

            float progress = GrowthProgress();

            int index = 0;
            for (int i = 0; i < extension.stages.Count; i++)
            {
                if (progress >= extension.stages[i].minGrowth)
                    index = i;
            }

            return index;
        }

        private float GrowthProgress()
        {
            return Mathf.Clamp01(Growth);
        }

        private Graphic BuildGraphicForStage(int stageIndex)
        {
            PlantGrowthStageExtension extension = def.GetModExtension<PlantGrowthStageExtension>();
            if (extension == null || extension.stages.NullOrEmpty() || stageIndex >= extension.stages.Count)
                return base.Graphic;

            string texPath = extension.stages[stageIndex].texPath;

            GraphicData tempData = new GraphicData();
            tempData.CopyFrom(def.graphicData);
            tempData.texPath = texPath;

            return tempData.Graphic;
        }
    }
}