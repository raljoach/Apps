using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace Earthquake
{
    public class DecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {

                //return System.Convert.ToDouble(value).ToString(parameter as string);
                return string.Format(parameter as string, value);

            }

            catch (Exception)
            {

            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
