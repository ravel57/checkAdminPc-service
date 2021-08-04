using Cassia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace checkAdminPc_service {
    class Settings {
        public string key;
        public string serverUrl;
        public string computerName;
    }


    public partial class checkAdminPC : ServiceBase {

        static string setJson = File.ReadAllText(".\\set.json");
        static Settings settings = JsonConvert.DeserializeObject<Settings>(setJson);

        public checkAdminPC() {
            InitializeComponent();
        }

        void exec() {
            ITerminalServer server = new TerminalServicesManager().GetLocalServer();
            server.Open();
            ITerminalServicesSession session = server.GetSessions().First(s => s.ConnectionState == Cassia.ConnectionState.Active);
            if(session != null) {

                string userName = session.UserAccount.Value.Remove(0, 1 + session.DomainName.Length);
                if(!settings.serverUrl.EndsWith("/")) settings.serverUrl += '/';
                string url = new StringBuilder(settings.serverUrl).Append(settings.computerName).ToString();
                string json = new StringBuilder("{\"key\": \"").Append(settings.key).Append("\", \"lastLoggedUser\": \"").Append(userName).Append("\"}").ToString();

                HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = "POST";
                //Console.WriteLine(json);

                using(var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                    streamWriter.Write(json);
                }
                using(var response = httpWebRequest.GetResponse()) {}
                //HttpWebResponse httpRes = (HttpWebResponse) httpWebRequest.GetResponse();
            }
        }


        protected override void OnStart(string[] args) {
            Task.Run(() => {
                while(true) {
                    try {
                        exec();
                    } catch(WebException e) {
                        settings = JsonConvert.DeserializeObject<Settings>(setJson);
                        //EventLog.WriteEntry("WebException: " + e.Message, EventLogEntryType.Error);
                    } catch(InvalidOperationException e) {
                        settings = JsonConvert.DeserializeObject<Settings>(setJson);
                        //EventLog.WriteEntry("InvalidOperationException: " + e.Message, EventLogEntryType.Error);
                    } catch(Exception e) {
                        settings = JsonConvert.DeserializeObject<Settings>(setJson);
                        //EventLog.WriteEntry("Exception: " + e.Message, EventLogEntryType.Error);
                    } finally {
                        Thread.Sleep(20000);
                    }
                }
            });

        }

        protected override void OnStop() {
            //EventLog.WriteEntry("We did it! Stoped", EventLogEntryType.Information);
        }
    }
}
