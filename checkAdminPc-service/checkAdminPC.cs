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

		static string setJsonFile = File.ReadAllText("C:\\Program Files\\checkAdminPc\\set.json");
		static Settings settings = JsonConvert.DeserializeObject<Settings>(setJsonFile);
		static string lastSendLoggedUser = "";
		static DateTime lastSendTime = DateTime.Now;

		public checkAdminPC() {
			InitializeComponent();
		}

		HttpStatusCode send(string url, string json) {
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
			try {
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
				httpWebRequest.ContentType = "application/json; charset=utf-8";
				httpWebRequest.Method = "POST";

				using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
					streamWriter.Write(json);
				}
				using (var response = httpWebRequest.GetResponse()) {
					return (((HttpWebResponse)response).StatusCode);
				}
			} catch (Exception e) {
				EventLog.WriteEntry("session ex\n>> " + e.Message + "\n>> " + e.StackTrace);
				return HttpStatusCode.ExpectationFailed;
			}
			//HttpWebResponse httpRes = (HttpWebResponse) httpWebRequest.GetResponse();
		}

		void findActiveUser() {
			string userName = "";
			ITerminalServer server = new TerminalServicesManager().GetLocalServer();
			try {
				server.Open();
				ITerminalServicesSession session = server.GetSessions().First(s => s.ConnectionState == Cassia.ConnectionState.Active);
				userName = session.UserName;
			} catch (InvalidOperationException ioe) {
				userName = "";
			} catch (Exception e) {
				//userName = "";
				EventLog.WriteEntry("session ex\n>> " + e.Message + "StackTrace\n>> " + e.StackTrace + "GetType\n>> " + e.GetType());
			}
			if (lastSendLoggedUser != userName | DateTime.Now.Subtract(lastSendTime).Minutes >= 5) {
				string url = new StringBuilder(settings.serverUrl).Append(settings.computerName).Append("/").ToString();
				string json = new StringBuilder("{\"key\": \"")
					.Append(settings.key)
					.Append("\", \"lastLoggedUser\": \"")
					.Append(userName == String.Empty ? null : userName)
					.Append("\"}")
					.ToString();
				HttpStatusCode status = send(url, json);
				//EventLog.WriteEntry("userName\n>> " + userName + "\nlastSendLoggedUser\n>> " + lastSendLoggedUser
				//+ "\nsendData\n>> " + json + '\n' + url + "\nstatus\n>> " + status.ToString());
				if (status == HttpStatusCode.OK) {
					lastSendLoggedUser = userName;
					lastSendTime = DateTime.Now;
				}
			}
		}


		protected override void OnStart(string[] args) {
			Task.Run(() => {

				while (true) {
					try {
						findActiveUser();
						//exec();
					} catch (Exception e) {
						settings = JsonConvert.DeserializeObject<Settings>(setJsonFile);
						if (!settings.serverUrl.EndsWith("/")) settings.serverUrl.Append('/');
						if (!settings.serverUrl.StartsWith("http://")) settings.serverUrl = "http://" + settings.serverUrl;
					} finally {
						Thread.Sleep(5000);
						//EventLog.WriteEntry("dif seconds\n>> " + DateTime.Now.Subtract(lastSendTime).Seconds.ToString());
					}
				}

			});
		}

		protected override void OnStop() { }
	}
}
