using System.Collections.Generic;
using System.Text;
using Verse;

namespace IndustrialAge.Objects
{
    public class TuneDef : SoundDef
    {
        private readonly string version = "0";
        public string artist;
        public float durationTime;
        public List<ThingDef> instrumentDefs = new List<ThingDef>();
        public bool instrumentOnly;

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

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append(base.LabelCap + " - " + artist);
            return s.ToString();
        }
    }
}