using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using WhatsAppApi;
using WhatsAppApi.Account;
using WhatsAppApi.Helper;
using WhatsAppApi.Response;
using log4net;

namespace WhatsAppApi.Facades
{
    public class WhatsAppConnector
    {
        private WhatsApp wa;
        private string username = "5491162921031"; // Mobile number with country code (but without + or 00)
        private string nickname;
        private string password;
        private Thread thRecv;
        private WhatsUserManager usrMan;
        private Dictionary<string, WhatsUser> users = new Dictionary<string, WhatsUser>();
        private bool connected = false;
        private static readonly ILog log = LogManager.GetLogger(typeof(WhatsAppConnector));

        public event EventHandler<MediaEventArgs> Media;

        protected virtual void OnMedia(MediaEventArgs args)
        {
            if (this.Media != null)
            {
                var deleg = this.Media;
                deleg(this, args);
            }
        }

        public string Username { get { return this.username; } set { this.username = value; } }

        public WhatsAppConnector(string username, string password, string nickname)
        {
            var tmpEncoding = Encoding.UTF8;
            System.Console.OutputEncoding = Encoding.Default;
            System.Console.InputEncoding = Encoding.Default;
            this.username = username;
            this.nickname = nickname; // "WhatsApiNet";
            this.password = password; // "st+pHGejcBmq81rbJsWfhL8w+dQ=";//v2 password
            wa = new WhatsApp(username, password, nickname, true);

            //event bindings
            wa.OnLoginSuccess += wa_OnLoginSuccess;
            wa.OnLoginFailed += wa_OnLoginFailed;
            wa.OnGetMessage += wa_OnGetMessage;
            wa.OnGetMessageReceivedClient += wa_OnGetMessageReceivedClient;
            wa.OnGetMessageReceivedServer += wa_OnGetMessageReceivedServer;
            wa.OnNotificationPicture += wa_OnNotificationPicture;
            wa.OnGetPresence += wa_OnGetPresence;
            wa.OnGetGroupParticipants += wa_OnGetGroupParticipants;
            wa.OnGetLastSeen += wa_OnGetLastSeen;
            wa.OnGetTyping += wa_OnGetTyping;
            wa.OnGetPaused += wa_OnGetPaused;
            wa.OnGetMessageImage += wa_OnGetMessageImage;
            wa.OnGetMessageAudio += wa_OnGetMessageAudio;
            wa.OnGetMessageVideo += wa_OnGetMessageVideo;
            wa.OnGetMessageLocation += wa_OnGetMessageLocation;
            wa.OnGetMessageVcard += wa_OnGetMessageVcard;
            wa.OnGetPhoto += wa_OnGetPhoto;
            wa.OnGetPhotoPreview += wa_OnGetPhotoPreview;
            wa.OnGetGroups += wa_OnGetGroups;
            wa.OnGetSyncResult += wa_OnGetSyncResult;
            wa.OnGetStatus += wa_OnGetStatus;
            wa.OnGetPrivacySettings += wa_OnGetPrivacySettings;
            wa.OnDisconnect += wa_OnDisconnect;
            DebugAdapter.Instance.OnPrintDebug += Instance_OnPrintDebug;
        }
        public void Connect()
        {
            connected = true;
            wa.Connect();

            string datFile = getDatFileName(username);
            byte[] nextChallenge = null;
            if (File.Exists(datFile))
            {
                try
                {
                    string foo = File.ReadAllText(datFile);
                    nextChallenge = Convert.FromBase64String(foo);
                }
                catch (Exception) { };
            }

            wa.Login(nextChallenge);

            thRecv = new Thread(t =>
            {
                try
                {
                    while (wa != null)
                    {
                        wa.PollMessages();
                        Thread.Sleep(100);
                        continue;
                    }

                }
                catch (ThreadAbortException)
                {
                }
            }) { IsBackground = true };
            thRecv.Start();
            usrMan = new WhatsUserManager();
        }

        void wa_OnDisconnect(Exception ex)
        {
            if (connected)
            {
                //Reconnecting...
                log.Info("WhatsAppConnector: Reconnecting...");
                Connect();
            }
        }
        public void Disconnect()
        {
            connected = false;
            wa.Disconnect();
            wa = null;
            if (!thRecv.Join(500))
            {
                thRecv.Abort();
            }

        }

        #region static handlers

        static void Instance_OnPrintDebug(object value)
        {
            //Console.WriteLine(value);
        }

        void wa_OnGetPrivacySettings(Dictionary<ApiBase.VisibilityCategory, ApiBase.VisibilitySetting> settings)
        {
            throw new NotImplementedException();
        }

        void wa_OnGetStatus(string from, string type, string name, string status)
        {
            //Console.WriteLine(String.Format("Got status from {0}: {1}", from, status));
        }

        static string getDatFileName(string pn)
        {
            string filename = string.Format("{0}.next.dat", pn);
            return Path.Combine(Directory.GetCurrentDirectory(), filename);
        }

        void wa_OnGetSyncResult(int index, string sid, Dictionary<string, string> existingUsers, string[] failedNumbers)
        {
            //Console.WriteLine("Sync result for {0}:", sid);
            foreach (KeyValuePair<string, string> item in existingUsers)
            {
                //Console.WriteLine("Existing: {0} (username {1})", item.Key, item.Value);
            }
            foreach (string item in failedNumbers)
            {
                //Console.WriteLine("Non-Existing: {0}", item);
            }
        }

        void wa_OnGetGroups(WaGroupInfo[] groups)
        {
            //Console.WriteLine("Got groups:");
            foreach (WaGroupInfo info in groups)
            {
                //Console.WriteLine("\t{0} {1}", info.subject, info.id);
            }
        }

        void wa_OnGetPhotoPreview(string from, string id, byte[] data)
        {
            //Console.WriteLine("Got preview photo for {0}", from);
            File.WriteAllBytes(string.Format("preview_{0}.jpg", from), data);
        }

        void wa_OnGetPhoto(string from, string id, byte[] data)
        {
            //Console.WriteLine("Got full photo for {0}", from);
            File.WriteAllBytes(string.Format("{0}.jpg", from), data);
        }

        void wa_OnGetMessageVcard(ProtocolTreeNode vcardNode, string from, string id, string name, byte[] data)
        {
            //Console.WriteLine("Got vcard \"{0}\" from {1}", name, from);
            File.WriteAllBytes(string.Format("{0}.vcf", name), data);
        }

        void wa_OnGetMessageLocation(ProtocolTreeNode locationNode, string from, string id, double lon, double lat, string url, string name, byte[] preview)
        {
            //Console.WriteLine("Got location from {0} ({1}, {2})", from, lat, lon);
            if (!string.IsNullOrEmpty(name))
            {
                //Console.WriteLine("\t{0}", name);
            }
            File.WriteAllBytes(string.Format("{0}{1}.jpg", lat, lon), preview);
        }

        void wa_OnGetMessageVideo(ProtocolTreeNode mediaNode, string from, string id, string fileName, int fileSize, string url, byte[] preview)
        {
            //Console.WriteLine("Got video from {0}", from, fileName);
            OnGetMedia(fileName, url, preview);
        }

        byte[] OnGetMedia(string file, string url, byte[] data)
        {
            //save preview
            File.WriteAllBytes(string.Format("preview_{0}.jpg", file), data);

            byte[] bytes = null;
            //download
            using (WebClient wc = new WebClient())
            {
                wc.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                wc.DownloadFile(new Uri(url), string.Format("full_{0}.jpg", file));
                bytes = wc.DownloadData(new Uri(url));
            }
            return bytes;
        }

        void wa_OnGetMessageAudio(ProtocolTreeNode mediaNode, string from, string id, string fileName, int fileSize, string url, byte[] preview)
        {
            //Console.WriteLine("Got audio from {0}", from, fileName);
            OnGetMedia(fileName, url, preview);
        }

        void wa_OnGetMessageImage(ProtocolTreeNode mediaNode, string from, string id, string fileName, int size, string url, byte[] preview)
        {
            //Console.WriteLine("Got image from {0}", from, fileName);
            byte[] bytes = OnGetMedia(fileName, url, preview);
            OnMedia(new MediaEventArgs() { FileName = fileName, Bytes = bytes, From = from, Id = id, Preview = preview, Size = size, Url = url });
        }

        void wa_OnGetPaused(string from)
        {
            //Console.WriteLine("{0} stopped typing", from);
        }

        void wa_OnGetTyping(string from)
        {
            //Console.WriteLine("{0} is typing...", from);
        }

        void wa_OnGetLastSeen(string from, DateTime lastSeen)
        {
            //Console.WriteLine("{0} last seen on {1}", from, lastSeen.ToString());
        }

        void wa_OnGetMessageReceivedServer(string from, string participant, string id)
        {
            //Console.WriteLine("Message {0} to {1} received by server", id, from);
        }

        void wa_OnGetMessageReceivedClient(string from, string participant, string id)
        {
            //Console.WriteLine("Message {0} to {1} received by client", id, from);
        }

        void wa_OnGetGroupParticipants(string gjid, string[] jids)
        {
            //Console.WriteLine("Got participants from {0}:", gjid);
            foreach (string jid in jids)
            {
                //Console.WriteLine("\t{0}", jid);
            }
        }

        void wa_OnGetPresence(string from, string type)
        {
            //Console.WriteLine("Presence from {0}: {1}", from, type);
        }

        void wa_OnNotificationPicture(string type, string jid, string id)
        {
            //TODO
            //throw new NotImplementedException();
        }

        void wa_OnGetMessage(ProtocolTreeNode node, string from, string id, string name, string message, bool receipt_sent)
        {
            //Console.WriteLine("Message from {0} {1}: {2}", name, from, message);
            OnMessageReceived(new MessageEventArgs(from, name, message));
        }

        private void wa_OnLoginFailed(string data)
        {
            //Console.WriteLine("Login failed. Reason: {0}", data);
        }

        private void wa_OnLoginSuccess(string phoneNumber, byte[] data)
        {
            //Console.WriteLine("Login success. Next password:");
            string sdata = Convert.ToBase64String(data);
            //Console.WriteLine(sdata);
            try
            {
                File.WriteAllText(getDatFileName(phoneNumber), sdata);
            }
            catch (Exception) { }
        }

        # endregion
        public void SendMessage(string destination, string message)
        {
            WhatsUser tmpUser = GetWhatsUser(destination);
            //Console.WriteLine("[] Send message to {0}: {1}", tmpUser, message);
            wa.SendMessage(tmpUser.GetFullJid(), message);
        }

        public void SendImage(string destination, byte[] ImageData, WhatsAppApi.ApiBase.ImageType type)
        {
            WhatsUser tmpUser = GetWhatsUser(destination);
            //Console.WriteLine("[] Send media to {0}", tmpUser);
            wa.SendMessageImage(tmpUser.GetFullJid(), ImageData, type);
        }

        public void SendAudio(string destination, byte[] ImageData, WhatsAppApi.ApiBase.AudioType type)
        {
            WhatsUser tmpUser = GetWhatsUser(destination);
            //Console.WriteLine("[] Send media to {0}", tmpUser);
            wa.SendMessageAudio(tmpUser.GetFullJid(), ImageData, type);
        }

        private WhatsUser GetWhatsUser(string destination)
        {
            WhatsUser tmpUser;
            if (!users.ContainsKey(destination))
            {
                tmpUser = usrMan.CreateUser(destination, "User");
                users.Add(destination, tmpUser);
            }
            tmpUser = users[destination];
            return tmpUser;
        }

        public event EventHandler<MessageEventArgs> IncomingMessageReceived;

        protected virtual void OnMessageReceived(MessageEventArgs args)
        {
            if (this.IncomingMessageReceived != null)
            {
                var handler = this.IncomingMessageReceived;
                handler(this, args);
            }
        }
    }
}
