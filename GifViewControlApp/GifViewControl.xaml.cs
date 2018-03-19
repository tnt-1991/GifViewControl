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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Globalization;
using System.ComponentModel;

namespace GifViewControlApp
{
    /// <summary>
    /// Interaction logic for GifViewControl.xaml
    /// </summary>
    public partial class GifViewControl : UserControl
    {
        private bool _isInitialized;
        private GifBitmapDecoder _gifDecoder;
        private Int32Animation _animation;

        #region dependency properties definition

        public int FrameIndex
        {
            get { return (int)base.GetValue(FrameIndexProperty); }
            set { base.SetValue(FrameIndexProperty, value); }
        }

        public Double SpeedRatio
        {
            get { return (Double)base.GetValue(SpeedRatioProperty); }
            set { base.SetValue(SpeedRatioProperty, value); }
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public string GifSourceLink
        {
            get { return (string)base.GetValue(GifSourceLinkProperty); }
            set { base.SetValue(GifSourceLinkProperty, value); }
        }

        public BitmapImage GifSource
        {
            get { return (BitmapImage)base.GetValue(GifSourceProperty); }
            set { base.SetValue(GifSourceProperty, value); }
        }

        public Stretch Stretch
        {
            get { return (Stretch)base.GetValue(StretchProperty); }
            set { base.SetValue(StretchProperty, value); }
        }

        public bool AutoStart
        {
            get { return (bool)base.GetValue(AutoStartProperty); }
            set { base.SetValue(AutoStartProperty, value); }
        }

        #endregion dependency properties definition

        #region dependency properties

        public static readonly DependencyProperty FrameIndexProperty =
           WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, int>("FrameIndex", 0, x => x.ChangingFrameIndex);

        private void ChangingFrameIndex(int oldValue, int newValue)
        {
            //setting FrameIndex to max possible in case of value overflow
            if (this.FrameIndex > this._gifDecoder.Frames.Count - 1)
            {
                this.FrameIndex = this._gifDecoder.Frames.Count - 1;
            }
            else
            {
                Source = this._gifDecoder.Frames[newValue];
            }
        }

        public static readonly DependencyProperty SpeedRatioProperty =
            WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, Double>("SpeedRatio", 1.0, x => x.ChangingSpeedRatio);

        private void ChangingSpeedRatio(Double oldValue, Double newValue)
        {
            this.SpeedRatio = newValue;
            this._isInitialized = false;
            this.Initialize();
        }

        public static readonly DependencyProperty SourceProperty =
            WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, ImageSource>("Source", null, x => x.SourcePropertyChanged);

        private void SourcePropertyChanged(ImageSource oldValue, ImageSource newValue)
        {
            Source = newValue;
        }

        public static readonly DependencyProperty AutoStartProperty =
           WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, bool>("AutoStart", false, x => x.AutoStartPropertyChanged);

        private void AutoStartPropertyChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                this.StartAnimation();
            }
        }

        public static readonly DependencyProperty GifSourceProperty =
            WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, BitmapImage>("GifSource", new BitmapImage(new Uri("Images/default.gif", UriKind.Relative)), x => x.GifSourcePropertyChanged);

        private void GifSourcePropertyChanged(BitmapImage oldValue, BitmapImage newValue)
        {
            this.Initialize();
        }

        public static readonly DependencyProperty GifSourceLinkProperty =
            WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, string>("GifSourceLink", string.Empty, x => x.GifSourceLinkPropertyChanged);

        private void GifSourceLinkPropertyChanged(string oldValue, string newValue)
        {
            this.Initialize();
        }

        public static DependencyProperty StretchProperty =
            WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, Stretch>("Stretch", Stretch.Fill, x => x.StretchPropertyChanged);

        private void StretchPropertyChanged(Stretch oldValue, Stretch newValue)
        {
            this.Initialize();
        }

        private void VisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Visibility)e.NewValue == Visibility.Visible)
            {
                this.StartAnimation();
            }
            else
            {
                this.StopAnimation();
            }
        }

        #endregion dependency properties

        #region init

        public GifViewControl()
        {
            InitializeComponent();
            VisibilityProperty.OverrideMetadata(typeof(GifViewControl), new FrameworkPropertyMetadata(VisibilityPropertyChanged));
        }

        private void Initialize()
        {
            if (!_isInitialized)
            {
                Uri uri = GetUri();
                _gifDecoder = new GifBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                _animation = new Int32Animation(0, _gifDecoder.Frames.Count - 1, new Duration(TimeSpan.FromSeconds(_gifDecoder.Frames.Count / 10.0)));
                _animation.SpeedRatio = SpeedRatio;
                _animation.RepeatBehavior = RepeatBehavior.Forever;

                Source = _gifDecoder.Frames[0];
                CreateImageControl();

                _isInitialized = true;
            }
        }

        /// <summary>
        /// As Uri will be used:
        ///  - GifSourceLink (string) if defined
        ///  - GifSource (BitmapImage) if defined, GifSourceLink is not defined
        ///  - default.gif if GifSource and GifSourceLink are not defined
        /// </summary>
        private Uri GetUri()
        {
            Uri uri;
            if (!String.IsNullOrEmpty(GifSourceLink))
            {
                uri = new Uri(GifSourceLink, UriKind.Relative);
            }
            else
            {
                uri = GifSource.UriSource;
            }

            return uri;
        }

        private void CreateImageControl()
        {
            Image img = new Image();
            img.Stretch = Stretch;

            Binding sourceBinding = new Binding();
            sourceBinding.Source = this;
            sourceBinding.Path = new PropertyPath("Source");
            sourceBinding.Mode = BindingMode.TwoWay;
            sourceBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(img, Image.SourceProperty, sourceBinding);

            main.Children.Add(img);
        }

        #endregion init

        #region methods

        public void StartAnimation()
        {
            if (!_isInitialized)
            {
                this.Initialize();
            }

            base.BeginAnimation(FrameIndexProperty, _animation);
        }

        public void StopAnimation()
        {
            base.BeginAnimation(FrameIndexProperty, null);
        }

        #endregion methods
    }

    internal static class WpfUtils
    {
        public static DependencyProperty RegisterDependencyPropertyWithCallback<TObject, TProperty>(string propertyName, TProperty defaultValue,
            Func<TObject, Action<TProperty, TProperty>> getOnChanged)
            where TObject : DependencyObject
        {
            return DependencyProperty.Register(
                propertyName,
                typeof(TProperty),
                typeof(TObject),
                new PropertyMetadata(defaultValue, new PropertyChangedCallback((d, e) =>
                    getOnChanged((TObject)d)((TProperty)e.OldValue, (TProperty)e.NewValue)
                ))
            );
        }
    }
}