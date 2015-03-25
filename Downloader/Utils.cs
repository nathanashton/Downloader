using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Download
{
    public static class Utils
    {

        public static string BitsToString(int input)
        {
            const int byteConversion = 1024;
            var bytes = Convert.ToDouble(input);

            if (bytes >= Math.Pow(byteConversion, 3))
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 3), 2), " GB");
            }
            if (bytes >= Math.Pow(byteConversion, 2))
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 2), 2), " MB");
            }
            if (bytes >= byteConversion)
            {
                return string.Concat(Math.Round(bytes / byteConversion, 2), " KB");
            }
            return string.Concat(bytes, " Bytes");
        }
    }

}
