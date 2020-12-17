using CoreRCON;
using CoreRCON.PacketFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ARKDiscordBot
{
    public class StatusClass
    {
        private RCON _RCON;
        internal bool Connected;
        internal string MapName;
        internal string Name => _name;

        string _ip;
        string _port;
        string _rconpassword;
        string _name;

        RCONManager owner;
        Thread t;
        Thread chatThread;

        public StatusClass(string name, string ip, string port, string rconpassword, string mapname, RCONManager manager)
        {
            MapName = mapname;
            owner = manager;

            _ip = ip;
            _port = port;
            _rconpassword = rconpassword;
            _name = name;

            t = new Thread(new ThreadStart(Start));
            t.Start();
        }

        internal void Reconnect()
        {
            if (t.IsAlive)
                return;

            t = new Thread(new ThreadStart(Start));
            t.Start();
        }

        void Start()
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    var endpoint = new IPEndPoint(IPAddress.Parse(_ip), int.Parse(_port));
                    _RCON = new RCON(endpoint, _rconpassword, 0);
                    await _RCON.ConnectAsync();

                    Connected = true;

                    _RCON.OnDisconnected += Rcon_OnDisconnected;
                    this.chatThread = new Thread(async delegate ()
                    {
                        Thread.Sleep(5000);
                        for (; ; )
                        {
                            try
                            {
                                string r = await _RCON.SendCommandAsync("getchat").ConfigureAwait(false);
                                string[] s = r.Split("\r\n");

                                foreach (string str in s)
                                {
                                    if (str.Contains("Server received, But no response!!")) { }
                                    else
                                    {
                                        if (str.Contains("was killed by"))
                                        {
                                            //Tribemember Human - Lvl 36 was killed by a Titanomyrma Soldier - Lvl 140!
                                            var regex = Regex.Match(r, "ID [0-9]{1,}:");

                                            if (!regex.Success) continue;
                                            string chk = str;

                                            string pt1 = "", pt2 = "";

                                            //Tribe Tribe of Human, ID 1837849902: Day 23, 11:39:57: <RichColor Color="1, 0, 0, 1">Your Raptor - Lvl 52 (Raptor) was killed by Brontosaurus - Lvl 76 (Brontosaurus) (dannyisgay)!</>)
                                            int indexOfTribeMember = chk.IndexOf("Tribemember");
                                            if (indexOfTribeMember > 0)
                                            {
                                                chk = chk.Substring(chk.IndexOf("Tribemember"));
                                                chk = chk.Substring(0, chk.IndexOf("</>"));
                                            }
                                            else
                                            {
                                                pt1 = chk.Substring(0, chk.IndexOf(","));
                                                pt2 = chk.Substring(chk.IndexOf("Your"));
                                                pt2 = pt2.Substring(0, pt2.IndexOf("</>"));
                                            }

                                            string[] split = chk.Split(" ");

                                            string playerName = "";
                                            int offset = 0;
                                            for (int i = 1; i < split.Length; i++)
                                            {
                                                if (split[i] == "-") break;
                                                playerName += split[i];
                                                offset++;
                                            }
                                            playerName = playerName.Trim();

                                            Player plr = owner.GetPlayer(playerName);
                                            if (plr != null)
                                            {
                                                plr.DeathCount++;

                                                string killerName = "";
                                                for (int i = 7 + offset; i < split.Length; i++)
                                                {
                                                    if (split[i] == "-") break;
                                                    killerName += split[i];
                                                    offset++;
                                                }

                                                Player klr = owner.GetPlayer(killerName);

                                                List<Team> teams = owner.GetTeams();

                                                bool pKill = false;
                                                foreach (var team in teams)
                                                {
                                                    string pattern = @$"- Lvl [0-9]{1,3} was killed by {killerName} \({team.TribeName}\)";

                                                    var regex2 = Regex.Match(chk, pattern);
                                                    pKill = regex2.Success;
                                                }

                                                if (klr != null && pKill)
                                                {
                                                    await owner.SendKillFeed(plr, klr);

                                                    Team t = owner.GetTeam(klr);
                                                    if (t != null)
                                                    {
                                                        t.MinutesRemaining += 60;
                                                    }
                                                }
                                                else
                                                // Check if Tribe Owned Dino killed player.
                                                {
                                                    teams.ForEach(async x =>
                                                    {
                                                        if (chk.Contains($"({x.TribeName}"))
                                                        {
                                                            await owner.SendKillFeed(chk, false).ConfigureAwait(false);
                                                            x.MinutesRemaining += 60;
                                                        }
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                List<Team> teams = owner.GetTeams();
                                                teams.ForEach(async x =>
                                                {
                                                    if (pt2.Contains($"({x.TribeName}"))
                                                    {
                                                        await owner.SendKillFeed(pt1 + " - " + pt2, true).ConfigureAwait(false);
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                owner._log.Info(ex.Message);
                                break;
                            }
                            Thread.Sleep(5000);
                        }
                    });
                    chatThread.Start();

                    await Task.Delay(-1).ConfigureAwait(false);

                }
                catch { }
            });

            task.GetAwaiter().GetResult();

            Connected = false;
        }

        internal async Task KickAllTeamPlayers(Team team)
        {
            string result = await _RCON.SendCommandAsync("listplayers");

            Storage.GetInstance().Players.Where(x => x.TeamId == team.Id).Select(x => x.SteamID)
                .ToList().ForEach(async x =>
                { 
                    if (result.Contains(x))
                        await _RCON.SendCommandAsync("KickPlayer " + x).ConfigureAwait(false);
                }
                );
        }

        internal void WhiteListTeam(Team team)
        {
            Storage.GetInstance().Players.Where(x => x.TeamId == team.Id)
                .ToList().ForEach(async x => await _RCON.SendCommandAsync("AllowPlayerToJoinNoCheck " + x.SteamID).ConfigureAwait(false));
        }

        internal void RemoveTeamFromWhiteList(Team team)
        {
            Storage.GetInstance().Players.Where(x => x.TeamId == team.Id)
                .ToList().ForEach(async x => await _RCON.SendCommandAsync("DisallowPlayerToJoinNoCheck " + x.SteamID).ConfigureAwait(false));
        }

        internal async Task SendSaveAndShutdownCommand()
        {
            await _RCON.SendCommandAsync("DoExit").ConfigureAwait(false);
        }

        private void Rcon_OnDisconnected()
        {
            Connected = false;
        }
    }
}
