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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Specialized;

namespace TrilliMon
{
    /// <summary>
    /// Interaction logic for FoldersList.xaml
    /// </summary>
    public partial class FoldersList : Window
    {
        public bool usingAstra = false;
        public StringCollection monitorFolders = null;

        public FoldersList()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            monitorFolders = new StringCollection();
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            monitorFolders = new StringCollection();
            foreach (BoolStringClass cb in lbFolders.Items)
            {
                if (cb.IsSelected == true)
                {
                    monitorFolders.Add(cb.TheText);
                }
            }
            if (monitorFolders.Count == 0)
            { MessageBox.Show("You need to select at least one folder to monitor.", System.Windows.Forms.Application.ProductName); }
            else { this.Close(); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FolderList = new ObservableCollection<BoolStringClass>();
            string trillianLogsDir = CommonFuncs.GetTrillianLogsDir(usingAstra);

            if (Directory.Exists(trillianLogsDir))
            {
                foreach (string dir in Directory.GetDirectories(trillianLogsDir))
                {
                    DirectoryInfo dirinfo = new DirectoryInfo(dir);
                    string queryDir = trillianLogsDir + @"\" + dirinfo.Name + @"\Query";
                    if (Directory.Exists(queryDir))
                    {
                        if (Directory.GetFiles(queryDir, "*.xml").Length > 0)
                        {
                            BoolStringClass listItem = new BoolStringClass();
                            listItem.TheText = dirinfo.Name;
                            listItem.IsSelected = (monitorFolders != null && monitorFolders.Contains(dirinfo.Name));
                            FolderList.Add(listItem);
                        }
                    }
                }

                lbFolders.ItemsSource = FolderList;
            }
            else
            {
                MessageBox.Show("Unable to locate the Trillian logs.", System.Windows.Forms.Application.ProductName);
            }
        }

        public ObservableCollection<BoolStringClass> FolderList { get; set; }

        public class BoolStringClass
        {
            public string TheText { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
