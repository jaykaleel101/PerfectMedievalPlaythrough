using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace PMP
{
    public class Apiary : Building
    {
        int ApiaryProgress;
        int duration = 420000; // 7 days
        public int tickBeforeTend = 120000; // 2days
        float progressPercentage;
        private int flowerCount;
        private List<IntVec3> cellsAround = new List<IntVec3>();

        public bool HoneyReady
        {
            get
            {
                return ApiaryProgress >= duration;
            }
        }

        public bool needTend
        {
            get
            {
                return tickBeforeTend <= 0;
            }
        }

        private int EstimatedTicksLeft
        {
            get
            {
                return this.duration - this.ApiaryProgress;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ApiaryProgress, "ApiaryProgress", 0, false);
            Scribe_Values.Look<int>(ref this.tickBeforeTend, "tickBeforeTend", 0, false);
            Scribe_Values.Look<int>(ref this.flowerCount, "flowerCount", 0, false);
        }

        public override void TickRare()
        {
            base.TickRare();
            progressPercentage = (float)this.ApiaryProgress / this.duration;
            countFlowers();
            
            if (!(this.AmbientTemperature < 10) && !(this.Map.weatherManager.RainRate>0) && flowerCount > 0)
            {
                this.tickBeforeTend -= 250;
                if (!this.needTend)
                {
                    this.ApiaryProgress += 250;
                }
                else
                {
                    this.ApiaryProgress += 125;
                }
            }
        }

        private void Reset()
        {
            this.ApiaryProgress = 0;
        }

        public void ResetTend(Pawn pawn)
        {
            int skill = pawn.skills.skills.Find((SkillRecord r) => r.def.defName == "Animals").levelInt;
            System.Random rnd = new System.Random();
            if(rnd.Next(0, 21 - skill) == 1)
            {
                this.tickBeforeTend = 120000;
            }
            else
            {
                pawn.health.AddHediff(PMP_DefOf.PMP_Sting);
                pawn.needs.mood.thoughts.memories.TryGainMemoryFast(PMP_DefOf.PMP_StingMoodDebuff);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "Tending failed", 5f);
            }
            pawn.skills.skills.Find((SkillRecord r) => r.def.defName == "Animals").Learn(100, false);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }

            if (this.HoneyReady)
            {
                stringBuilder.AppendLine("AReady".Translate());
            }
            else
            {
                if (flowerCount == 0)
                {
                    stringBuilder.AppendLine("ANeedFlower");
                }
                else
                {
                    if (this.AmbientTemperature < 10)
                    {
                        stringBuilder.AppendLine("AResting".Translate());
                    }
                    else
                    {
                        if (this.Map.weatherManager.RainRate>0)
                        {
                            stringBuilder.AppendLine("ARestingRain".Translate());
                        } else {
                            stringBuilder.AppendLine("FermentationProgress".Translate(this.progressPercentage.ToStringPercent(), this.EstimatedTicksLeft.ToStringTicksToPeriod()));
                            if (this.needTend)
                            {
                                stringBuilder.AppendLine("ANeedTend".Translate());
                            }
                            else
                            {
                                stringBuilder.AppendLine("ANeedTIn".Translate(this.tickBeforeTend.ToStringTicksToPeriod()));
                            }
                        }
                    }
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public Thing TakeOutHoney()
        {
            if (!this.HoneyReady)
            {
                Log.Warning("Tried to get honey but it's not yet ready.");
                return null;
            }
            Thing thing = ThingMaker.MakeThing(PMP_DefOf.DankPyon_Honeycomb, null);
            thing.stackCount = flowerCount;
            this.Reset();
            return thing;
        }

        public void countFlowers()
        {
            flowerCount = 0;
            cellsAround = CellsAroundA(this.TrueCenter().ToIntVec3(), this.Map);
            foreach (IntVec3 cell in cellsAround)
            {
                foreach (Thing item in cell.GetThingList(this.Map))
                {
                    if(item.def.plant != null && (
                    item.def.plant.purpose == PlantPurpose.Beauty
                    || item.def.defName== "DankPyon_Tree_Apple"
                    || item.def.defName== "DankPyon_Tree_Lemon"
                    || item.def.defName== "DankPyon_Tree_Mulberry"
                    || item.def.defName== "DankPyon_Tree_GriffonBerry"
                    || item.def.defName== "Plant_Berry"
                    || item.def.defName== "Plant_Brambles"
                    || item.def.defName== "Plant_Daylily"
                    || item.def.defName== "Plant_AYLavender"
                    || item.def.defName== "Plant_AYStJohnsWort"
                    || item.def.defName== "Plant_AYWhiteWillow"
                    ))
                    {
                        flowerCount++;
                    }
                }
            }
            if (flowerCount > 20)
            {
                flowerCount = 20;
            }
        }

        public List<IntVec3> CellsAroundA(IntVec3 pos, Map map)
        {
            List<IntVec3> cellsAround = new List<IntVec3>();
            if (!pos.InBounds(map))
            {
                return cellsAround;
            }
            IEnumerable<IntVec3> cells = CellRect.CenteredOn(this.Position, 10).Cells;
            foreach (IntVec3 item in cells)
            {
                if (item.InHorDistOf(pos, 9.9f))
                {
                    cellsAround.Add(item);
                }
            }
            return cellsAround;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to max",
                    action = delegate ()
                    {
                        this.ApiaryProgress = this.duration;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Add progress",
                    action = delegate ()
                    {
                        this.ApiaryProgress += 12000;
                        this.tickBeforeTend -= 12000;
                    }
                };
            }
            yield break;
        }
    }
}
