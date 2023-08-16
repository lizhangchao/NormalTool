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

namespace GPSpeedView
{
    /// <summary>
    /// RemoveListView.xaml 的交互逻辑
    /// </summary>
    public partial class RemoveListView : Window
    {
        public RemoveListView()
        {
            InitializeComponent();
            removeListBox.ItemsSource = ConfigData.FlagGps;
        }

        private void removeListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(removeListBox.SelectedItem != null)
            {
                ConfigData.FlagGps.RemoveAll(x => x == removeListBox.SelectedItem.ToString());
            }
            removeListBox.ItemsSource = null;
            removeListBox.ItemsSource = ConfigData.FlagGps;
        }
    }
}
