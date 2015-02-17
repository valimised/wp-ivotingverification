using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVK_WP8
{
    class QRResult
    {
        public string VoteContainerId   { get; private set; }
        public Dictionary<string, string> ElectionHexSeeds { get; private set; }

        private static QRResult _instance = null;
        public static QRResult Instance
        {
            private set { _instance = value; }
            get
            {
                if (_instance == null)
                    _instance = new QRResult();

                return _instance;
            }
        }

        private QRResult()
        {

        }

        public void ResetData()
        {
            ElectionHexSeeds = null;
            VoteContainerId = null;
        }

        public bool Parse(string input)
        {
            ElectionHexSeeds = new Dictionary<string, string>();


            if (Util.isCorrectQR(input))
            {
                string[] components = input.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                if (components.Length > 0)
                {
                    VoteContainerId = components[0];
                    if (components.Length > 1)
                    {
                        for (int i = 1; i < components.Length; i++)
                        {
                            string[] rowComponents = components[i].Split('\t');
                            if (rowComponents.Length > 1)
                            {
                                ElectionHexSeeds.Add(rowComponents[0], rowComponents[1]);
                            }
                        }

                        if (ElectionHexSeeds.Count > 0)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
