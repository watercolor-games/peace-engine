﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plex.Objects;
using System.IO;
using Newtonsoft.Json;

namespace Plex.Server
{
    public static class SessionManager
    {
        private static List<ServerAccount> getAccts()
        {
            if (!File.Exists("accts.json"))
                return new List<ServerAccount>();
            return JsonConvert.DeserializeObject<List<ServerAccount>>(File.ReadAllText("accts.json"));
        }

        private static void setSessions(List<ServerAccount> info)
        {
            File.WriteAllText("accts.json", JsonConvert.SerializeObject(info));
        }

        public static bool IsExpired(string session_id)
        {
            var acct = getAccts().FirstOrDefault(x => x.SessionID == session_id);
            if (acct == null)
                return true;
            return DateTime.Now > acct.Expiry;
        }

        [ServerMessageHandler("acct_get_key")]
        public static void AccountGetKey(string session_id, string content, string ip)
        {
            string[] split = content.Split('\t');
            if (split.Length != 2)
            {
                Program.SendMessage(new PlexServerHeader
                {
                    IPForwardedBy = ip,
                    Message = "malformed_data",
                    SessionID = session_id,
                    Content = ""
                });
                return;
            }
            string username = split[0];
            string password = split[1];
            var user = getAccts().FirstOrDefault(x => x.Username == username);
            if(user == null)
            {
                Program.SendMessage(new PlexServerHeader
                {
                    IPForwardedBy = ip,
                    Message = "session_accessdenied",
                    SessionID = session_id,
                    Content = ""
                });
                return;
            }
            var hashedpass = PasswordHasher.Hash(password, user.PasswordSalt);
            if(hashedpass != user.PasswordHash)
            {
                Program.SendMessage(new PlexServerHeader
                {
                    IPForwardedBy = ip,
                    Message = "session_accessdenied",
                    SessionID = session_id,
                    Content = ""
                });
                return;

            }
            var now = DateTime.Now;
            var expiry = user.Expiry;
            if(now > expiry)
            {
                string sessionkey = Guid.NewGuid().ToString();
                var accts = getAccts();
                accts.FirstOrDefault(x => x.Username == user.Username).Expiry = DateTime.Now;
                accts.FirstOrDefault(x => x.Username == user.Username).SessionID = sessionkey;
                setSessions(accts);
                user.SessionID = sessionkey;
            }

            Program.SendMessage(new PlexServerHeader
            {
                IPForwardedBy = ip,
                Message = "session_accessgranted",
                SessionID = session_id,
                Content = user.SessionID
            });


        }
    }
}
