using GlancesClientSharp.Glances;
using GlancesClientSharp.Glances.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace GlancesClientSharp
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NotifyContext());
        }

        public class NotifyContext : ApplicationContext
        {
            GlancesServer server;
            private Thread updater;
            string SrvAdr = "http://bukkitcrafters.de:61208";
            string NetInt = "enp5s0f0";
            private static NotifyIcon tray;
            private bool TrayClicked = false;

            //CPU
            MenuItem menuCpu = new MenuItem(); //TopLevel & CpuTotal
            MenuItem menuCorePhysical = new MenuItem();
            MenuItem menuCoreLogical = new MenuItem();
            MenuItem menuQuickLookCpuName = new MenuItem();
            MenuItem menuQuickLookCpuHz = new MenuItem();
            MenuItem menuQuickLookCpuHzCurrent = new MenuItem();

            MenuItem menuMemLoad = new MenuItem();

            MenuItem menuFileSystem = new MenuItem();
            //Net
            MenuItem menuNetUp = new MenuItem();
            MenuItem menuNetDown = new MenuItem();
            MenuItem menuIpAdress = new MenuItem();

            //System
            MenuItem menuSystem = new MenuItem(); //TopLevel
            MenuItem menuSystemOsName = new MenuItem();
            MenuItem menuSystemOsVersion = new MenuItem();
            MenuItem menuSystemLinuxDistro = new MenuItem();
            MenuItem menuSystemHostname = new MenuItem();
            MenuItem menuSystemPlatform = new MenuItem();
            MenuItem menuSystemHumanReadableName = new MenuItem();

            //UpTime
            MenuItem menuUpTime = new MenuItem();

            MenuItem menuAbout = new MenuItem("About", ti_about);
            MenuItem menuExit = new MenuItem("Exit", (sender, e) =>
            {
                tray.Visible = false;
                tray.Dispose();
                Environment.Exit(0);
            });

            public NotifyContext()
            {
                server = new Glances.GlancesServer(SrvAdr);
                updater = new Thread(() => UpdateValues());
                updater.IsBackground = true;


                tray = new NotifyIcon()
                {
                    Icon = Properties.Resources.favicon,
                    ContextMenu = new ContextMenu(new MenuItem[]
                    {
                        menuCpu,
                        menuMemLoad,
                        menuFileSystem,
                        menuNetUp,
                        menuNetDown,
                        menuIpAdress,
                        menuUpTime,
                        menuSystem,
                        new MenuItem("-"),
                        menuAbout,
                        menuExit
                    }),
                    Visible = true,
                };

                menuCpu.MenuItems.AddRange(new MenuItem[]
                {
                    menuQuickLookCpuName,
                    menuCorePhysical,
                    menuCoreLogical,
                    menuQuickLookCpuHz,
                    menuQuickLookCpuHzCurrent
                });

                menuSystem.MenuItems.AddRange(new MenuItem[]
                {
                    menuSystemOsName,
                    menuSystemOsVersion,
                    menuSystemLinuxDistro,
                    menuSystemHostname,
                    menuSystemPlatform,
                    menuSystemHumanReadableName
                });

                //tray.MouseClick += tray_Click;
                updater.Start();
            }

            /*private void tray_Click(object sender, System.EventArgs e)
            {
                TrayClicked = true;
            }*/


            private static void ti_about(object sender, EventArgs e)
            {
                MessageBox.Show("Created by Cubicon" + Environment.NewLine + "Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }

            private void UpdateValues()
            {
                Stopwatch sw = new Stopwatch();

                
                /*if (TrayClicked == true)
                {


                }*/

                while (true)
                {
                    sw.Start();
                    Cpu cpu = null;
                    Memory ram = null;
                    Network[] net = null;
                    All all = null;
                    FileSystem filesystem = null;
                    QuickLook quicklook = null;
                    Core core = null;
                    try
                    {
                        all = server.PerformQueryHack<All>("all"); //(All)server.PerformQuery<All>();
                        cpu = all.Cpu; //(Cpu)server.PerformQuery<Cpu>();
                        ram = (Memory)server.PerformQuery<Memory>();
                        net = all.Network.ToArray(); //(Network[])server.PerformQuery<Network>();
                        var fs = server.PerformQueryHack<List<FileSystem>>("fs");
                        filesystem = fs[0];
                        //quicklook = all.QuickLook;
                        quicklook = server.PerformQueryHack<QuickLook>("quicklook");// (Glances.Plugins.QuickLook)server.PerformQuery<Glances.Plugins.QuickLook>();
                        //core = all.Core; //(Core)server.PerformQueryHack<Core>("Core");
                        core = server.PerformQueryHack<Core>("core");

                    }
                    catch (Exception ex)
                    {
                        all = new All() { System = new Glances.Plugins.System(), Ip = new Ip() };
                        cpu = new Cpu();
                        ram = new Memory();
                        net = new Network[] { new Network() { InterfaceName = NetInt } };
                        quicklook = new QuickLook();
                        core = new Core();
                        filesystem = new FileSystem() { Size = 0, Used = 0 };
                    }

                    // this.Invoke((MethodInvoker)delegate //Only on Windows Forms
                    try
                    {
                        {
                            //CPU
                            menuCpu.Text = string.Format("Cpu {0}%", Math.Round((double)cpu.Total, 2));
                            menuQuickLookCpuName.Text = string.Format("CPU Model : {0}", quicklook.CpuName);// all.QuickLook.CpuName;
                            menuCorePhysical.Text = string.Format("Cores : {0}", core.Physical);
                            menuCoreLogical.Text = string.Format("Threads : {0}", core.Logical);
                            menuQuickLookCpuHz.Text = string.Format("Max Clock : {0}", GetUnitHz(quicklook.CpuHz/*all.QuickLook.CpuHz*/));
                            menuQuickLookCpuHzCurrent.Text = string.Format("Current Clock : {0}", GetUnitHz(quicklook.CpuHzCurrent/*all.QuickLook.CpuHzCurrent*/));

                            menuMemLoad.Text = string.Format("MemUsage {0} ({1}%)", GetUnitSize(ram.Total - ram.Free), Math.Round((double)ram.Percent, 2));
                            var netLo = net.First(x => x.InterfaceName == NetInt);
                            menuNetUp.Text = string.Format("NetUp ˄{0}", GetUnitSize(netLo.Tx));
                            menuNetDown.Text = string.Format("NetDown ˅{0}", GetUnitSize(netLo.Rx));
                            menuIpAdress.Text = "IpAddress " + all.Ip.PublicAddress;
                            menuUpTime.Text = "UpTime " + all.Uptime;
                            menuFileSystem.Text = string.Format("FreeSpace: {0}", GetUnitSize(filesystem.Size - filesystem.Used), Math.Round((float)filesystem.Percent, 2));

                            //menuSystem
                            menuSystem.Text = "System";
                            menuSystemOsName.Text = "OS : " + all.System.OsName;
                            menuSystemOsVersion.Text = "Kernel : " + all.System.OsVersion;
                            menuSystemLinuxDistro.Text = "Distro : " + all.System.LinuxDistro;
                            menuSystemHostname.Text = "Hostname : " + all.System.Hostname;
                            menuSystemPlatform.Text = "Platform : " + all.System.Platform;
                            menuSystemHumanReadableName.Text = "HRN : " + all.System.HumanReadableName;

                        }//);
                        sw.Stop();
                        if (sw.ElapsedMilliseconds < 1000)
                            Thread.Sleep(1000 - (int)sw.ElapsedMilliseconds);
                        sw.Reset();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("EXCEPTION: {0} {1}\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace));
                        Debug.WriteLine(string.Format("EXCEPTION: {0} {1}\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace));
                    }
                }
            }

            private static string[] unitSize = new string[] { "B", "KB", "MB", "GB", "TB" };
            private static string[] unitHz = new string[] { "Hz", "KHz", "MHz", "GHz" };
            private static string GetUnitSize(long size)
            {
                return GetUnit(size, 1024, unitSize);
            }
            private static string GetUnitHz(long size)
            {
                return GetUnit(size, 1000, unitHz);
            }
            private static string GetUnit(long size, int divider, string[] _units)
            {
                double s = (double)size;
                int idx = 0;
                for (; s >= divider; s /= divider, idx++) ;

                return string.Format("{0} {1}", Math.Round(s, 2), _units[idx]);
            }
        }

    }
}