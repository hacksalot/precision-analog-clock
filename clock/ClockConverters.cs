using System;
using System.Windows.Data;
using System.Globalization;

namespace OCDClock
{



    /// <summary>
    /// Convert milliseconds to a clock angle.
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(double))]
    public class MilliSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            return date.Second * 6 + (date.Millisecond * .006);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }



    /// <summary>
    /// Convert seconds to a clock angle.
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(int))]
    public class SecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ClockControl c = parameter as ClockControl;
            DateTime date = (DateTime)value;
            return date.Second * 6;// (date.Millisecond * .006);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }



    /// <summary>
    /// Convert minutes to a clock angle.
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(int))]
    public class MinutesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            return date.Minute * 6 + (date.Second * .1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }



    /// <summary>
    /// Convert hours to a clock angle.
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(int))]
    public class HoursConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            return (date.Hour * 30) + (date.Minute / 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }



}
