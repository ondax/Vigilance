namespace Vigilance.API
{
    public class Version
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int API { get; set; }
        public string Letter { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        
        public bool IsTesting { get; set; }
        public bool IsBeta { get; set; }

        public Version(int major, int minor, int api, string let, bool beta = false)
        {
            Major = major;
            Minor = minor;
            API = api;
            Letter = let;
            if (!string.IsNullOrEmpty(let))
                IsTesting = true;
            IsBeta = beta;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Letter))
                return $"{Major}.{Minor}.{API}-{Letter}";
            return $"{Major}.{Minor}.{API}";
        }

        public override int GetHashCode()
        {
            return Major + Minor + API;
        }

        public override bool Equals(object obj)
        {
            Version v = (Version)obj;
            if (v == null)
                return false;
            if (v.Major == Major && v.Minor == Minor && v.API == API && v.Letter == Letter && v.IsTesting == IsTesting && v.IsBeta == IsBeta && v.FullName == FullName)
                return true;
            return false;
        }
    }
}
