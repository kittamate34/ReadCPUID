using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadCPUID
{
    public class Util
    {
        public bool ErrorLogs(string pMessage)
        {
            bool lResult = false;

            try
            {
                string lPath = Application.StartupPath + @"\Logs";

                if (!string.IsNullOrEmpty(lPath))
                {
                    if (Directory.Exists(lPath) == false) { Directory.CreateDirectory(lPath); }

                    File.AppendAllText(lPath + @"\Logs_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " : " + pMessage + Environment.NewLine, UTF8Encoding.Default);

                    lResult = true;
                }
                else
                {
                    lResult = false;
                }
            }
            catch
            {
                lResult = false;
            }

            return lResult;
        }

        public Boolean Str2Bool(string pValue)
        {
            try
            {
                Boolean lReBool = false;
                Int32 lnumber = 0;

                if (Int32.TryParse(pValue, out lnumber) == true)
                {
                    lReBool = Convert.ToBoolean(lnumber);
                }
                else
                {
                    lReBool = Convert.ToBoolean(pValue.Trim());
                }

                return lReBool;
            }
            catch
            {
                return false;
            }
        }

        public int Str2Int(string pValue, int pDefault = 0)
        {
            //If Convert.ToInt(pValue) = Error for Return Parameter = pDefault
            try
            {
                return Convert.ToInt32(pValue);
            }
            catch
            {
                return pDefault;
            }
        }

        public Double Str2Double(string pValue, Double pDefault = 0.00)
        {
            //If Convert.ToInt(pValue) = Error for Return Parameter = pDefault
            try
            {
                return Convert.ToDouble(pValue);
            }
            catch
            {
                return pDefault;
            }
        }
    }
}
