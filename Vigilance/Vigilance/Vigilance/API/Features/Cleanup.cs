using MEC;
using System.Collections.Generic;

namespace Vigilance.API.Features
{
    public class Cleanup
    {
        public static int Time => ConfigManager.GetInt("item_cleanup");
        public static bool Enabled => Time > 0;
        public static bool NukeCleanup => ConfigManager.GetBool("nuke_cleanup");
        public static List<ItemType> IgnoredItems => ConfigManager.GetItems("cleanup_ignored_items");

        public Cleanup()
        { }

        public void Start()
        {
            if (!Enabled)
                return;
            Timing.RunCoroutine(CleanUp());
        }

        private IEnumerator<float> CleanUp()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(Time);
                foreach (Pickup pickup in UnityEngine.Object.FindObjectsOfType<Pickup>())
                {
                    if (!IgnoredItems.Contains(pickup.ItemId))
                    {
                        pickup.Delete();
                    }
                }
            }
        }
    }
}
