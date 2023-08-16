using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GPSpeedView.Converters
{
    public class OrderByToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sortType =  (SortType)value;
            if(sortType == SortType.ACCER)
            {
                if(parameter.ToString() == "ACCER")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (parameter.ToString() == "ACCER")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sortType = (bool)value;
            if (sortType)
            {
                if (parameter.ToString() == "ACCER")
                {
                    return SortType.ACCER;
                }
                else
                {
                    return SortType.ACCERInFive;
                }
            }
            else
            {
                if (parameter.ToString() == "ACCER")
                {
                    return SortType.ACCERInFive;
                }
                else
                {
                    return SortType.ACCER;
                }
            }
        }
    }

    public class OrderByToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sortType = (SortType)value;
            if (sortType == SortType.ACCER)
            {
                if (parameter.ToString() == "ACCER")
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            else
            {
                if (parameter.ToString() == "ACCER")
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
