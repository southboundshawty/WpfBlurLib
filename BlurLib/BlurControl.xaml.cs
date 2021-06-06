using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlurLib
{
    /// <summary>
    /// Логика взаимодействия для BlurControl.xaml
    /// </summary>
    public partial class BlurControl : UserControl
    {
        public BlurControl()
        {
            InitializeComponent();
        }

        private Visual _parentVisual;
        private bool _blurOn = false;

        public static readonly DependencyProperty BackgroundObjectToBlurProperty = DependencyProperty.Register(
            "BackgroundObjectToBlur", typeof(Visual), typeof(BlurControl), new PropertyMetadata(default(Visual), BackgroundObjectToBlurPropertyChangedCallback));

        private static void BackgroundObjectToBlurPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not BlurControl blurControl)
                return;

            if (e.NewValue is not Visual newValue)
                return;

            blurControl.BackgroundObjectToBlur = newValue;
        }

        public Visual BackgroundObjectToBlur
        {
            get => (Visual)GetValue(BackgroundObjectToBlurProperty);
            set => SetValue(BackgroundObjectToBlurProperty, value);
        }

        public static readonly DependencyProperty ForceBlurProperty = DependencyProperty.Register(
            "ForceBlur", typeof(bool), typeof(BlurControl), new PropertyMetadata(default(bool), ForceBlurPropertyChangedCallback));

        private static void ForceBlurPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not BlurControl blurControl)
                return;

            if (e.NewValue is not bool newValue)
                return;

            if (newValue)
                blurControl.OnBlur(null);
            else 
                blurControl.OffBlur();

            blurControl.ForceBlur = newValue;
        }

        public bool ForceBlur
        {
            get => (bool)GetValue(ForceBlurProperty);
            set => SetValue(ForceBlurProperty, value);
        }

        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(ContentControl), typeof(BlurControl), new PropertyMetadata(default(ContentControl), ContentPropertyChangedCallback));

        private static void ContentPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not BlurControl blurControl)
                return;

            if (e.NewValue is not ContentControl newValue)
                return;

            blurControl.MyContentControl.Content = newValue;
        }

        public new ContentControl Content
        {
            get => (ContentControl)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register(
            "BlurRadius", typeof(double), typeof(BlurControl), new PropertyMetadata(default(double), BlurRadiusPropertyChangedCallback));

        private static void BlurRadiusPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not BlurControl blurControl)
                return;

            if (e.NewValue is not double newValue)
                return;

            blurControl.BlurRadius = blurControl.BlurEffect.Radius = newValue;
        }

        public double BlurRadius
        {
            get => (double)GetValue(BlurRadiusProperty);
            set => SetValue(BlurRadiusProperty, value);
        }

        public void OnBlur(double? radius = 20)
        {
            double newRadius = radius ?? BlurRadius;

            ForceBlur = true;
            BlurRadius = newRadius;

            RectangleBlur.Fill = new VisualBrush(BackgroundObjectToBlur)
            {
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                ViewboxUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.None
            };
        }

        public void OffBlur()
        {
            ForceBlur = false;

            RectangleBlur.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        public void UpdateBlur()
        {
            if (!ForceBlur || BackgroundObjectToBlur is null)
            {
                return;
            }

            VisualBrush b = RectangleBlur.Fill as VisualBrush;

            try
            {
                _parentVisual ??= VisualTreeHelper.GetParent(BackgroundObjectToBlur) as Visual;

                if (b == null)
                {
                    OnBlur(null);

                    return;
                }

                Point offset = TransformToVisual(_parentVisual).Transform(new Point());

                b.Viewbox = new Rect(offset.X + GridBlur.Margin.Left + 2, offset.Y + GridBlur.Margin.Top + 2, 1, 1);
            }
            catch (InvalidOperationException) { }
        }

        private void BlurControl_OnLayoutUpdated(object sender, EventArgs e)
        {
            if (ForceBlur) 
                UpdateBlur();

            if (ForceBlur == _blurOn) 
                return;

            if (ForceBlur)
                OnBlur(null);
            else
                OffBlur();

            _blurOn = ForceBlur;
        }
    }
}
