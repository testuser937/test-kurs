using Shell32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        public static Shell shell = new Shell();
        public static Folder RecyclingBin = shell.NameSpace(10);

        public Stack<string> back_stack = new Stack<string>();
        public Stack<string> forward_stack = new Stack<string>();
        public MainWindow()
        {
            InitializeComponent();
            getDrives(); // загрузка списка дисков
            back_btn.IsEnabled = false;
            forward_btn.IsEnabled = false;
            paste_Item.IsEnabled = false;

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
                }
                else
                    dir = (DirectoryInfo)tvi.Tag;

                back_stack.Push(dir_textBox.Text);
                forward_stack.Clear();
                forward_btn.IsEnabled = false;

                dir_textBox.Text = dir.FullName;

                
                back_btn.IsEnabled = true;

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
            //files_dataGrid.Focus();
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

        public void Paste(string dest)
        {
            foreach (string fileSystemObject in Clipboard.GetFileDropList())
            {
                CopyFileSystemObject(fileSystemObject, dest);
            }
        }
        private void CopyFileSystemObject(string fileSystemObject, string destinationDirectory)
        {
            if (Directory.Exists(fileSystemObject))
            {
                string l_directoryName = Path.Combine(destinationDirectory, Path.GetFileName(fileSystemObject));
                if (!Directory.Exists(l_directoryName))
                {
                    Directory.CreateDirectory(l_directoryName);
                }
                foreach (string l_fileSystemObject in Directory.GetFileSystemEntries(fileSystemObject))
                {
                    CopyFileSystemObject(l_fileSystemObject, l_directoryName);
                }
            }
            else
            {
                File.Copy(fileSystemObject, Path.Combine(destinationDirectory, Path.GetFileName(fileSystemObject)), true);
            }
        }



        private void getFoldersAndFiles(string path)
        {
            files_dataGrid.Items.Clear();
            if (path == "")
            {
                return;
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] files = null;
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
                files_dataGrid.SelectedIndex = 0;
                files_dataGrid.Focus();
            }
        }

        private void files_dataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ExplorerObject row = (ExplorerObject)files_dataGrid.SelectedItem;
            string path = Path.Combine(dir_textBox.Text, row.Name);
            if (row.Type == "Папка")
            {
                back_stack.Push(dir_textBox.Text);
                dir_textBox.Text = path;
                getFoldersAndFiles(path);
            }
            
        }

        private void back_btn_Click(object sender, RoutedEventArgs e)
        {
           if (back_stack.Count != 0)
            {
                forward_stack.Push(dir_textBox.Text);
                string path = back_stack.Pop();

                dir_textBox.Text = path;
                getFoldersAndFiles(path);                
                forward_btn.IsEnabled = true;
                if (back_stack.Count == 0)
                {
                    back_btn.IsEnabled = false;
                }
            }
            else
            {
                back_btn.IsEnabled = false;
            }

        }

        private void forward_btn_Click(object sender, RoutedEventArgs e)
        {
            if (forward_stack.Count != 0)
            {
                back_stack.Push(dir_textBox.Text);
                string path = forward_stack.Pop();
                dir_textBox.Text = path;
                files_dataGrid.Items.Clear();
                getFoldersAndFiles(path);
                //forward_btn.IsEnabled = false;
                back_btn.IsEnabled = true;
                if (forward_stack.Count == 0)
                {
                    forward_btn.IsEnabled = false;
                }
            }
            else
            {
                forward_btn.IsEnabled = false;
            }
        }

        private void files_dataGrid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ExplorerObject row = (ExplorerObject)files_dataGrid.SelectedItem;
            if (e.Key == System.Windows.Input.Key.Space && row.Type == "Папка")
            {
                forward_stack.Clear();
                back_btn.IsEnabled = true;
                back_stack.Push(dir_textBox.Text);
                getFoldersAndFiles(Path.Combine(dir_textBox.Text, row.Name));
                dir_textBox.Text = Path.Combine(dir_textBox.Text, row.Name);
                //if (back_stack.Count == 0)
                //{
                //    back_btn.IsEnabled = false;
                //}
            }
            if(e.Key == System.Windows.Input.Key.Back && back_stack.Count != 0)
            {
                forward_btn.IsEnabled = true;
                forward_stack.Push(dir_textBox.Text);
                string path = back_stack.Pop();
                getFoldersAndFiles(path);
                dir_textBox.Text = path;
                if (back_stack.Count == 0)
                {
                    back_btn.IsEnabled = false;
                }
            }
            if(e.Key == System.Windows.Input.Key.F5)
            {
                getFoldersAndFiles(dir_textBox.Text);
            }
        }

        private void copy_Menu(object sender, RoutedEventArgs e)
        {
            ExplorerObject row = (ExplorerObject)files_dataGrid.SelectedItem;
            if (row != null)
            {
                string path = Path.Combine(dir_textBox.Text, row.Name);
                StringCollection paths = new StringCollection();
                paths.Add(path);
                Clipboard.SetFileDropList(paths);
                paste_Item.IsEnabled = true;
            }
        }

        private void paste_Menu(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsFileDropList())
            {
                string dest_str = dir_textBox.Text;
                var returnList = Clipboard.GetFileDropList();
                Paste(dest_str);
                getFoldersAndFiles(dir_textBox.Text);
            }
        }

        private void delete_Menu(object sender, RoutedEventArgs e)
        {
            ExplorerObject row = (ExplorerObject)files_dataGrid.SelectedItem;
            if (row != null)
            {
                string path = Path.Combine(dir_textBox.Text, row.Name);
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
                    catch (IOException ee)
                    {
                        MessageBox.Show(ee.Message);
                        return;
                    }
                }
            }
        }

        private void rename_Menu(object sender, RoutedEventArgs e)
        {
            ExplorerObject row = (ExplorerObject)files_dataGrid.SelectedItem;
            if (row != null)
            {
                renameObject a = new renameObject();
                a.newname_TextBox.SelectionStart = 0;
                a.newname_TextBox.SelectionLength = a.newname_TextBox.Text.Length;
                a.newname_TextBox.SelectAll();
                a.newname_TextBox.Text = row.Name;
                a.btn.Click += (s, ee) =>
                {
                    try
                    {
                        if (row.Type != "Папка")
                        {

                            File.Move(Path.Combine(dir_textBox.Text, row.Name), Path.Combine(dir_textBox.Text, a.newname_TextBox.Text));


                        }
                        else
                        {
                            Directory.Move(Path.Combine(dir_textBox.Text, row.Name), Path.Combine(dir_textBox.Text, a.newname_TextBox.Text));


                        }
                        a.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    getFoldersAndFiles(dir_textBox.Text);

                };
                a.Show();
            }
        }
    }
}

