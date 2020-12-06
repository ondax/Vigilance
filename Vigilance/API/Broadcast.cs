namespace Vigilance.API
{
    public class Broadcast
    {
        private static global::Broadcast _bc = null;
        public int Duration { get; set; }
        public string Message { get; set; }
        public bool Monospaced { get; set; }

        public Broadcast(int duration, string message, bool monoSpaced = false)
        {
            Duration = duration;
            Message = message;
            Monospaced = monoSpaced;
        }

        public void Show() => Map.Broadcast(this);
        public void Show(Player player) => player?.Broadcast(this);

        public static global::Broadcast LocalBroadcast
        {
            get
            {
                if (_bc == null)
                    _bc = PlayerManager.localPlayer.GetComponent<global::Broadcast>();
                return _bc;
            }
        }
    }
}
