using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.IO;

namespace Controls
{
    //GifViewControl as extension of Image class, no XAML needed, direct binding to base.Source
    internal class GifViewControl : Image
    {
        #region properties

        private bool _isInitialized;
        private GifBitmapDecoder _gifDecoder;
        private Int32Animation _animation;

        #endregion properties

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

        public Stream GifSource
        {
            get { return (Stream)base.GetValue(GifSourceProperty); }
            set { base.SetValue(GifSourceProperty, value); }
        }

        public Stretch StretchView
        {
            get { return (Stretch)base.GetValue(StretchViewProperty); }
            set { base.SetValue(StretchViewProperty, value); }
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

        private void ChangingFrameIndex(DependencyObject dependencyObject, int oldValue, int newValue)
        {
            //setting FrameIndex to max possible in case of value overflow
            if (this.FrameIndex > this._gifDecoder.Frames.Count - 1)
            {
                this.FrameIndex = this._gifDecoder.Frames.Count - 1;
            }
            else
            {
                base.Source = this._gifDecoder.Frames[newValue];
            }
        }

        public static readonly DependencyProperty SpeedRatioProperty =
        WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, Double>("SpeedRatio", 1.0, x => x.ChangingSpeedRatio);

        private void ChangingSpeedRatio(DependencyObject dependencyObject, Double oldValue, Double newValue)
        {
            this.SpeedRatio = newValue;
            this._isInitialized = false;
            this.Initialize();
        }

        public static readonly DependencyProperty AutoStartProperty =
               WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, bool>("AutoStart", false, x => x.AutoStartPropertyChanged);

        private void AutoStartPropertyChanged(DependencyObject dependencyObject, bool oldValue, bool newValue)
        {
            if (newValue)
            {
                this.StartAnimation();
            }
        }

        public static readonly DependencyProperty GifSourceProperty =
            WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, Stream>("GifSource", null, x => x.GifSourcePropertyChanged);

        private void GifSourcePropertyChanged(DependencyObject dependencyObject, Stream oldValue, Stream newValue)
        {
            _isInitialized = false;
            this.Initialize();
        }

        public static DependencyProperty StretchViewProperty =
            WpfUtils.RegisterDependencyPropertyWithCallback<GifViewControl, Stretch>("Stretch", Stretch.Fill, x => x.StretchViewPropertyChanged);

        private void StretchViewPropertyChanged(DependencyObject dependencyObject, Stretch oldValue, Stretch newValue)
        {
            _isInitialized = false;
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

        #region public methods

        public GifViewControl()
        {
        }

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

        #endregion public methods

        #region private methods

        private void Initialize()
        {
            if (!_isInitialized)
            {
                _gifDecoder = new GifBitmapDecoder(GifSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                _animation = new Int32Animation(0, _gifDecoder.Frames.Count - 1, new Duration(TimeSpan.FromSeconds(_gifDecoder.Frames.Count / 10.0)));
                _animation.SpeedRatio = SpeedRatio;
                _animation.RepeatBehavior = RepeatBehavior.Forever;

                base.Source = _gifDecoder.Frames[0];

                _isInitialized = true;
            }
        }

        #endregion private methods
    }

    internal static class WpfUtils
    {
        public static DependencyProperty RegisterDependencyPropertyWithCallback<TObject, TProperty>(string propertyName, TProperty defaultValue,
            Func<TObject, Action<TObject, TProperty, TProperty>> getOnChanged)
            where TObject : DependencyObject
        {
            return DependencyProperty.Register(
                propertyName,
                typeof(TProperty),
                typeof(TObject),
                new PropertyMetadata(defaultValue, new PropertyChangedCallback((d, e) =>
                    getOnChanged((TObject)d)((TObject)d, (TProperty)e.OldValue, (TProperty)e.NewValue)
                ))
            );
        }
    }
}