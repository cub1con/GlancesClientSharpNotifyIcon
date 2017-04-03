using GlancesClientSharp.Glances;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GlancesClientSharp
{
    public partial class frmMain : Form
    {
        GlancesServer server;
        private static Color clrBlue = Color.FromArgb(96, 140, 226);
        private static Color clrRed = Color.FromArgb(221, 80, 79);
        private Thread updater;
        private static string SrvAdr = "http://bukkitcrafters.de:61208";
        private static string NetInt = "enp5s0f0";

        public frmMain()
        {
            InitializeComponent();
            updater = new Thread(() => UpdateValues());
            updater.IsBackground = true;
            updater.Start();
        }

        private void UpdateValues()
        {
            Stopwatch sw = new Stopwatch();
            while (true)
            {
                sw.Start();
                Glances.Plugins.Cpu cpu = null;
                Glances.Plugins.Memory ram = null;
                Glances.Plugins.Network[] net = null;
                Glances.Plugins.All all = null;
                try
                {
                    all = (Glances.Plugins.All)server.PerformQuery<Glances.Plugins.All>();
                    cpu = (Glances.Plugins.Cpu)server.PerformQuery<Glances.Plugins.Cpu>();
                    ram = (Glances.Plugins.Memory)server.PerformQuery<Glances.Plugins.Memory>();
                    net = (Glances.Plugins.Network[])server.PerformQuery<Glances.Plugins.Network>();
                }
                catch (Exception ex)
                {
                    all = new Glances.Plugins.All() { System = new Glances.Plugins.System(), Ip = new Glances.Plugins.Ip() };
                    cpu = new Glances.Plugins.Cpu();
                    ram = new Glances.Plugins.Memory();
                    net = new Glances.Plugins.Network[] { new Glances.Plugins.Network() { InterfaceName = NetInt } };
                }

                this.Invoke((MethodInvoker)delegate
                    {
                        grpCpu.DataEntries[0].Data.Add(cpu.Total);
                        lblCpu.Text = string.Format("{0}%", Math.Round((double)cpu.Total, 2));
                        grpRam.DataEntries[0].Data.Add(ram.Percent);
                        lblRam.Text = string.Format("{0} ({1}%)", GetUnitSize(ram.Total - ram.Free), Math.Round((double)ram.Percent, 2));
                        var netLo = net.First(x => x.InterfaceName == NetInt);
                        lblNetUp.Text = string.Format("˄{0}", GetUnitSize(netLo.Tx));
                        lblNetDown.Text = string.Format("˅{0}", GetUnitSize(netLo.Rx));
                        grpNetDown.DataEntries[0].Data.Add(netLo.Rx);
                        grpNetUp.DataEntries[0].Data.Add(netLo.Tx);
                        lblHost.Text = all.System.Hostname;
                        lblCovered.Text = TimeSpan.FromSeconds(grpCpu.DataEntries[0].Data.Capacity).ToString();
                        lblIp.Text = all.Ip.PublicAddress;
                        lblUptime.Text = all.Uptime;
                    });
                sw.Stop();
                if (sw.ElapsedMilliseconds < 1000)
                    Thread.Sleep(1000 - (int)sw.ElapsedMilliseconds);
                sw.Reset();
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

            return string.Format("{0}{1}", Math.Round(s, 2), _units[idx]);
        }
    }
}
