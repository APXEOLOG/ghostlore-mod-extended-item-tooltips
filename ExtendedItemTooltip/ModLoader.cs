using HarmonyLib;

namespace ExtendedItemTooltip
{
    public class ModLoader : IModLoader
    {
        public void OnCreated()
        {
            var harmony = new Harmony("com.apxeolog.extended-item-tooltip");
            harmony.PatchAll();
        }

        public void OnReleased()
        {
        }

        public void OnGameLoaded(LoadMode mode)
        {
        }

        public void OnGameUnloaded()
        {
        }
    }
}