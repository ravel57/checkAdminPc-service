using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace checkAdminPc_service {
	static class Program {
		/// <summary>
		/// Главная точка входа для приложения.
		/// </summary>

		static void install() {
			try {
				var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
				System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { appPath });
				ServiceController sc = new ServiceController("checkAdminPC");
				sc.Start();
				Console.WriteLine("\nInstalled successfully\n");
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
			}
		}

		static void uninstall() {
			try {
				var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
				System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/u", appPath });
				Console.WriteLine("Unnstalled successfully");
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
			}
		}

		static void Main(string[] args) {
			if (!Environment.UserInteractive) {
				ServiceBase[] ServicesToRun = new ServiceBase[] {
					new checkAdminPC()
				};
				ServiceBase.Run(ServicesToRun);

			} else {
				if (args != null && args.Length > 0) {
					if (args.Contains("--uninstall") | args.Contains("-u"))
						uninstall();
				} else {
					install();
				}
			}
		}
	}
}
