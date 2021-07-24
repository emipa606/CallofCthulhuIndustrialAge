using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace IndustrialAge.Objects
{
    internal class WorldComponent_Tunes : WorldComponent
    {
        private bool AreTunesReady;
        public List<TuneDef> TuneDefCache = new List<TuneDef>();

        public WorldComponent_Tunes(World world) : base(world)
        {
        }

        public TuneDef GetCache(TuneDef tune)
        {
            TuneDef result;
            if (TuneDefCache == null)
            {
                TuneDefCache = new List<TuneDef>();
            }

            foreach (var current in TuneDefCache)
            {
                if (current != tune)
                {
                    continue;
                }

                result = current;
                return result;
            }

            TuneDefCache.Add(tune);
            result = tune;
            return result;
        }


        public void GenerateTunesList()
        {
            if (AreTunesReady)
            {
                return;
            }

            foreach (var current in DefDatabase<TuneDef>.AllDefs)
            {
                GetCache(current);
            }

            AreTunesReady = true;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            GenerateTunesList();
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref TuneDefCache, "TuneDefCache", LookMode.Def);
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                GenerateTunesList();
            }
        }
    }
}