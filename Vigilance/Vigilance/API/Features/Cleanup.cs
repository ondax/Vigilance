using MEC;
using System.Collections.Generic;

namespace Vigilance.API.Features
{
    public class Cleanup
    {
        public int Time => ConfigManager.GetInt("item_cleanup");
        public bool Enabled => Time > 0;
        public bool NukeCleanup => ConfigManager.GetBool("nuke_cleanup");
        public List<ItemType> IgnoredItems => ConfigManager.GetItems("cleanup_ignored_items");

        public Cleanup()
        { }

        public void Start()
        {
            if (!Enabled)
                return;
            Timing.RunCoroutine(CleanUp());
        }

        public void ClearAllItems()
        {
            foreach (Pickup pickup in Map.Pickups)
            {
                pickup.Delete();
            }
        }

        public void ClearItems()
        {
            foreach (Pickup pickup in Map.Pickups)
            {
                if (!IgnoredItems.Contains(pickup.ItemId))
                {
                    pickup.Delete();
                }
            }
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
