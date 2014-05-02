using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using System.Threading;

namespace VVK_WP8
{
    public partial class InfoPage : PhoneApplicationPage
    {
        WebBrowser browser;
        
        public InfoPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            browser = new WebBrowser();
            browser.Width = LayoutRoot.Width;
            browser.Height = LayoutRoot.Height;
            browser.IsScriptEnabled = true;
            browser.Source = new Uri(Conf.Instance.Model.AppConfig.Params.HelpUrl, UriKind.Absolute);
            LayoutRoot.Children.Add(browser);

            Timer timer = new Timer((object state) => { Dispatcher.BeginInvoke(() => {  browser.IsScriptEnabled = true; }); }, null, 5000, Timeout.Infinite);
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e); 
        }
    }
}