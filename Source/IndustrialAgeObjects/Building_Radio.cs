using Verse;

namespace IndustrialAge.Objects;

public class Building_Radio : Building_Gramophone
{
    public override void SpawnSetup(Map map, bool bla)
    {
        base.SpawnSetup(map, bla);
        isRadio = true;
    }
}