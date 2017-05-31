using Microsoft.Win32;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloader
{
    public class DownloadFile
    {
        private static string[] returnedArray;
        private static int failCounter;

        //public static void Main()
        //{
        //    string[] urlArray = {
        //            "http://example.com"
        //    };
        //    string[] pathArray = {
        //            "C:\\Reports\\test"
        //    };
        //
        //    DownloadList(urlArray, pathArray, "user", "pass", true);
        //}

        public static string[] DownloadList(string[] urlArray, string[] toPathArray, string login = "", string pass = "", bool getExt = false)
        {
            //Console.WriteLine("DownloadList({0}, {1}, {2}, {3}, {4})", urlArray, toPathArray, login, pass, getExt);
            try
            {
                returnedArray = new string[urlArray.Length];
                Task[] taskArray = new Task[urlArray.Length];
                for (int x = 0; x < urlArray.Length; x++)
                {
                    int i = x;
                    //Thread.Sleep(600);
                    //Console.WriteLine("x = {0}", i);
                    Task task = new Task(() => { returnedArray[i] = Download(urlArray[i], toPathArray[i], login, pass, getExt, true); });
                    task.Start();
                    taskArray[i] = task;
                }
                Task.WaitAll(taskArray);
                Thread.Sleep(1000);
                //Console.WriteLine();
                //Console.WriteLine("Done! Press Enter to close.");
                //Console.ReadLine();
                return returnedArray;
            }
            catch (Exception)
            {
                //Console.WriteLine();
                //Console.WriteLine(e.Message);
                //Console.ReadLine();
                return null;
            }
        }

        public static string Download(string url, string toPath, string login = "", string pass = "", bool getExt = false, bool counterReset = false)
        {

            //Console.WriteLine("Download({0}, {1}, {2}, {3}, {4}, {5})", url, toPath, login, pass, getExt, counterReset);
            if (counterReset) failCounter = 0;
            WebClient webClient = new WebClient();
            if (login != "" && pass != "") webClient.Credentials = new NetworkCredential(login, pass);

            WebProxy webProxy = new WebProxy("gscproxy.barratt.internal:8080", true);
            webProxy.UseDefaultCredentials = true;
            webClient.Proxy = webProxy;
            try
            {
                webClient.DownloadData(url);
                string contentType = webClient.ResponseHeaders["Content-Type"];
                contentType = GetDefaultExtension(contentType);

                string newToPath = getExt ? toPath + contentType : toPath;

                webClient.DownloadFile(url, newToPath);

                //Console.WriteLine("Downloaded: {0}", newToPath);
                return newToPath;
            }
            catch (Exception)
            {
                //Console.WriteLine(e.Message);
                failCounter++;
                if (failCounter >= 5) Thread.Sleep(1000); Download(url, toPath, login, pass, getExt, false);
                return "";
            }
        }

        private static string GetDefaultExtension(string mimeType)
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
                object value = key != null ? key.GetValue("Extension", null) : null;
                if (value == null & mimeType == "text/comma-separated-values") value = ".csv";
                return value != null ? value.ToString() : mimeType;
            }
            catch (Exception)
            {
                //Console.WriteLine(e.Message);
                return mimeType == "comma-separated-values" ? ".csv" : mimeType;
            }
        }

    }
}
