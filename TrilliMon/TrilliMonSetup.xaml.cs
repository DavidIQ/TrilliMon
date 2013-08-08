using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrilliMon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TrilliMonSetup : Window
    {
        private static bool notificationIconLoaded = false;
        private static FolderMonitor folderMon = null;

        public TrilliMonSetup()
        { InitializeComponent(); }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Load application settings
            ckUsingAstra.IsChecked = Properties.Settings.Default.UsingAstra;
            ckRememberLogin.IsChecked = !string.IsNullOrWhiteSpace(Properties.Settings.Default.Username);
            tbUsername.Text = Properties.Settings.Default.Username;
            tbCellNumber.Text = Properties.Settings.Default.CellNumber;
            if (string.IsNullOrWhiteSpace(tbUsername.Text))
            { tbUsername.Focus(); }
            else
            { tbPassword.Focus(); }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            string GVUsername = tbUsername.Text.Trim();
            string GVPassword = tbPassword.Password.Trim();
            string CellNumber = tbCellNumber.Text.Trim();

            //Do some sanity checks first
            double i = 0;
            if (string.IsNullOrWhiteSpace(GVUsername))
            {
                MessageBox.Show("You must enter your Google Voice username.", System.Windows.Forms.Application.ProductName);
                tbUsername.Focus();
            }
            else if (string.IsNullOrWhiteSpace(GVPassword))
            {
                MessageBox.Show("You must enter your Google Voice password.", System.Windows.Forms.Application.ProductName);
                tbPassword.Focus();
            }
            else if (string.IsNullOrWhiteSpace(CellNumber))
            {
                MessageBox.Show("You must enter the number you wish the messages to be forwarded to.", System.Windows.Forms.Application.ProductName);
                tbCellNumber.SelectAll();
                tbCellNumber.Focus();
            }
            else if (CellNumber.Length != 10 || double.TryParse(CellNumber, out i) == false)
            {
                MessageBox.Show("You have entered an invalid/incomplete number.", System.Windows.Forms.Application.ProductName);
                tbCellNumber.SelectAll();
                tbCellNumber.Focus();
            }
            else
            {
                //Save some settings
                if (ckRememberLogin.IsChecked == true)
                { Properties.Settings.Default.Username = GVUsername; }
                else
                { Properties.Settings.Default.Username = string.Empty; }
                Properties.Settings.Default.UsingAstra = (bool)ckUsingAstra.IsChecked;
                Properties.Settings.Default.CellNumber = CellNumber;

                FoldersList fl = new FoldersList();
                fl.usingAstra = (bool)ckUsingAstra.IsChecked;
                fl.monitorFolders = Properties.Settings.Default.MonitorFolders;
                fl.Owner = this;
                fl.ShowDialog();

                //We need to know which folders to monitor
                if (fl.monitorFolders == null || fl.monitorFolders.Count == 0)
                { return; }
                
                Properties.Settings.Default.MonitorFolders = fl.monitorFolders;
                Properties.Settings.Default.Save();

                this.IsEnabled = false;

                //Check to make sure we have a valid login
                using (SharpGoogleVoice gv = new SharpGoogleVoice(GVUsername, GVPassword))
                {
                    if (gv.ValidLogin() == false)
                    {
                        MessageBox.Show("Could not log in to Google Voice.  Please verify that your credentials are correct.", System.Windows.Forms.Application.ProductName);
                        this.IsEnabled = true;
                        return;
                    }
                }

                if (notificationIconLoaded == false)
                {
                    NotificationIcon();
                    notificationIconLoaded = true;
                }

                folderMon = new FolderMonitor();
                folderMon.gvUsername = GVUsername;
                folderMon.gvPassword = GVPassword;
                folderMon.cellNum = CellNumber;
                folderMon.usingAstra = (bool)ckUsingAstra.IsChecked;
                folderMon.monitorFolders = Properties.Settings.Default.MonitorFolders;
                folderMon.Show();
                folderMon.setupWin = this;

                this.Hide();
            }
        }

        private void NotificationIcon()
        {
            //Set up our notification/taskbar icon
            System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = System.Windows.Forms.Application.ProductName;
            notifyIcon.Icon = Properties.Resources.TrilliMon;
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipText = "TrilliMon is now monitoring your conversations.";
            notifyIcon.ShowBalloonTip(5000);
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add("&Show Log", new System.EventHandler(this.menuShowLog_Click));
            contextMenu.MenuItems.Add("E&xit", new System.EventHandler(this.menuExit_Click));
            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            folderMon.Show();
        }

        private void menuShowLog_Click(object sender, System.EventArgs e)
        {
            folderMon.Show();
        }

        private void menuExit_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Exit TrilliMon?", System.Windows.Forms.Application.ProductName, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Environment.Exit(0);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void numericOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            int i = 0;
            e.Handled = !int.TryParse(e.Text, out i);
        }
    }
}
