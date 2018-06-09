using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace MediaPlayerApp1.Converters
{
    class TimeSpanToTimeLabelConverter : IValueConverter
    {
        private const string TimeTemplate = "{0}:{1}:{2}";
        private const string TimeTemplateSmall = "{0:00}";
        
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return "00:00:00";
            }

            if (value.GetType() == typeof(TimeSpan))
            {
                var time = (TimeSpan)value;
                return string.Format(TimeTemplate, String.Format(TimeTemplateSmall, time.Hours),
                    String.Format(TimeTemplateSmall, time.Minutes), String.Format(TimeTemplateSmall, time.Seconds));
            }
            return "00:00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
