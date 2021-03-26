using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace File_manager
{
    public class FTPClient
    {
        private string url, login, password;
        public delegate void UploadEnded(object sender, string e);
        public delegate void Error(object sender, Exception e);
        public event UploadEnded onUploadEnded;
        public event Error onError;

        public string Url { get { return url; } set { if ((value != null) && (value != "")) url = value; } }
        public string Login { get { return login; } set { if ((value != null) && (value != "")) login = value; } }
        public string Password { get { return password; } set { if ((value != null) && (value != "")) password = value; } }

        public FTPClient()
        {

        }

        public void UploadFile(string webName, string loaclName)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + webName);
            request.Credentials = new NetworkCredential(login, password);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            FileStream readStream = new FileStream(loaclName, FileMode.Open);
            Stream sendStream = request.GetRequestStream();
            byte[] bytes = new byte[readStream.Length];
            readStream.Read(bytes, 0, bytes.Length);
            sendStream.Write(bytes, 0, bytes.Length);
            readStream.Close();
            sendStream.Close();
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            onUploadEnded?.Invoke(this, response.StatusDescription);
            request.Abort();
        }

        public void DeleteDirectory(string fname)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + fname);
            request.Credentials = new NetworkCredential(login, password);
            request.Method = WebRequestMethods.Ftp.RemoveDirectory;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            onUploadEnded?.Invoke(this, response.StatusDescription);
            request.Abort();
        }

        public async void DeleteDirectoryAsync(string fname)
        {
            await Task.Run(() =>
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + fname);
                request.Credentials = new NetworkCredential(login, password);
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                onUploadEnded?.Invoke(this, response.StatusDescription);
                request.Abort();
            });
        }

        public void DeleteFile(string fname)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + fname);
            request.Credentials = new NetworkCredential(login, password);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            onUploadEnded?.Invoke(this, response.StatusDescription);
            request.Abort();
        }

        public async void DeleteFileAsync(string fname)
        {
            await Task.Run(() => DeleteFile(fname));
        }

        public async void DownloadFileAsync(string fname, string dir) //Corrected
        {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + fname);
                request.Credentials = new NetworkCredential(login, password);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                FtpWebResponse response;
                FileStream writeStream;
                await Task.Run(
                    () =>
                    {
                        try
                        {
                            response = (FtpWebResponse)request.GetResponse();
                            writeStream = new FileStream(dir + fname.Substring(fname.LastIndexOf(@"\")), FileMode.Create);
                            Stream responseStream = request.GetResponse().GetResponseStream();
                            //int len = GetStreamLength(responseStream);
                            //byte[] bytes = new byte[len];
                            //responseStream.Read(bytes, 0, bytes.Length);
                            byte[] bytes = GetBytesFromStream(responseStream);
                            writeStream.Write(bytes, 0, bytes.Length);
                            writeStream.Close();
                            responseStream.Close();
                            onUploadEnded?.Invoke(this, response.StatusDescription);
                            request.Abort();
                        }
                        catch(Exception e)
                        {
                            onError?.Invoke(this, e);
                        }
                    });

        }

        public void DownloadFile(string fname, string dir) //Corrected
        {   //string url1 = url + @"/" + fname;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + fname);
            request.Credentials = new NetworkCredential(login, password);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            FileStream writeStream = new FileStream(dir + fname.Substring(fname.LastIndexOf(@"\")), FileMode.Create);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            //byte[] bytes = new byte[responseStream.Length];
            //responseStream.Read(bytes, 0, bytes.Length);
            byte[] bytes = GetBytesFromStream(responseStream);
            writeStream.Write(bytes, 0, bytes.Length);
            responseStream.Close();
            writeStream.Close();
            onUploadEnded?.Invoke(this, response.StatusDescription);
            responseStream.Close();
            request.Abort();
        }

        public void CreateDirectory(string path)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + path);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(login, password);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            request.Abort();
            //response = response;
        }

        public void CreateFile(string path)
        {
            File.Open(path.Substring(path.LastIndexOf(@"\") + 1),FileMode.Create).Close();
            UploadFile(path, path.Substring(path.LastIndexOf(@"\") + 1));
            File.Delete(path.Substring(path.LastIndexOf(@"\") + 1));
        }

        public string[] GetListDirectory(string path)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + path + @"/");
            request.Credentials = new NetworkCredential(login, password);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            List<string> list = new List<string>();
            List<string> dir = new List<string>();
            List<string> file = new List<string>();
            while (!reader.EndOfStream)
            {
                string st = reader.ReadLine();
                if (st == "..") continue;
                if (st.Contains(".")) file.Add(st);
                else dir.Add(st);
            }
            list.Add("..");
            foreach (string st in dir) list.Add(st);
            foreach (string st in file) list.Add(st);
            return list.ToArray();
        }

        public string[] GetFiles(string path)
        {
            List<string> output = new List<string>();
            foreach (string str in GetListDirectory(path))
            {
                if (str.Contains(".")) output.Add(str);
            }
            return output.ToArray();
        }

        public string[] GetDirectories(string path)
        {
            ///Output list of directories in inputed directory
            List<string> output = new List<string>();
            foreach (string str in GetListDirectory(path))
            {
                if (!str.Contains(".")) output.Add(str);
            }
            return output.ToArray();
        }

        public Stream GetFileStream(string fname)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + fname);
            request.Credentials = new NetworkCredential(login, password);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            request.Abort();
            return response.GetResponseStream();
        }

        public byte[] GetFileBytes(string fname)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + fname);
            request.Credentials = new NetworkCredential(login, password);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            int d = 0;
            List<byte> bytes = new List<byte>();
            while ((d = stream.ReadByte()) != -1) bytes.Add((byte)d);
            request.Abort();
            return bytes.ToArray();
        }

        public void WriteFile(byte[] bytes, string way)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/" + way);
            request.Credentials = new NetworkCredential(login, password);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            Stream sendStream = request.GetRequestStream();
            sendStream.Write(bytes, 0, bytes.Length);
            sendStream.Close();
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            request.Abort();
        }

        public string GetCurrent()
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + @"/");
            request.Credentials = new NetworkCredential(login, password);
            request.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            return reader.ReadLine();
        }

        //Внутренние методы

        private byte[] GetBytesFromStream(Stream stream)
        {
            List<byte> bytes = new List<byte>();
            int d;
            while ((d = stream.ReadByte()) != -1) bytes.Add((byte)d);
            return bytes.ToArray();
        }


    }
}
