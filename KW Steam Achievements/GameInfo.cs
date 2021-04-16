using System.Globalization;

namespace KW_Steam_Achievements
{
    internal class GameInfo
    {
        private string _Name;

        public uint Id;
        public string Type;
        public int ImageIndex;

        public string Name
        {
            get { return this._Name; }
            set { this._Name = value ?? "App " + this.Id.ToString(CultureInfo.InvariantCulture); }
        }

        public string Logo;

        public GameInfo(uint id, string type)
        {
            this.Id = id;
            this.Type = type;
            this.Name = null;
            this.ImageIndex = 0;
            this.Logo = null;
        }
    }
}
