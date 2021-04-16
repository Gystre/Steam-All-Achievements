using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

using SAM.Game.Stats;
using System.Diagnostics;

namespace KW_Steam_Achievements
{
    public partial class Form1 : Form
    {
        private readonly SAM.API.Client _SteamClient;
        private readonly Dictionary<uint, GameInfo> _Games;

        public Form1(SAM.API.Client client)
        {
            this._Games = new Dictionary<uint, GameInfo>();
            InitializeComponent();
            this._SteamClient = client;
        }

        private bool OwnsGame(uint id)
        {
            return this._SteamClient.SteamApps008.IsSubscribedApp(id);
        }

        private void AddGame(uint id, string type)
        {
            if (this._Games.ContainsKey(id))
            {
                return;
            }

            if (this.OwnsGame(id) == false)
            {
                return;
            }

            var info = new GameInfo(id, type);
            info.Name = this._SteamClient.SteamApps001.GetAppData(info.Id, "name");

            this._Games.Add(id, info);
        }

        private void DoDownloadList()
        {
            var pairs = new List<KeyValuePair<uint, string>>();
            byte[] bytes;
            using (var downloader = new WebClient())
            {
                bytes = downloader.DownloadData(new Uri("http://gib.me/sam/games.xml"));
            }
            using (var stream = new MemoryStream(bytes, false))
            {
                var document = new XPathDocument(stream);
                var navigator = document.CreateNavigator();
                var nodes = navigator.Select("/games/game");
                while (nodes.MoveNext())
                {
                    string type = nodes.Current.GetAttribute("type", "");
                    if (string.IsNullOrEmpty(type) == true)
                    {
                        type = "normal";
                    }
                    pairs.Add(new KeyValuePair<uint, string>((uint)nodes.Current.ValueAsLong, type));
                }
            }

            foreach (var kv in pairs)
            {
                this.AddGame(kv.Key, kv.Value);
            }
        }
       

        private void button1_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "getting the games...";

            DoDownloadList();

            this.toolStripStatusLabel1.Text = "got " + _Games.Keys.Count + " games, now starting the tasks...";

            foreach (var x in this._Games)
            {
                Process.Start("C:\\Users\\xaist\\source\\repos\\KW Steam Achievements\\HeadlessGame\\bin\\Debug\\net5.0\\HeadlessGame.exe", x.Value.Id.ToString());
                //break;
            }

            this.toolStripStatusLabel1.Text = "finished!!!!";
        }
    }
}
