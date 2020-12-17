using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ARKDiscordBot
{
    class Storage
    {
        public List<Player> Players;
        public List<Team> Teams;

        public static Storage GetInstance()
        {
            if (_instance is null)
                _instance = new Storage();

            return _instance;
        }
        private static Storage _instance;

        private Storage()
        {
            Load();
        }

        public void Save()
        {
            SaveObject obj = new SaveObject();
            obj.Players = Players.ToArray();
            obj.Teams = Teams.ToArray();

            string json = JsonConvert.SerializeObject(obj);
            File.WriteAllText("save.json", json);
        }

        public void Load()
        {
            if (!File.Exists("save.json"))
            {
                Players = new List<Player>();
                Teams = new List<Team>();
            }
            else
            {
                SaveObject obj = JsonConvert.DeserializeObject<SaveObject>(File.ReadAllText("save.json"));
                Players = new List<Player>(obj.Players);
                Teams = new List<Team>(obj.Teams);
            }
        }
    }

    public class SaveObject
    {
        public Player[] Players;
        public Team[] Teams;
    }

}
