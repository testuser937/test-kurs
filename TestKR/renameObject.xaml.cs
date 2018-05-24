using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.IO;

namespace TestKR
{
    /// <summary>
    /// Логика взаимодействия для renameObject.xaml
    /// </summary>
    public partial class renameObject : Window
    {
        public renameObject()
        {
            InitializeComponent();
            //newname_TextBox.SelectionStart = 0;
            //newname_TextBox.SelectionLength = newname_TextBox.Text.Length;
            newname_TextBox.SelectAll(); 
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            
            
        }
    }
}
