namespace Vigilance.API.Features
{
    public class RoundCounter
    {
        public int RoundsToPass => ConfigManager.GetInt("rounds_to_pass");
        public bool RestartEnabled => RoundsToPass > 0;
        private int _roundCount;
        public int RoundCount => _roundCount;

        public RoundCounter()
        { }

        public void Start()
        {
            _roundCount = 0;
        }

        public void Reset() => _roundCount = 0;

        public void AddRound()
        {
            _roundCount++;
            Log.Debug("RoundCounter", $"Rounds amount: {_roundCount}");
        }

        public void Restart()
        {
            if (!RestartEnabled)
                return;
            if (RoundsToPass >= _roundCount)
            {
                Log.Info("RoundCounter", $"{RoundsToPass} rounds have passed, restarting the server ..");
                Server.Restart(true);
                Reset();
            }
        }
    }
}
