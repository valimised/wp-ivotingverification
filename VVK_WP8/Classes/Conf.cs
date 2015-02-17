using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace VVK_WP8
{
    class Conf
    {
        private const string CONF_URL_LIVE = "https://www.valimised.ee/config.json";
        private const string CONF_URL = CONF_URL_LIVE;
        private const string CONF_CERT = "Juur-SK.pem.crt";

        private static Conf _instance = null;
        public static Conf Instance
        {
            private set { _instance = value; }
            get
            {
                if (_instance == null)
                    _instance = new Conf();

                return _instance;    
            }
        }

        private ConfModel _confModel;
        public ConfModel Model { get { return _confModel; } private set { _confModel = value; } }


        public delegate void CompletedEventHandler(bool success);

        private Conf()
        {
        }

        public void ResetData()
        {
            _confModel = null;
        }

        public bool ElectionsStarted()
        {
            if (_confModel == null)
                return false;

            if (_confModel.AppConfig.Params.AppUrl == null || _confModel.AppConfig.Params.AppUrl.Length < 1)
                return false;

            try
            {
                Uri uri = new Uri(_confModel.AppConfig.Params.AppUrl);
            } catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public void DownloadAsync (CompletedEventHandler completedEvent)
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                VVK_WPRC.CurlUtil cutil = new VVK_WPRC.CurlUtil();

                string output = cutil.DownloadWithCert(CONF_URL, false, "", CONF_CERT);
                Debug.WriteLine(output);
                if (output == null || output.Length < 1)
                {
                    completedEvent(false);
                    return;
                }

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ConfModel));
                using (MemoryStream memStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(output)))
                {
                    try
                    {
                        _confModel = serializer.ReadObject(memStream) as ConfModel;
                    }
                    catch (Exception e)
                    {
                        if(completedEvent != null)
                            completedEvent(false);
                        return;
                    }
                }

                if (completedEvent != null)
                    completedEvent(true);
            }));
            thread.Start();
        }
    }
}
