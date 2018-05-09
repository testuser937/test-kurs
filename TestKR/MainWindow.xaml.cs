using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace TestKR
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

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
            else
                dir = (DirectoryInfo)item.Tag;
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
                }

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
                }
                else
                    dir = (DirectoryInfo)tvi.Tag;

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

        //TODO написать обработчики контекстного меню
        private void copy_MenuItem_clicked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Скопировано!\n(На самом деле нет)");
        }

        private void rename_MenuItem_clciked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Переименовано\n(На самом деле нет)");
        }

        private void delete_MenuItem_clicked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Удалено!\n(На самом деле нет)");
        }
    }
}
