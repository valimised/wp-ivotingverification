using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;

namespace VVK_WP8
{
    public partial class VerificationResultsPage : PhoneApplicationPage, IDisposable
    {
        class CandidateItem
        {
            public string Name { get; set; }
            public string Party { get; set; }
            public string Code { get; set; }
            public string ElectionName { get; set; }
            public string LblOuterContainerBackground { get; set; }
            public string LblInnerContainerBackground { get; set; }
            public string LblOuterContainerForeground { get; set; }
            public string LblInnerContainerForeground { get; set; }
        }

        private Timer _appResetTimer;
        private int _closeTimeLeft;
        private int _closeInterval;
        private int _closeTimeout;
        private ObservableCollection<CandidateItem> _verifiedCandidates;
        private bool _navigatedFromBackground = false;

        public VerificationResultsPage()
        {
            InitializeComponent();

            _verifiedCandidates = new ObservableCollection<CandidateItem>();
            foreach (Candidate candidate in Vote.Instance.VerifiedCandidates) 
            {
                CandidateItem item = new CandidateItem();

                string[] components = candidate.Code.Split('.');
                if (components.Length > 0)
                {
                    item.Code = string.Format("#{0}", components[1]);
                }
                else
                {
                    item.Code = string.Format("#{0}", candidate.Code);
                }

                item.ElectionName = candidate.ElectionName;
                item.Name = candidate.Name;
                item.Party = candidate.Party;
                item.LblOuterContainerBackground = Conf.Instance.Model.AppConfig.Colors.LblOuterContainerBackground;
                item.LblInnerContainerBackground = Conf.Instance.Model.AppConfig.Colors.LblInnerContainerBackground;
                item.LblOuterContainerForeground = Conf.Instance.Model.AppConfig.Colors.LblOuterContainerForeground;
                item.LblInnerContainerForeground = Conf.Instance.Model.AppConfig.Colors.LblInnerContainerForeground;

                _verifiedCandidates.Add(item);
            }
            ResultsList.ItemsSource = _verifiedCandidates;
            Vote.Instance.VerifiedCandidates.Clear();
        }

        public virtual void Dispose()
        {
            if (_appResetTimer != null) _appResetTimer.Dispose();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnBackKeyPress(e);
        }

        // because we have blocked the Back Key, this event should
        // be fired only before going to background
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigatedFromBackground = true;

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_navigatedFromBackground)
            {
                // do nothing if we are returning from background
                base.OnNavigatedTo(e);
                return;
            }

            _closeInterval = Convert.ToInt32(Conf.Instance.Model.AppConfig.Params.CloseInterval);
            _closeTimeout = Convert.ToInt32(Conf.Instance.Model.AppConfig.Params.CloseTimeout);
            _closeTimeLeft = _closeTimeout;

            SetResetWarningText(_closeTimeLeft);

            InitResultTitle();

            _appResetTimer = new Timer(new TimerCallback((o) =>
            {
                Conf conf = Conf.Instance;

                Dispatcher.BeginInvoke(() =>
                {
                    SetResetWarningText(_closeTimeLeft);
                });

                _closeTimeLeft -= _closeInterval;

                if (_closeTimeLeft <= 0)
                {
                    _appResetTimer.Dispose();
                    Dispatcher.BeginInvoke(() =>
                    {
                        Application.Current.Terminate();
                    });
                }
                else
                {
                    _appResetTimer.Change(_closeInterval, Timeout.Infinite);
                }
            }), null, _closeInterval, Timeout.Infinite);

            base.OnNavigatedTo(e);
        }

        private void SetResetWarningText(int milliseconds)
        {
            string closeText = Conf.Instance.Model.AppConfig.Texts.LblCloseTimeout;
            ResetWarning.Text = closeText.Replace("XX", (milliseconds / 1000).ToString());
        }

        private void InitResultTitle()
        {
            ResultTitle.Text = Conf.Instance.Model.AppConfig.Texts.LblChoice;
        }
    }
}