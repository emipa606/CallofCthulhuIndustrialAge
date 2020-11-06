using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;

namespace IndustrialAge.Objects
{
    public class TuneDef : SoundDef
    {
        private readonly string version = "0";
        public string artist;
        public float durationTime;
        public bool instrumentOnly;
        public List<ThingDef> instrumentDefs = new List<ThingDef>();

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append(base.LabelCap + " - " + artist);
            return s.ToString();
        }

        public int Version
        {
            get
            {
                if (int.TryParse(version, out var x))
                {
                    return x;
                }
                return 0;
            }
        }
    }
}
