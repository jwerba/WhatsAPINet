using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using WhatsAppApi.Facades;

namespace Demos
{
    class Program
    {

        private static ILog log;
        private static WhatsAppConnector whatsApp;
        private static string cursor = ">";

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            log = LogManager.GetLogger(typeof(Program));
            
            //Add a .settings file called Credetials.settings and the following entries
            //Username = XXXXX
            //Password = YYYYY
            //Nickname = ZZZZZ

            
            whatsApp = new WhatsAppConnector(Credentials.Default.Username, Credentials.Default.Password, Credentials.Default.Nickname);
            whatsApp.Connect();

            whatsApp.IncomingMessageReceived += delegate(object sender, MessageEventArgs args3)
            {
                Console.WriteLine();
                Console.WriteLine(string.Format("From {0}: {1}", args3.From, args3.Text));
                Console.Write(cursor);
            };

            whatsApp.Media += delegate(object sender, MediaEventArgs e)
            {
                File.WriteAllBytes(e.FileName, e.Bytes);
                Console.WriteLine(string.Format("File {0} received from {1}", e.FileName, e.From));
            };


            BindToConsole();


            Console.WriteLine("Press [ENTER] to disconnect");
            Console.ReadLine();
            whatsApp.Disconnect();
        }

        private static void BindToConsole()
        {
            Console.Write(cursor);
            string line = string.Empty;
            line = Console.ReadLine();
            while (!line.Trim().ToLower().Equals("exit"))
            {
                line = line.Trim().ToLower();

                if (line.StartsWith("talk to"))
                {
                    string jid = line.Replace("talk to", "").Trim();
                    cursor = string.Format("{0}>", jid);
                    Console.Write(cursor);
                    line = Console.ReadLine();
                    while (!line.Trim().ToLower().Equals("exit"))
                    {
                        if (line.StartsWith("send image "))
                        {
                            string filename = line.Split(' ')[2];
                            whatsApp.SendImage(jid, File.ReadAllBytes(filename), WhatsAppApi.ApiBase.ImageType.JPEG);
                        }
                        else if (line.StartsWith("send audio "))
                        {
                            string filename = line.Split(' ')[2];
                            whatsApp.SendAudio(jid, File.ReadAllBytes(filename), WhatsAppApi.ApiBase.AudioType.MP3);
                        }
                        else
                        {
                            whatsApp.SendMessage(jid, line);
                        }
                        Console.Write(cursor);
                        line = Console.ReadLine();
                    }
                }
                cursor = ">";
                Console.Write(cursor);
                line = Console.ReadLine();
            }
        }

    }
}
