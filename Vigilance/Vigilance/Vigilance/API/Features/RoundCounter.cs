﻿namespace Vigilance.API.Features
{
    public class RoundCounter
    {
        public static int RoundsToPass => ConfigManager.GetInt("rounds_to_pass");
        public static bool RestartEnabled => RoundsToPass > 0;
        private int _roundCount;
        public int RoundCount => _roundCount;

        public RoundCounter()
        { }

        public void Start()
        {
            _roundCount = 0;
        }

        public void Reset() => _roundCount = 0;

        public void AddRound() => _roundCount++;

        public void Restart()
        {
            if (!RestartEnabled)
                return;
            if (RoundsToPass >= _roundCount)
                Server.Restart(true);
            Reset();
        }
    }
}
