namespace Vigilance.API
{
    public class Broadcast
    {
        public int Duration { get; set; }
        public string Message { get; set; }
        public bool Monospaced { get; set; }

        public Broadcast(int duration, string message, bool monoSpaced = false)
        {
            Duration = duration;
            Message = message;
            Monospaced = monoSpaced;
        }
    }
}
