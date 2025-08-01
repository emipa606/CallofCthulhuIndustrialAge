using System.Linq;
using RimWorld.Planet;
using Verse;

namespace IndustrialAge.Objects;

internal class WorldComponent_ArkhamEstate(World world) : WorldComponent(world)
{
    private bool AreRecipesReady;
    private bool CheckedForRecipes;

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();

        //Log.Message("UtilityWorldObject Arkham Estate Started");
        if (CheckedForRecipes)
        {
            return;
        }

        GenerateStrangeMeatRecipe();
        CheckedForRecipes = true;
    }

    private void GenerateStrangeMeatRecipe()
    {
        if (!LoadedModManager.RunningMods.Any(x => x.Name.Contains("Cosmic Horrors")) || AreRecipesReady)
        {
            return;
        }

        //Not really, but hey, let's get started.
        AreRecipesReady = true;

        //We want to use strange meat to make wax.
        var recipeMakeWax = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(d => d.defName == "Jecrell_MakeWax");
        if (recipeMakeWax != null)
        {
            var newFilter = new ThingFilter();
            newFilter.CopyAllowancesFrom(recipeMakeWax.fixedIngredientFilter);
            newFilter.SetAllow(ThingCategoryDef.Named("ROM_StrangeMeatRaw"), true);
            recipeMakeWax.fixedIngredientFilter = newFilter;

            var newFilter2 = new ThingFilter();
            newFilter2.CopyAllowancesFrom(recipeMakeWax.defaultIngredientFilter);
            newFilter2.SetAllow(ThingCategoryDef.Named("ROM_StrangeMeatRaw"), true);
            recipeMakeWax.defaultIngredientFilter = newFilter;

            foreach (var temp in recipeMakeWax.ingredients)
            {
                if (temp.filter == null)
                {
                    continue;
                }

                var newFilter3 = new ThingFilter();
                newFilter3.CopyAllowancesFrom(temp.filter);
                newFilter3.SetAllow(ThingCategoryDef.Named("ROM_StrangeMeatRaw"), true);
                temp.filter = newFilter3;
                Log.Message("Added new filter");
            }

            Log.Message("Strange meat added to wax recipes.");
        }

        //I want stoves to be able to cook strange meals too.
        var stoveDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(def => def.defName == "WoodStoveFurnace");
        if (stoveDef == null)
        {
            return;
        }

        if (stoveDef.recipes.FirstOrDefault(def => def.defName == "ROM_CookStrangeMealSimple") == null)
        {
            stoveDef.recipes.Add(DefDatabase<RecipeDef>.GetNamed("ROM_CookStrangeMealSimple"));
        }

        if (stoveDef.recipes.FirstOrDefault(def => def.defName == "ROM_CookStrangeMealFine") == null)
        {
            stoveDef.recipes.Add(DefDatabase<RecipeDef>.GetNamed("ROM_CookStrangeMealFine"));
        }

        if (stoveDef.recipes.FirstOrDefault(def => def.defName == "ROM_CookStrangeMealLavish") == null)
        {
            stoveDef.recipes.Add(DefDatabase<RecipeDef>.GetNamed("ROM_CookStrangeMealLavish"));
        }

        Log.Message("Strange meal recipes added to WoodStoveFurnace defs");
    }

    public override void ExposeData()
    {
        //Scribe_Collections.Look<CosmicEntity>(ref this.DeityCache, "Deities", LookMode.Deep, new object[0]);
        //Scribe_Values.Look<bool>(ref this.AreRecipesReady, "AreRecipesReady", false, false);
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            GenerateStrangeMeatRecipe();
        }
    }
}