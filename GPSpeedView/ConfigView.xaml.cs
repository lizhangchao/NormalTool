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
    /// ConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigView : Window
    {
        public ConfigView()
        {
            InitializeComponent();
            InitData();
        }

        private void InitData()
        {
            MaxPriceBox.Text = ConfigData.MaxPrice.ToString();
            MinAllBox.Text = ConfigData.MinMarkValue.ToString();
            MinACCER.Text = ConfigData.MinACCER.ToString();

            Box60.IsChecked = ConfigData.Show60GP;
            Box00.IsChecked = ConfigData.Show00GP;
            Box30.IsChecked = ConfigData.Show30GP;
            Box43.IsChecked = ConfigData.Show43GP;
            BoxST.IsChecked = ConfigData.ShowSTGP;

            MinMarkUpLocalBox.Text = ConfigData.MinMarkUpForLocal.ToString();
            StartHourBox.Text = ConfigData.GPStartRecordTime.Hours.ToString();
            StartMinuBox.Text = ConfigData.GPStartRecordTime.Minutes.ToString();
            StartSecondBox.Text = ConfigData.GPStartRecordTime.Seconds.ToString();
            EndHourBox.Text = ConfigData.GPEndRecordTime.Hours.ToString();
            EndMinuBox.Text = ConfigData.GPEndRecordTime.Minutes.ToString();
            EndSecondBox.Text = ConfigData.GPEndRecordTime.Seconds.ToString();
            MinMarkUpForMidFastBox.Text = ConfigData.MinMarkUpForMidFast.ToString();

            IsMinMarkUpLocalEnableCheck.IsChecked = ConfigData.IsMinMarkUpForLocalEnable;
            IsFilterCheck.IsChecked = ConfigData.IsFilterGps;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(MaxPriceBox.Text))
            {
                ConfigData.MaxPrice = double.TryParse(MaxPriceBox.Text,out double maxPrice)? maxPrice : ConfigData.MaxPrice;
            }
            if (!string.IsNullOrEmpty(MinAllBox.Text))
            {
                ConfigData.MinMarkValue = double.TryParse(MinAllBox.Text, out double minAll) ? minAll : ConfigData.MinMarkValue;
            }
            if (!string.IsNullOrEmpty(MinACCER.Text))
            {
                ConfigData.MinACCER = double.TryParse(MinACCER.Text, out double minAccer) ? minAccer : ConfigData.MinACCER;
            }

            ConfigData.Show60GP = Box60.IsChecked == true;
            ConfigData.Show00GP = Box00.IsChecked == true;
            ConfigData.Show30GP = Box30.IsChecked == true;
            ConfigData.Show43GP = Box43.IsChecked == true;
            ConfigData.ShowSTGP = BoxST.IsChecked == true;

            if (!string.IsNullOrEmpty(MinMarkUpLocalBox.Text))
            {
                ConfigData.MinMarkUpForLocal = double.TryParse(MinMarkUpLocalBox.Text, out double minMarkUpForLocal) ? minMarkUpForLocal : ConfigData.MinMarkUpForLocal;
            }
            if (!string.IsNullOrEmpty(StartHourBox.Text) && !string.IsNullOrEmpty(StartMinuBox.Text) && !string.IsNullOrEmpty(StartSecondBox.Text))
            {
                int hour = int.TryParse(StartHourBox.Text.ToString(), out int tempHour) ? tempHour : ConfigData.GPStartRecordTime.Hours;
                int minu = int.TryParse(StartMinuBox.Text.ToString(), out int tempMinu) ? tempMinu : ConfigData.GPStartRecordTime.Minutes;
                int second = int.TryParse(StartSecondBox.Text.ToString(), out int tempSecond) ? tempSecond : ConfigData.GPStartRecordTime.Seconds;
                ConfigData.GPStartRecordTime = new TimeSpan(hour, minu, second);
            }
            if (!string.IsNullOrEmpty(EndHourBox.Text) && !string.IsNullOrEmpty(EndMinuBox.Text) && !string.IsNullOrEmpty(EndSecondBox.Text))
            {
                int hour = int.TryParse(EndHourBox.Text.ToString(), out int tempHour) ? tempHour : ConfigData.GPEndRecordTime.Hours;
                int minu = int.TryParse(EndMinuBox.Text.ToString(), out int tempMinu) ? tempMinu : ConfigData.GPEndRecordTime.Minutes;
                int second = int.TryParse(EndSecondBox.Text.ToString(), out int tempSecond) ? tempSecond : ConfigData.GPEndRecordTime.Seconds;
                ConfigData.GPEndRecordTime = new TimeSpan(hour, minu, second);
            }

            ConfigData.IsMinMarkUpForLocalEnable = IsMinMarkUpLocalEnableCheck.IsChecked == true;

            if (!string.IsNullOrEmpty(MinMarkUpForMidFastBox.Text))
            {
                ConfigData.MinMarkUpForMidFast = double.TryParse(MinMarkUpForMidFastBox.Text, out double minMarkUpForMidFast) ? minMarkUpForMidFast : ConfigData.MinMarkUpForMidFast;
            }
            ConfigData.IsFilterGps = IsFilterCheck.IsChecked == true;

        }
    }
}
