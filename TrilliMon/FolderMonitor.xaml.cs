using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Collections.Specialized;

namespace TrilliMon
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class FolderMonitor : Window
    {
        public string gvUsername = string.Empty;
        public string gvPassword = string.Empty;
        public string cellNum = string.Empty;
        public bool usingAstra = false;
        public TrilliMonSetup setupWin = null;
        public StringCollection monitorFolders = null;

        public FolderMonitor()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string trillianLogs = CommonFuncs.GetTrillianLogsDir(usingAstra);
            
            if (!Directory.Exists(trillianLogs)) //Can't find the Trillian logs folder...
            {
                lbLog.Items.Add("Unable to find the Trillian logs directory...");
            }
            else
            {
                //Create a watcher for each selected directory
                foreach (string watchFolder in monitorFolders)
                {
                    string watchDir = trillianLogs + string.Format(@"\{0}\Query", watchFolder);
                    if (Directory.Exists(watchDir))
                    {
                        FileSystemWatcher watcher = new FileSystemWatcher();
                        watcher.Filter = "*.xml";
                        watcher.Path = watchDir;
                        watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                        watcher.Changed += new FileSystemEventHandler(watcher_FileAction);
                        watcher.Created += new FileSystemEventHandler(watcher_FileAction);
                        watcher.Deleted += new FileSystemEventHandler(watcher_FileAction);
                        //watcher.Renamed += new FileSystemEventHandler(watcher_FileCreated);
                        watcher.Error += new ErrorEventHandler(OnError);
                        watcher.EnableRaisingEvents = true;
                    }
                }
                this.Hide(); //Hide the window when it loads and everything else checks out
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void watcher_FileAction(object sender, System.IO.FileSystemEventArgs e)
        {
            WatcherChangeTypes wct = e.ChangeType;
            HandleMessage(string.Format("File {0} {1}", e.Name, wct.ToString()));
            string[] alllines = File.ReadAllLines(e.FullPath);
            string lastline = alllines[alllines.Length - 1];

            if (lastline.IndexOf("incoming_privateMessage") > 0)
            {
                using (SharpGoogleVoice gv = new SharpGoogleVoice(gvUsername, gvPassword))
                {
                    string from = e.Name.Replace(".xml", "");
                    if (from.Contains("@") && !from.StartsWith("@")) { from = from.Substring(0, from.IndexOf("@")); }
                    HandleMessage("Incoming Message");
                    Regex rnrRegex = new Regex(@"text=""(.*?)""");
                    if (rnrRegex.IsMatch(lastline))
                    {
                        //send the content of the message
                        string msg = Uri.UnescapeDataString(rnrRegex.Match(lastline).ToString().Trim());
                        if (msg.StartsWith("text=\"")) { msg = msg.Substring(("text=\"").Length).Trim(); }
                        if (msg.EndsWith("\"")) { msg = msg.Substring(0, msg.Length - 1); }
                        gv.SendSMS("+1" + cellNum, from + ": " + msg);
                    }
                    else
                    {
                        //did not get a regex match...still let me know that I got an IM
                        gv.SendSMS("+1" + cellNum, from + " has sent you an IM!");
                    }
                }
            }
            else
            {
                HandleMessage("Not an incoming message.");
            }
        }

        private void OnError(object source, ErrorEventArgs e)
        {
            HandleMessage("Ooops! We got an error: " + e.GetException().Message);
            Dispatcher.BeginInvoke((Action)(() => this.Show()));
        }

        private void HandleMessage(string msg)
        {
            Dispatcher.BeginInvoke((Action)(() => lbLog.Items.Add(msg)));
        }
    }
}
