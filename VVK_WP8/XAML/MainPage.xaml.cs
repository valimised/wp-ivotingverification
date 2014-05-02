using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using VVK_WP8.Resources;
using Microsoft.Devices;
using System.Windows.Media.Imaging;
using ZXing;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Net.NetworkInformation;
using System.Diagnostics;
using System.Windows.Media;

namespace VVK_WP8
{
    public partial class MainPage : PhoneApplicationPage, IDisposable
    {
        private PhotoCamera _phoneCamera;
        private IBarcodeReader _barcodeReader;
        private DispatcherTimer _scanTimer;
        private WriteableBitmap _previewBuffer;
        private DispatcherTimer _autofocusTimer;

        public bool BlockBackKey { get; set; }

        private enum MessageType
        {
            Regular,
            Error,
            Verify
        };

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        public virtual void Dispose()
        {
            if(_phoneCamera != null) _phoneCamera.Dispose();
        }

        private void CheckNetworkAvailability()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                ShowMessage("Veenduge, et nutiseadme andmeside on võimaldatud",
                    "Proovi uuesti", () =>
                    {
                        
                        CheckNetworkAvailability();
                    }, null, null, MessageType.Error);

            }
            else
            {
                DownloadConfiguration();
            }
        }

        void DisplayWelcomeMessage(bool displayOnlyInfoButton = false)
        {
            TextsModel texts;
            if (Conf.Instance.Model != null)
            {
                texts = Conf.Instance.Model.AppConfig.Texts;
            }
            else
            {
                texts = new TextsModel();
                texts.WelcomeMessage = "welcome";
                texts.BtnMore = "more";
                texts.BtnNext = "next";

            }

            MessageButtonClicked button2Click = null;
            string button2Text = null;

            if (Conf.Instance.ElectionsStarted())
            {
                button2Text = texts.BtnNext;
                button2Click = () =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            HideMessage();
                            InitializeBarcodeReader();
                            StartScanning();
                        });
                    };
            }

            ShowMessage(texts.WelcomeMessage, texts.BtnMore,
            () =>
            {
                var timer = new System.Threading.Timer(obj =>
                {
                    Dispatcher.BeginInvoke(
                        () =>
                        {
                            NavigationService.Navigate(new Uri(App.XamlFolder + "InfoPage.xaml", UriKind.Relative));
                        });
                }, null, 200, System.Threading.Timeout.Infinite);

            },
            button2Text,
            button2Click,
            MessageType.Regular);

        }

        private void DownloadConfiguration()
        {
            if (Conf.Instance.Model == null)
            {
                Spinner.Show();
                Conf.Instance.DownloadAsync((bool success) =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        Spinner.Hide();
                        if (success)
                        {
                            if (Conf.Instance.ElectionsStarted() == false)
                            {
                                DisplayWelcomeMessage();
                            }
                            else
                            {
                                InitializePhoneCamera();
                            }
                        }
                        else
                        {
                            ShowMessage("Konfiguratsiooni laadimine ebaõnnestus", "Proovi uuesti", () =>
                            {
                                DownloadConfiguration();
                            },
                            null, null, MessageType.Error);
                        }
                    });
                });

                
            }
            else
            {
                InitializePhoneCamera();
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            StopScanning();                        
            HideMessage();
            CheckNetworkAvailability();

            base.OnNavigatedTo(e);

        }

        private void UninitializeCamera()
        {
            StopAutofocusTimer();
            StopScanning();

            if (_phoneCamera != null)
            {
                // Cleanup
                _phoneCamera.Initialized -= CameraInitialized;
                CameraButtons.ShutterKeyHalfPressed -= CameraButtons_ShutterKeyHalfPressed;
                _phoneCamera.CancelFocus();
                _phoneCamera.Dispose();
                _phoneCamera = null;
            }
            
            if (_autofocusTimer != null)
            {
                _autofocusTimer = null;
            }

            if (_scanTimer != null)
            {
                _scanTimer = null;
            }

            if (_previewBuffer != null)
            {
                _previewBuffer = null;
            }

            if (_barcodeReader != null)
            {
                _barcodeReader = null;
            }
        }


        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            UninitializeCamera();
        }

        void BarcodeReader_ResultFound(Result obj)
        {
            StopScanning();

            if (QRResult.Instance.Parse(obj.Text) == false)
            {
                ShowMessage(Conf.Instance.Model.AppConfig.Errors.ProblemQrcodeMessage, Conf.Instance.Model.AppConfig.Texts.BtnNext, () =>
                {
                    StartScanning();
                },
                null,
                null, MessageType.Error);

                return;
            }

            Spinner.Show();
            Vote.Instance.VerifyAsync(Conf.Instance.Model.AppConfig.Params.AppUrl, QRResult.Instance.VoteContainerId, (Vote.VerificationStatus verificationStatus) =>
            {
                if (verificationStatus == Vote.VerificationStatus.Failure)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Spinner.Hide();
                        string errorMessage = (Vote.Instance.ErrorMessage ?? Conf.Instance.Model.AppConfig.Errors.BadServerResponseMessage);
                        
                        ShowMessage(errorMessage, Conf.Instance.Model.AppConfig.Texts.BtnNext, () =>
                        {
                            StartScanning();
                        },
                        null, null, MessageType.Error);

                        
                    });
                    return;
                }

                Dispatcher.BeginInvoke(() =>
                {
                    Spinner.Hide();
                    ShowMessage(Conf.Instance.Model.AppConfig.Texts.VerifyMessage, Conf.Instance.Model.AppConfig.Texts.BtnVerify, () =>
                    {
                        Thread thread = new Thread(new ThreadStart(() =>
                        {
                            Dispatcher.BeginInvoke(() =>
                            {
                                Spinner.Show();
                            });

                            Vote.VoteBruteForceStatus status = Vote.Instance.BruteForceVerification();
                            
                            

                            Dispatcher.BeginInvoke(() =>
                            {
                                Spinner.Hide();
                                if (status == Vote.VoteBruteForceStatus.Success)
                                {
                                    NavigationService.Navigate(new Uri(App.XamlFolder + "VerificationResultsPage.xaml", UriKind.Relative));
                                }
                                else if(status == Vote.VoteBruteForceStatus.NoVerifiedCandidates)
                                {
                                    ShowMessage(Conf.Instance.Model.AppConfig.Errors.BadVerificationMessage, Conf.Instance.Model.AppConfig.Texts.CloseButton, () =>
                                    {
                                        Application.Current.Terminate();
                                    }, null, null, MessageType.Error);
                                }
                                else if (status == Vote.VoteBruteForceStatus.KeyInitFailed)
                                {
                                    ShowMessage(Conf.Instance.Model.AppConfig.Errors.BadServerResponseMessage, Conf.Instance.Model.AppConfig.Texts.CloseButton, () =>
                                    {
                                        Application.Current.Terminate();
                                    }, null, null, MessageType.Error);
                                }
                            });
                        }));
                        thread.Start();
                    },
                    null, null, MessageType.Verify);
                });

            });
        }

        void StartScanning()
        {
            if (_scanTimer != null && _autofocusTimer != null)
            {
                _scanTimer.Start();
                CameraCrosshair.Show();
                StartAutofocusTimer();

            }
            else throw new Exception("Can't start scanning: scan timer or autofocus timer is not initialized");
        }

        void StopScanning()
        {
            CameraCrosshair.Hide();
            StopAutofocusTimer();

            if (_scanTimer != null)
                _scanTimer.Stop();

        }

        void StartAutofocusTimer()
        {
            if (_autofocusTimer != null)
            {
                _autofocusTimer.Start();
            }
        }

        void StopAutofocusTimer()
        {
            if (_autofocusTimer != null)
                _autofocusTimer.Stop();
        }

        void CameraFocusTapped(object sender, GestureEventArgs e)
        {
            if (_phoneCamera != null)
            {
                if (_phoneCamera.IsFocusAtPointSupported == true)
                {
                    if (viewfinderCanvas == null)
                    {
                        return;
                    }
                }
            }
        }

        void CameraButtons_ShutterKeyHalfPressed(object sender, EventArgs e)
        {
            if (_phoneCamera != null)
                _phoneCamera.Focus();
        }

        private void InitializeBarcodeReader()
        {
            _phoneCamera.FlashMode = FlashMode.Off;
            _previewBuffer = new WriteableBitmap((int)_phoneCamera.PreviewResolution.Width, (int)_phoneCamera.PreviewResolution.Height);

            _barcodeReader = new BarcodeReader();

            var supportedBarcodeFormats = new List<BarcodeFormat>();
            supportedBarcodeFormats.Add(BarcodeFormat.QR_CODE);
            _barcodeReader.Options.PossibleFormats = supportedBarcodeFormats;
            _barcodeReader.Options.TryHarder = true;

            _barcodeReader.ResultFound += BarcodeReader_ResultFound;
        }

        private void UninitializeBarcodeReader()
        {
            if (_barcodeReader != null)
            {
                _barcodeReader.ResultFound -= BarcodeReader_ResultFound;
                _barcodeReader = null;
            }

            if (_previewBuffer != null)
            {
                _previewBuffer = null;
            }
        }

        void CameraInitialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    DisplayWelcomeMessage();
     
                });
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Unable to initialize the camera");
                });
            }
        }

        private void ScanForBarcode()
        {
            //grab a camera snapshot
            _phoneCamera.GetPreviewBufferArgb32(_previewBuffer.Pixels);
            _previewBuffer.Invalidate();

            //scan the captured snapshot for barcodes
            //if a barcode is found, the ResultFound event will fire
            _barcodeReader.Decode(_previewBuffer);

        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
        }

        public static bool _cameraInitialized = false;

        private delegate void MessageButtonClicked();

        private void HideMessage()
        {
            PopupMessage_Grid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ShowMessage(string title, string button1Text, MessageButtonClicked button1Clicked,
            string button2Text, MessageButtonClicked button2Clicked, MessageType messageType)
        {
            PopupMessage_Title.Text = title;

            ButtonsStackPanel.Children.Clear();

            // add buttons
            bool twoButtons = (button2Text != null && button2Clicked != null);

            Button button1 = new Button();
            Button button2 = null;

            button1.Width = 170;
            button1.Height = 130;
            button1.Content = button1Text;
            button1.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            button1.BorderThickness = new Thickness(0);
            button1.Click += (sender, e) => { button1Clicked(); HideMessage();  };
            
            if (twoButtons)
            {
                button1.Margin = new Thickness(0, 0, -8, 0); 

                button2 = new Button();
                button2.Margin = new Thickness(-8, 0, 0, 0);
                button2.Width = 170;
                button2.Height = 130;
                button2.Content = button2Text;
                button2.BorderThickness = new Thickness(0);
                button2.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                button2.Click += (sender, e) => { button2Clicked(); HideMessage(); };
            }
            else
            {
                button1.Width = 320;
                button1.Margin = new Thickness(0, 0, 0, 0);
            }

            ButtonsStackPanel.Children.Add(button1);
            if(twoButtons)
                ButtonsStackPanel.Children.Add(button2);

            if (Conf.Instance.Model != null)
            {
                button1.Background = new SolidColorBrush(Util.HexColorToWindowsMediaColor(Conf.Instance.Model.AppConfig.Colors.BtnBackground));
                button1.Foreground = new SolidColorBrush(Util.HexColorToWindowsMediaColor(Conf.Instance.Model.AppConfig.Colors.BtnForeground));
                if (button2Clicked != null)
                {
                    button2.Background = new SolidColorBrush(Util.HexColorToWindowsMediaColor(Conf.Instance.Model.AppConfig.Colors.BtnBackground));
                    button2.Foreground = new SolidColorBrush(Util.HexColorToWindowsMediaColor(Conf.Instance.Model.AppConfig.Colors.BtnForeground));
                }
                PopupMessage_Title.Foreground = new SolidColorBrush(Util.HexColorToWindowsMediaColor(Conf.Instance.Model.AppConfig.Colors.LblForeground));

                if (messageType == MessageType.Error)
                {
                    PopupMessage_Grid.Background = new SolidColorBrush(Util.HexColorToWindowsMediaColor(Conf.Instance.Model.AppConfig.Colors.ErrorWindow));
                }
                else if (messageType == MessageType.Regular || messageType == MessageType.Verify)
                {
                    PopupMessage_Grid.Background = new SolidColorBrush(Util.HexColorToWindowsMediaColor(Conf.Instance.Model.AppConfig.Colors.MainWindow));
                }
            }
            else
            {

                button1.Background = new SolidColorBrush(Util.HexColorToWindowsMediaColor("#F0F0F0"));
                button1.Foreground = new SolidColorBrush(Util.HexColorToWindowsMediaColor("#727272"));
                if (button2Clicked != null)
                {
                    button2.Background = new SolidColorBrush(Util.HexColorToWindowsMediaColor("#F0F0F0"));
                    button2.Foreground = new SolidColorBrush(Util.HexColorToWindowsMediaColor("#727272"));
                }
                PopupMessage_Title.Foreground = new SolidColorBrush(Util.HexColorToWindowsMediaColor("#FFFFFF"));

                if (messageType == MessageType.Error)
                {
                    PopupMessage_Grid.Background = new SolidColorBrush(Util.HexColorToWindowsMediaColor("#FF0000"));
                }
                else if (messageType == MessageType.Regular || messageType == MessageType.Verify)
                {
                    PopupMessage_Grid.Background = new SolidColorBrush(Util.HexColorToWindowsMediaColor("#33B5E5"));
                }
            }
            PopupMessage_Grid.Margin = new Thickness(0, 80, 0, 0);
            PopupMessage_Grid.Visibility = System.Windows.Visibility.Visible;
        }

        private Thread _cameraThread;
        private void InitializePhoneCamera()
        {
            _phoneCamera = new PhotoCamera(CameraType.Primary);
            _phoneCamera.Initialized += CameraInitialized;

            viewfinderBrush.SetSource(_phoneCamera);


            double cameraAspectRatioRotated = _phoneCamera.Resolution.Height / _phoneCamera.Resolution.Width;
            double deviceAspectRatio = Application.Current.Host.Content.ActualHeight / Application.Current.Host.Content.ActualWidth;
            double scaleY = cameraAspectRatioRotated * deviceAspectRatio;


            viewfinderTransform.ScaleY = scaleY;

            CameraButtons.ShutterKeyHalfPressed += CameraButtons_ShutterKeyHalfPressed;

            _phoneCamera.AutoFocusCompleted += _phoneCamera_AutoFocusCompleted;

            //Display the camera feed in the UI
            
            _autofocusTimer = new DispatcherTimer();
            _autofocusTimer.Interval = TimeSpan.FromMilliseconds(2000);
            _autofocusTimer.Tick += (o, arg) =>
            {
                if (_phoneCamera == null)
                {
                    return;
                }
                _phoneCamera.Focus();
            };

            // This timer will be used to scan the camera buffer every 250ms and scan for any barcodes
            _scanTimer = new DispatcherTimer();
            _scanTimer.Interval = TimeSpan.FromMilliseconds(250);
            _scanTimer.Tick += (o, arg) => ScanForBarcode();

        }

        void _phoneCamera_AutoFocusCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            if (e.Exception != null)
            {
                Debug.WriteLine("{0}", e.Exception.Message);
            }

        }

    }
}