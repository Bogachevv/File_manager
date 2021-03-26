using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_manager
{
    class ConfigManager
    {
        public string Way { get; set; }
        public string localHomeDir { get; set; }
        public string serverHomeDir { get; set; }
        public string FTPIP { get; set; }
        public string FTPLogin { get; set; }
        public string FTPPassword { get; set; }
        
        
        private char startChar;
        private int charShift;

        public ConfigManager()
        {

        }

        public ConfigManager(string way)
        {
            Way = way;
        }

        public bool Open(char stChar, int shift)
        {
            startChar = stChar;
            charShift = shift;
            return true;
        }

        public bool LoadData()
        {
            try
            {
                if (!File.Exists(Way)) return false;
                StreamReader reader = new StreamReader(Way);
                string data = reader.ReadToEnd();
                data = DecodeData(ASCIIEncoding.ASCII.GetBytes(data),charShift,startChar);
                string[] dataArray = data.Split('|');
                FTPIP = dataArray[0];
                FTPLogin = dataArray[1];
                FTPPassword = dataArray[2];
                localHomeDir = dataArray[3];
                serverHomeDir = dataArray[4];
                reader.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UploadData()
        {
            try
            {
                StreamWriter writer = new StreamWriter(Way);
                string data = "";
                data += FTPIP +@"|";
                data += FTPLogin + @"|";
                data += FTPPassword + @"|";
                data += localHomeDir + @"|";
                data += serverHomeDir;
                writer.WriteLine(ASCIIEncoding.ASCII.GetString(EncodeData(data,charShift,startChar)));
                writer.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool LoadData(string way, int charShift, char startChar, out string[] args)
        {
            args = null;
            return true;
        }

        public static bool UploadData(string way, int charShift, char startChar, params string[] args)
        {
            try
            {
                StreamWriter writer = new StreamWriter(way);
                string data = "";
                foreach (string str in args) data += str + "|";
                data = data.Substring(0,data.Length-1);
                //data += FTPIP + @"|";
                //data += FTPLogin + @"|";
                //data += FTPPassword + @"|";
                //data += localHomeDir + @"|";
                //data += serverHomeDir;
                writer.WriteLine(ASCIIEncoding.ASCII.GetString(EncodeData(data,charShift,startChar)));
                writer.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] EncodeData(string data, int charShift, char startChar)
        {
            string str = "";
            byte[] res;
            char shiftedChar = startChar;
            foreach (char ch in data)
            {
                str += (char)(ch + charShift);
                str += shiftedChar++;
            }
            res = ASCIIEncoding.ASCII.GetBytes(str);
            return res;
        }

        private static string DecodeData(byte[] data, int charShift, char startChar)
        {
            string str = ASCIIEncoding.ASCII.GetString(data);
            string res = "";
            int len = str.Length;
            for (int i = 0; i < len-2; i+=2)
            {
                res += (char)(str[i]-charShift);
            }
            return res;
        }

    }
}
