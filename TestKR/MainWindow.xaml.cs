using Shell32;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TestKR
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static Shell shell = new Shell();
        public static Folder RecyclingBin = shell.NameSpace(10);
        public string copyFolderName = "";

        public MainWindow()
        {
            InitializeComponent();
            getDrives(); // загрузка списка дисков
        }


        public class NameIconPair
        {
            public String Name { get; set; }
            public BitmapSource IconSource { get; set; }
        }

        ObservableCollection<NameIconPair> pairs = new ObservableCollection<NameIconPair>();

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

        private void files_treeView_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.Items.Clear();
            DirectoryInfo dir;

            //определяем по тегу кликнули на диск или папку
            if (item.Tag is DriveInfo)
            {
                DriveInfo drive = (DriveInfo)item.Tag;
                dir = drive.RootDirectory;
            }
            else if (item.Tag is DirectoryInfo)
                dir = (DirectoryInfo)item.Tag;
            else
            {
                return; // при попытке раскрыть файл
            }
            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = subDir;
                    newItem.Header = subDir;//.ToString();
                    newItem.HeaderStringFormat = subDir.FullName;
                    newItem.Items.Add("*");
                    item.Items.Add(newItem);
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }

            try
            {
                foreach (FileInfo f in dir.GetFiles())
                {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Tag = f;
                    newItem.Header = f.Name;
                    newItem.HeaderStringFormat = f.FullName;
                    item.Items.Add(newItem);

                    //System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(f.Name);
                    //Stream stream = new MemoryStream();
                    //icon.Save(stream);
                    //BitmapDecoder decoder = IconBitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
                    //BitmapSource src = decoder.Frames[0];
                    //pairs.Add(new NameIconPair() { Name = f.Name, IconSource = src });
                }
                //files_treeView.DataContext = pairs;

            }
            catch (Exception ex)
            { }



        }
        public class ExplorerObject
        {
            public string Name { get; set; }
            public string Date { get; set; }
            public string Type { get; set; }
            public string Size { get; set; }
        }

        private void files_treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            files_dataGrid.Items.Clear();
            DirectoryInfo dir;
            TreeViewItem tvi = (TreeViewItem)((TreeView)sender).SelectedItem;
            e.Handled = true;
            
            if (tvi.Tag is DriveInfo || tvi.Tag is DirectoryInfo)
            {
                if (tvi.Tag is DriveInfo)
                {
                    DriveInfo drive = (DriveInfo)tvi.Tag;
                    dir = drive.RootDirectory;
                    //dir_textBox.Text = dir.FullName;
                }
                else
                    dir = (DirectoryInfo)tvi.Tag;

                dir_textBox.Text = dir.FullName;

                DirectoryInfo[] dirs = null;
                //попытка получить список подпапок
                try
                {
                    dirs = dir.GetDirectories();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                //добавляем в datagrid
                foreach (DirectoryInfo subDir in dirs)
                {
                    var data = new ExplorerObject { Name = subDir.Name, Date = subDir.LastWriteTime.ToString(), Type = "Папка", Size = "" };
                    files_dataGrid.Items.Add(data);
                }

                FileInfo[] files = null;
                //попытка получить файлы
                try
                {
                    files = dir.GetFiles();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                //добавляем в datagrid
                foreach (FileInfo file in files)
                {
                    var data = new ExplorerObject { Name = file.Name, Date = file.LastWriteTime.ToString(), Type = file.Extension, Size = SizeSuffix(file.Length) };
                    files_dataGrid.Items.Add(data);
                }
            }
            else
            {
                FileInfo file = (FileInfo)tvi.Tag;
                var data = new ExplorerObject { Name = file.Name, Date = file.LastWriteTime.ToString(), Type = file.Extension, Size = SizeSuffix(file.Length) };
                files_dataGrid.Items.Add(data);
            }

            //MessageBox.Show(tvi.HeaderStringFormat.ToString());
        }

        private static string SizeSuffix(Int64 value, int decimalPlaces = 1) // добавление суффикса в зависимости от размера файла
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);

        }

        private void files_dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void copy_MenuItem_clicked(object sender, RoutedEventArgs e)
        {
            ExplorerObject row = (ExplorerObject)files_dataGrid.SelectedItem;
            string path = Path.Combine(dir_textBox.Text, row.Name);
            if (row.Type == "Папка")
            {
                copyFolderName = row.Name;
                StringCollection paths = new StringCollection();
                foreach (var s in System.IO.Directory.GetFiles(path))
                {
                    paths.Add(s);
                }
                Clipboard.SetFileDropList(paths);
                int a = 5;
            }
            else
            {
                StringCollection s = new StringCollection() { Path.Combine(dir_textBox.Text, row.Name)};
                Clipboard.SetFileDropList(s);
            }
        }

        private void rename_MenuItem_clciked(object sender, RoutedEventArgs e)
        {
           

        }

        private void delete_MenuItem_clicked(object sender, RoutedEventArgs e)
        {
           // DataGrid dg = sender as DataGrid;
            ExplorerObject row = (ExplorerObject)files_dataGrid.SelectedItem;
            string path = Path.Combine(dir_textBox.Text,row.Name);

            MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить объект?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    RecyclingBin.MoveHere(path);
                    MessageBox.Show("Перемещено в корзину!");
                    string dir_str = dir_textBox.Text;
                    getFoldersAndFiles(dir_str);
                }
                catch (System.IO.IOException ee)
                {
                    MessageBox.Show(ee.Message);
                    return;
                }
            }           
        }

        private void getFoldersAndFiles(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = null;
            DirectoryInfo[] dirs = null;
            //попытка получить список подпапок
            files_dataGrid.Items.Clear();

            try
            {
                dirs = dir.GetDirectories();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            //добавляем в datagrid
            foreach (DirectoryInfo subDir in dirs)
            {
                var data = new ExplorerObject { Name = subDir.Name, Date = subDir.LastWriteTime.ToString(), Type = "Папка", Size = "" };
                files_dataGrid.Items.Add(data);
            }

            //попытка получить файлы
            try
            {
                files = dir.GetFiles();
            }
            catch { }

            foreach (FileInfo file in files)
            {
                var data = new ExplorerObject { Name = file.Name, Date = file.LastWriteTime.ToString(), Type = file.Extension, Size = SizeSuffix(file.Length) };
                files_dataGrid.Items.Add(data);
            }
        }

        private void paste_MenuItem_clicked(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsFileDropList())
            {
                string dest_str = dir_textBox.Text;
                var returnList = Clipboard.GetFileDropList();
                if (copyFolderName!="")
                {
                    string new_dest = Path.Combine(dest_str, copyFolderName);
                    Directory.CreateDirectory(new_dest);
                   
                    foreach (var s in returnList)
                    {
                        FileInfo f = new FileInfo(s);
                        File.Copy(s, Path.Combine(new_dest, f.Name));
                    }
                }
                getFoldersAndFiles(dir_textBox.Text);
            }
        }

        private void files_dataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ExplorerObject row = (ExplorerObject)files_dataGrid.SelectedItem;
            string path = Path.Combine(dir_textBox.Text, row.Name);
            if (row.Type == "Папка")
            {
                dir_textBox.Text = path;
                getFoldersAndFiles(path);

            }
            
        }
    }
}
