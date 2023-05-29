using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WUApiLib;

//using Microsoft.U

namespace WindowsUpdateChecker
{
    class checkForUpdate
    {
        public void isNewUpdate(bool sendEMail)
        {
            
            var updateSession = new UpdateSession();
            var updateSearcher = updateSession.CreateUpdateSearcher();
            updateSearcher.Online = false; //true = search online
            try
            {
                Console.Write("\rChecking for new updates: {0}", "pending");
                var searchResult = updateSearcher.Search("IsInstalled=0 And IsHidden=0");
                if (searchResult.Updates.Count > 0)
                {
                    List<string> updateNames = new List<string>();

                    Console.Write("\rChecking for new updates: {0}", "There are updates available for installation:\n");

                    foreach (IUpdate update in searchResult.Updates)
                    {
                        updateNames.Add("<p> > Update Name: " + update.Title + "</p>");
                    }  
                    sendMail(updateNames, sendEMail);
                }
                else
                {
                    Console.WriteLine("\rChecking for new updates: {0}", "There are no updates available for installation\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\rChecking for new updates: {0}", ex.Message, "Error");
            }
        }

        private void sendMail(List<string> updateNames, bool sendEMail) 
        {
            string mailContent = "<HTML><BODY><p>available updates:</p>";
            bool settings = File.Exists("settings.ini");
            if (!sendEMail || !settings) Console.WriteLine("> > > sending mail skipped...");

            foreach (string prime in updateNames)
            {
                Console.WriteLine("FROM LIST: " + prime);
                mailContent += "<div>" + prime + "</div>";
            }
            mailContent += "</BODY></HTML>";

            if (sendEMail)
            {
                //read ini
                var MyIni = new IniFile("settings.ini");
                var Host = MyIni.Read("Host");
                var Port = MyIni.Read("Port");
                var SSL = MyIni.Read("SSL");
                var EMailFrom = MyIni.Read("EMailFrom");
                var EMailTo = MyIni.Read("EMailTo");
                var Password = MyIni.Read("Password");

                Console.WriteLine("\n > > > sending mail...");
                MailMessage message = new MailMessage(EMailFrom, EMailTo);
                message.Subject = "Windows Update News";
                message.Body = mailContent;
                message.IsBodyHtml = true;
                message.BodyEncoding = UTF8Encoding.UTF8;
                message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                SmtpClient client = new SmtpClient();
                client.Port =Int16.Parse(Port);
                client.Host = Host;
                //SecurityProtocolType protocol = SecurityProtocolType.Tls12;
                

                client.EnableSsl = true;// Convert.ToBoolean(SSL);
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(EMailFrom, Password);

                
                //MailMessage mm = new MailMessage(EMail, Password, "NEW WINDOWS UPDATES AVAILABLE", mailContent);
                //mm.BodyEncoding = UTF8Encoding.UTF8;
                //mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(message);
            }
        }

        class IniFile
        {
            string Path;
            string EXE = Assembly.GetExecutingAssembly().GetName().Name;

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

            public IniFile(string IniPath = null)
            {
                Path = new FileInfo(IniPath ?? EXE + ".ini").FullName;
            }

            public string Read(string Key, string Section = null)
            {
                var RetVal = new StringBuilder(255);
                GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
                return RetVal.ToString();
            }

            public void Write(string Key, string Value, string Section = null)
            {
                WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
            }

            public void DeleteKey(string Key, string Section = null)
            {
                Write(Key, null, Section ?? EXE);
            }

            public void DeleteSection(string Section = null)
            {
                Write(null, null, Section ?? EXE);
            }

            public bool KeyExists(string Key, string Section = null)
            {
                return Read(Key, Section).Length > 0;
            }
        }
    }
}

