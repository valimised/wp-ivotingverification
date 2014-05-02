using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Controls.Primitives;
using Windows.UI;
using System.Windows.Media;

namespace VVK_WP8
{
    public partial class CameraCrosshair : UserControl
    {
        public enum Type
        {
            Normal,
            Error
        };

        public CameraCrosshair()
        {
            InitializeComponent();
            SystemTray.IsVisible = false;
        }

        private static Popup _popup = null;
        private static CameraCrosshair _camCrosshair = null;

        public static void Hide()
        {
            if(_popup != null)
                _popup.IsOpen = false;
        }

        public static MessageBoxResult Show()
        {
            _popup = new Popup();
            _camCrosshair = new CameraCrosshair();

            double screenWidth = Application.Current.Host.Content.ActualWidth;
            double screenHeight = Application.Current.Host.Content.ActualHeight;
            _camCrosshair.Margin = new Thickness((screenWidth / 2) - 160, (screenHeight / 2) - 160, 0, 0);

            _popup.Child = _camCrosshair;
            _popup.IsOpen = true;

            return MessageBoxResult.Cancel;
        }

    }
}