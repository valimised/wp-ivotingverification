using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace VVK_WP8
{
    class Util
    {
        public static System.Windows.Media.Color HexColorToWindowsMediaColor(string hexColor)
        {
            if (hexColor[0] == '#')
                hexColor = hexColor.Substring(1);

            if (hexColor.Length < 6)
                return System.Windows.Media.Color.FromArgb(255, 0, 0, 0);

            byte A, R, G, B;

            if (hexColor.Length == 6)
            {
                A = 255;
                R = Convert.ToByte(hexColor.Substring(0, 2), 16);
                G = Convert.ToByte(hexColor.Substring(2, 2), 16);
                B = Convert.ToByte(hexColor.Substring(4, 2), 16);
            }
            else if (hexColor.Length == 8)
            {
                A = Convert.ToByte(hexColor.Substring(0, 2), 16);
                R = Convert.ToByte(hexColor.Substring(2, 2), 16);
                G = Convert.ToByte(hexColor.Substring(4, 2), 16);
                B = Convert.ToByte(hexColor.Substring(6, 2), 16);
            }
            else
            {
                throw new ArgumentException("Invalid web color");
            }

            return System.Windows.Media.Color.FromArgb(255, R, G, B);
        }

        public static Boolean isCorrectQR(string input)
        {
            Match m = Regex.Match(input, @"^\w{40}\r\n(\w{1,28}\t([A-Fa-f0-9]){40}\r\n){1,5}");

            return m.Success;
        }
    }
}
