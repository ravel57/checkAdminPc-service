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
        static void Main(string[] args) {
            if(Environment.UserInteractive) {
                try {
                    var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { appPath });
                    ServiceController sc = new ServiceController("checkAdminPC");
                    sc.Start();
                    Console.WriteLine("\nInstalled successfully\n");
                } catch(Exception ex) { Console.WriteLine(ex.Message); }
                if(args != null && args.Length > 0) {
                    switch(args[0]) {
                        case "--uninstall":
                            try {
                                var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                                System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/u", appPath });
                                Console.WriteLine("Unnstalled successfully");
                            } catch(Exception ex) { Console.WriteLine(ex.Message); }
                            break;
                    }
                }
            } else {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] {
                    new checkAdminPC()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
