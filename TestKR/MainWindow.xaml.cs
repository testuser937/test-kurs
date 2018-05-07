using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestKR
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            getDrives(); // загрузка списка дисков
        }


        private void getDrives()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Tag = drive;
                item.Header = drive.ToString();
                item.Items.Add("*");
                files_treeView.Items.Add(item);
            }
        }

        private void getFiles(string startdirectory, List<string> filess)
        {
            string[] searchdirectory = Directory.GetDirectories(startdirectory);
            if (searchdirectory.Length > 0)
            {
                for (int i = 0; i < searchdirectory.Length; i++)
                {
                    getFiles(searchdirectory[i] + @"\", filess);
                }
            }
            string[] filesss = Directory.GetFiles(startdirectory);
            for (int i = 0; i < filesss.Length; i++)
            {
                filess.Add(filesss[i]);
            }
        }

        private void files_treeView_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.Items.Clear();
            DirectoryInfo dir;
            if (item.Tag is DriveInfo)
            {
                DriveInfo drive = (DriveInfo)item.Tag;
                dir = drive.RootDirectory;
            }
            else
                dir = (DirectoryInfo)item.Tag;
            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = subDir;
                    newItem.Header = subDir.ToString();
                    newItem.Items.Add("*");
                    foreach (FileInfo f in subDir.GetFiles())
                    {
                        newItem.Items.Add(f.Name);
                    }
                    item.Items.Add(newItem);
                }
            }
            catch
            { }
        }

    }
}
