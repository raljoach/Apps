using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Earthquake
{
    public class IntegerBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // NB: not properly checking types.  
            Earthquake eq = (Earthquake)value;

            Brush brush = eq.magnitude >= 8 ? this.PositiveBrush : this.NegativeBrush;
            return (brush);
        }
        public Brush PositiveBrush
        {
            get
            {
                return (this.positiveBrush);
            }
            set
            {
                this.positiveBrush = value;
            }
        }
        private Brush positiveBrush;

        public Brush NegativeBrush
        {
            get
            {
                return (this.negativeBrush);
            }
            set
            {
                this.negativeBrush = value;
            }
        }
        private Brush negativeBrush;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }  
}
