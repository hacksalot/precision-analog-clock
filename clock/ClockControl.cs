using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;
using System.Linq;

namespace OCDClock
{
    /// <summary>
    /// ========================================
    /// WinFX Custom Control
    /// ========================================
    ///
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Untamed.MMA.View.WPF.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Untamed.MMA.View.WPF.Controls;assembly=Untamed.MMA.View.WPF.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file. Note that Intellisense in the
    /// XML editor does not currently work on custom controls and its child elements.
    ///
    ///     <MyNamespace:Clock/>
    ///
    /// </summary>

    public class ClockControl : Control
    {



        /// <summary>
        /// Static constructor. Initialize the theme's default style. See generic.xaml.
        /// </summary>
        static ClockControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClockControl), new FrameworkPropertyMetadata(typeof(ClockControl)));
        }



        /// <summary>
        /// Initialize the clock control. Create and start the timer.
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitTimer(IsRunning);
            _hourFont = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
            if (!_hourFont.TryGetGlyphTypeface(out _hourFontGlyph))
                throw new InvalidOperationException("No glyphtypeface found");
        }



        /// <summary>
        /// Initialize the timer used to drive the clock.
        /// </summary>
        protected void InitTimer(bool create)
        {
            if(create && _timer == null)
            { 
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(10);
                //timer.Interval = TimeSpan.FromMilliseconds(1000);
                //timer.Interval = TimeSpan.FromMilliseconds(100 - DateTime.Now.Millisecond / 10);
                _timer.Tick += new EventHandler(Timer_Tick);
                _timer.Start();
            }
            else if(!create && _timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                _timer.Interval = TimeSpan.Zero;
                _timer = null;
            }
        }



        /// <summary>
        /// Called after the control template is applied. Here we can manipulate
        /// the control template on the fly as desired. For now let's assign
        /// positions to twelve TextBlock objects representing the numerals on 
        /// the clock face. We could as well do this in the XAML, but this way 
        /// we capture the positioning logic in code, rather than the output
        /// of that positioning logic in XAML.
        /// </summary>
        public override void OnApplyTemplate()
        {
            DrawingGroup dg = (DrawingGroup)this.Template.FindName("clockGlyphsContainer", this);
            var labels = dg.Children.OfType<GlyphRunDrawing>();
            double innerOffset = (50 - NumeralRadius) + 1;
            double innerCircleDiameter = NumeralRadius * 2;
            double fontSizeDIP = this.FontSize;
            double fontSizePt = fontSizeDIP * (72.0 / 96.0);
            int index = 1;

            foreach(GlyphRunDrawing grd in labels)
            {
                // Get the point where the numeral should be drawn
                Point pt = GetHourPosition(index);

                // Measure how much space the numeral will take
                string strHour = index.ToString();
                Size sz = MeasureString(strHour, fontSizeDIP);

                // Now adjust the position based on where on the clock face
                // the numeral falls, interpolating such that, at the far
                // end of the clock, the positions are pulled inward (to the
                // left and up) reflecting the right- and/or bottom-alignment
                // of numerals on the far sides of the clock.
                pt.X -= ((pt.X - innerOffset) / innerCircleDiameter) * sz.Width;
                pt.Y += (1.0 - ((pt.Y - innerOffset) / innerCircleDiameter)) * sz.Height;

                // Create the low-level glyph run for the text.
                grd.GlyphRun = CreateGlyphRun(strHour, pt, fontSizeDIP);
                grd.ForegroundBrush = Brushes.DarkRed;
                index++;
            }

        }




        /// <summary>
        /// Compute the formal position of the specified hour on the clock face
        /// using the parametric equation for a circle and the equation for the
        /// angle of the hour hand.
        /// </summary>
        /// <param name="hour">The hour. A value from 1 to 12 inclusive.</param>
        /// <returns>The position of the upper-left point of where the hour glyph
        /// should be drawn.</returns>
        public Point GetHourPosition(int hour)
        {
            // The long way:
            // double angle = .5 * ((60.0 * hour) + 0)
            // (eg: double angle = 30.0 * hour)
            // double rads = (Math.PI / 180) * angle;
            // double r = 38; // inner circle radius
            // double cx = 50; // circle center (x)
            // double cy = 50; // circle center (y)
            // return new Point(cx + r * Math.Cos(rads), cy + r * Math.Sin(rads));

            // The short way:
            double angle = (30.0 * hour) - 90;
            double rads = (Math.PI / 180) * angle;
            return new Point((50 + NumeralRadius * Math.Cos(rads)), (50 + NumeralRadius * Math.Sin(rads)));
        }



        /// <summary>
        /// Quick 'n dirty string measurement.
        /// </summary>
        private Size MeasureString(string str, double fontSize)
        {
            Size sz = new Size();
            for(int c = 0; c < str.Length; c++)
            { 
                ushort glyph = _hourFontGlyph.CharacterToGlyphMap[ str[c] ];
                sz.Width += _hourFontGlyph.AdvanceWidths[glyph] * fontSize;
                sz.Height = Math.Max(_hourFontGlyph.AdvanceHeights[glyph] * (fontSize * (72.0 / 96.0)), sz.Height);
            }
            return sz;
        }


        /// <summary>
        /// Handler for the timer's Tick event.
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            if(!IsDiscrete || now.Second != _lastTick.Second)
                UpdateDateTime(now);
        }



        /// <summary>
        /// Update this.DateTime to the current time.
        /// </summary>
        private void UpdateDateTime(DateTime newVal)
        {
            _lastTick = newVal;
            this.DateTime = (IsDiscrete) ? newVal.AddMilliseconds(-newVal.Millisecond) : newVal;
        }



        /// <summary>
        /// Get or set the clock's current time.
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                return (DateTime)GetValue(DateTimeProperty);
            }
            set
            {
                SetValue(DateTimeProperty, value);
            }
        }



        /// <summary>
        /// Get or set whether the clock operates in discrete or continuous mode.
        /// </summary>
        public bool IsDiscrete
        {
            get
            {
                return (bool)GetValue(IsDiscreteProperty);
            }
            set
            {
                SetValue(IsDiscreteProperty, value);
            }
        }



        /// <summary>
        /// Get or set whether the clock is running or frozen.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return (bool)GetValue(IsRunningProperty);
            }
            set
            {
                SetValue(IsRunningProperty, value);
            }
        }



        /// <summary>
        /// Get or set whether the clock is running or frozen.
        /// </summary>
        public double NumeralRadius
        {
            get
            {
                return (double)GetValue(NumeralRadiusProperty);
            }
            set
            {
                SetValue(NumeralRadiusProperty, value);
            }
        }



        /// <summary>
        /// Register the "DateTime" property as a formal dependency property.
        /// </summary>
        public static DependencyProperty DateTimeProperty = DependencyProperty.Register(
                "DateTime",
                typeof(DateTime),
                typeof(ClockControl),
                new PropertyMetadata(DateTime.Now, new PropertyChangedCallback(OnDateTimeInvalidated)));



        /// <summary>
        /// Register the "IsDiscrete" property as a formal dependency property.
        /// </summary>
        public static DependencyProperty IsDiscreteProperty = DependencyProperty.Register(
                "IsDiscrete",
                typeof(bool),
                typeof(ClockControl),
                new PropertyMetadata(true, new PropertyChangedCallback(OnIsDiscreteInvalidated)));



        /// <summary>
        /// Register the "IsRunning" property as a formal dependency property.
        /// </summary>
        public static DependencyProperty IsRunningProperty = DependencyProperty.Register(
                "IsRunning",
                typeof(bool),
                typeof(ClockControl),
                new PropertyMetadata(true, new PropertyChangedCallback(OnIsRunningInvalidated)));



        /// <summary>
        /// Register the "NumeralRadius" property as a formal dependency property.
        /// </summary>
        public static DependencyProperty NumeralRadiusProperty = DependencyProperty.Register(
                "NumeralRadius",
                typeof(double),
                typeof(ClockControl),
                new PropertyMetadata(36.0));



        /// <summary>
        /// Set up a DateTimeChanged event.
        /// </summary>
        public static readonly RoutedEvent DateTimeChangedEvent = 
            EventManager.RegisterRoutedEvent( 
                "DateTimeChanged", 
                RoutingStrategy.Bubble, 
                typeof(RoutedPropertyChangedEventHandler<DateTime>), 
                typeof(ClockControl));



        /// <summary>
        /// Set up an DiscreteChanged event.
        /// </summary>
        public static readonly RoutedEvent DiscreteChangedEvent =
            EventManager.RegisterRoutedEvent(
                "DiscreteChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<bool>),
                typeof(ClockControl));



        /// <summary>
        /// Set up an RunningChanged event.
        /// </summary>
        public static readonly RoutedEvent RunningChangedEvent =
            EventManager.RegisterRoutedEvent(
                "RunningChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<bool>),
                typeof(ClockControl));



        /// <summary>
        /// Fire the DateTimeChanged event when the time changes.
        /// </summary>
        protected virtual void OnDateTimeChanged(DateTime oldValue, DateTime newValue)
        {
            RoutedPropertyChangedEventArgs<DateTime> args = new RoutedPropertyChangedEventArgs<DateTime>(oldValue, newValue);
            args.RoutedEvent = ClockControl.DateTimeChangedEvent;
            RaiseEvent(args);
        }



        /// <summary>
        /// Fire the IsDiscreteChanged event when the motion type changes.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected virtual void OnIsDiscreteChanged(bool oldValue, bool newValue)
        {
            RoutedPropertyChangedEventArgs<bool> args = new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue);
            args.RoutedEvent = ClockControl.DiscreteChangedEvent;
            RaiseEvent(args);
        }



        /// <summary>
        /// Fire the OnIsRunningChanged event when the motion type changes.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected virtual void OnIsRunningChanged(bool oldValue, bool newValue)
        {
            InitTimer(newValue);
            RoutedPropertyChangedEventArgs<bool> args = new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue);
            args.RoutedEvent = ClockControl.RunningChangedEvent;
            RaiseEvent(args);
        }



        /// <summary>
        /// Will be called every time the ClockControl.DateTime property changes.
        /// </summary>
        private static void OnDateTimeInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ClockControl clock = (ClockControl)d;

            DateTime oldValue = (DateTime)e.OldValue;
            DateTime newValue = (DateTime)e.NewValue;

            if (oldValue.Second != newValue.Second) {
                clock.OnDateTimeChanged(oldValue, newValue.AddMilliseconds(-newValue.Millisecond));
            }
        }



        /// <summary>
        /// Will be called every time the ClockControl.IsDiscrete property changes.
        /// </summary>
        private static void OnIsDiscreteInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ClockControl clock = (ClockControl)d;

            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;

            if(oldValue != newValue)
                clock.OnIsDiscreteChanged(oldValue, newValue);
        }



        /// <summary>
        /// Will be called every time the ClockControl.IsRunning property changes.
        /// </summary>
        private static void OnIsRunningInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ClockControl clock = (ClockControl)d;

            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;

            if(oldValue != newValue)
                clock.OnIsRunningChanged(oldValue, newValue);
        }



        /// <summary>
        /// Create a GlyphRun object corresponding to the specified text, at the
        /// specified location and size.
        /// </summary>
        private GlyphRun CreateGlyphRun(string text, Point origin, double fontSize)
        {
            ushort[] glyphIndexes = new ushort[text.Length];
            double[] advanceWidths = new double[text.Length];

            for (int n = 0; n < text.Length; n++)
            {
                ushort glyphIndex = _hourFontGlyph.CharacterToGlyphMap[text[n]];
                glyphIndexes[n] = glyphIndex;
                double width = _hourFontGlyph.AdvanceWidths[glyphIndex] * fontSize;
                advanceWidths[n] = width;
            }

            GlyphRun glyphRun = new GlyphRun(_hourFontGlyph, 0, false, fontSize,
                glyphIndexes, origin, advanceWidths, null, null, null, null,
                null, null);

            return glyphRun;
        }



        /// <summary>
        /// Timer used to drive the clock.
        /// </summary>
        DispatcherTimer     _timer = null;
        


        /// <summary>
        /// Cache the clock's "last tick time".
        /// </summary>
        DateTime            _lastTick = DateTime.Now;


        /// <summary>
        /// Radius of the circle used to position numerals on the clock face.
        /// </summary>
        //double              _numeralRadius = 36;


        /// <summary>
        /// Background brush for clock geometry.
        /// </summary>
        SolidColorBrush     _brush = new SolidColorBrush(Color.FromRgb(64, 64, 64));



        /// <summary>
        /// Default font for the clock face.
        /// </summary>
        Typeface            _hourFont = null;


        /// <summary>
        /// A glyph-friendly typeface for rendering the actual clock glyphs.
        /// </summary>
        GlyphTypeface       _hourFontGlyph = null;
    }


}
