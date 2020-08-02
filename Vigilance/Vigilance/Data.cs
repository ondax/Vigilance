using System;
using System.Collections.Generic;
using Vigilance.API.Features;

namespace Vigilance
{
    public static class Data
    {
        private static List<string> _playerLock;
        private static List<string> _instantKill;
        private static List<string> _breakDoors;
        private static Random _random;
        private static bool _warheadLock;
        private static RoundCounter _roundCounter;
        private static Cleanup _cleanup;
        private static RemoteCard _remoteCard;
        private static ScpHealing _scpHealing;
        private static NicknameFilter _nicknameFilter;
        private static Patcher _patcher;
        private static Sitrep _sitrep;
            
        public static List<string> PlayerLock => _playerLock;
        public static List<string> InstantKill => _instantKill;
        public static List<string> BreakDoors => _breakDoors;
        public static Random Random => _random;
        public static RoundCounter RoundCounter => _roundCounter;
        public static Cleanup Cleanup => _cleanup;
        public static RemoteCard RemoteCard => _remoteCard;
        public static ScpHealing ScpHealing => _scpHealing;
        public static NicknameFilter NicknameFilter => _nicknameFilter;
        public static Patcher Patcher => _patcher;
        public static Sitrep Sitrep => _sitrep;

        public static void Prepare()
        {
            _playerLock = new List<string>();
            _instantKill = new List<string>();
            _breakDoors = new List<string>();
            _random = new Random();
            _warheadLock = false;
            _roundCounter = new RoundCounter();
            _roundCounter?.Start();
            _cleanup = new Cleanup();
            _remoteCard = new RemoteCard();
            _scpHealing = new ScpHealing();
            _scpHealing?.Start();
            _nicknameFilter = new NicknameFilter();
            _patcher = new Patcher();
            _patcher?.Start();
            _sitrep = new Sitrep();
            _sitrep?.Start();
            Reset();
        }

        public static void StartCleanup() => _cleanup?.Start();

        public static void Reset()
        {
            _playerLock.Clear();
            _instantKill.Clear();
            _breakDoors.Clear();
            _random = new Random();
            _warheadLock = false;
        }
    }
}
