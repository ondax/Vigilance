﻿namespace Vigilance.API
{
    public class Version
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public string Letter { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        
        public bool IsTesting { get; set; }
        public bool IsBeta { get; set; }

        public Version(int major, int minor, int patch, string let, bool beta = false)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Letter = let;
            if (!string.IsNullOrEmpty(let))
                IsTesting = true;
            IsBeta = beta;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Letter))
                return $"{Major}.{Minor}.{Patch}-{Letter}";
            return $"{Major}.{Minor}.{Patch}";
        }
    }
}
