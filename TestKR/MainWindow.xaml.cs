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
using System.Drawing;
using System.Collections.ObjectModel;

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

        public class NameIconPair
        {
            public string Name { get; set; }
            public BitmapSource IconSource { get; set; }
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
                ObservableCollection<NameIconPair> pairs = new ObservableCollection<NameIconPair>();

                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = subDir;
                    newItem.Header = subDir;//.ToString();
                    newItem.Items.Add("*");
                    item.Items.Add(newItem);
                }
                

                try
                {
                    foreach (FileInfo f in dir.GetFiles())
                    {
                        item.Items.Add(f);
                        Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(@"F:\Mooo\111.txt"); ;//System.Drawing.Icon.ExtractAssociatedIcon(f.Name);
                        Stream stream = new MemoryStream();
                        icon.Save(stream);
                        BitmapDecoder decoder = IconBitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
                        BitmapSource src = decoder.Frames[0];
                        pairs.Add(new NameIconPair() { Name = f.Name, IconSource = src });
                        this.Icon = src;
                    }
                    this.DataContext = pairs;
                    
                }
                catch { }
            }
            catch (Exception ex)
            {MessageBox.Show(ex.Message); }

        }

        private void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            TreeViewItem tvi = (TreeViewItem)sender;
            e.Handled = true;
            
            
                MessageBox.Show();
            try
            {
                
            }
                //MessageBox.Show(tvi is FileInfo);
            var a = (TreeViewItem)files_treeView.SelectedItem;
           MessageBox.Show(a.Tag.ToString());
            
        }
    }
}
