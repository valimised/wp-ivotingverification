using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows.Media;

namespace VVK_WP8
{
    public partial class Spinner : UserControl
    {
        public Spinner()
        {
            InitializeComponent();
        
            
        }

        private static Spinner _spinner;
        private static Popup _popup;

        public static void Show()
        {
            _spinner = new Spinner();
            _spinner.InitializeComponent();

            if(Conf.Instance.Model == null) {
                _spinner.SpinnerBackground.Fill = new SolidColorBrush(Util.HexColorToWindowsMediaColor("#33B5E5"));
            } else {
                _spinner.SpinnerBackground.Fill = new SolidColorBrush(Util.HexColorToWindowsMediaColor(Conf.Instance.Model.AppConfig.Colors.MainWindow));
            }


            double screenWidth = Application.Current.Host.Content.ActualWidth;
            double screenHeight = Application.Current.Host.Content.ActualHeight;
            _spinner.Margin = new Thickness((screenWidth / 2) - 160, (screenHeight / 2) - 160, 0, 0);

            _popup = new Popup();
            _popup.Child = _spinner;
            _popup.IsOpen = true;

            _spinner.StartAnimation();
        }

        public static void Hide()
        {
            if (_popup != null)
            {
                _popup.IsOpen = false;
            }
        }

        private void StartAnimation()
        {
            Storyboard board = new Storyboard();
            var timeline = new DoubleAnimationUsingKeyFrames();
            timeline.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTarget(timeline, rotateTransform);
            Storyboard.SetTargetProperty(timeline, new PropertyPath("Angle"));
            var frame = new LinearDoubleKeyFrame() { KeyTime = TimeSpan.FromSeconds(1), Value = 360 };
            timeline.KeyFrames.Add(frame);
            board.Children.Add(timeline);

            board.Begin();
        }
    }
}
