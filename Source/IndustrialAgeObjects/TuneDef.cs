using System.Collections.Generic;
using System.Text;
using Verse;

namespace IndustrialAge.Objects;

public class TuneDef : SoundDef
{
    public readonly List<ThingDef> instrumentDefs = [];
    private readonly string version = "0";
    public string artist;
    public float durationTime;
    public bool instrumentOnly;

    public int Version => int.TryParse(version, out var x) ? x : 0;

    public override string ToString()
    {
        var s = new StringBuilder();
        s.Append(base.LabelCap + " - " + artist);
        return s.ToString();
    }
}