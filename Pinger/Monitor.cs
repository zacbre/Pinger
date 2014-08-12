using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Pinger
{
    class Monitor
    {
        public Thread th
        {
            get { return thred; }
        }
        private Thread thred;
        public int Status
        {
            get { return status; }
        }
        public int OldStatus
        {
            get { return oldstatus; }
            set { oldstatus = value; }
        }
        private int status = 2;
        private int oldstatus = 2;
        private bool isurl;
        private string url;
        public string err;
        public string URI
        {
            get { return url; }
        }
        public Monitor(string URL)
        {
            url = URL;
            if (URL.Contains("http://") || URL.Contains("https://"))
            {
                //Automatically a URL.
                isurl = true;
            }
            else
            {
                Regex Pattern = new Regex(@"((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)");
                Match m = Pattern.Match(URL);
                if (m.Groups.Count > 0)
                {
                    isurl = false;
                }
            }
            thred = new Thread(new ParameterizedThreadStart(MonitorURL)) { IsBackground = true };
            thred.Start(URL);
        }
        //Thread.
        private void MonitorURL(object URL)
        {
            while (true)
            {
                if (isurl)
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(URL.ToString()));
                    //request.Method = "HEAD";
                    try
                    {
                        var response = request.GetResponse();
                        response.Close();
                        //Site is online.
                        status = 1;
                    }
                    catch (WebException wex)
                    {
                        err = wex.Message;
                        status = 0;
                    }
                }
                else
                {
                    //IP, try pinging.
                    var ping = new System.Net.NetworkInformation.Ping();
                    var result = ping.Send(URL.ToString());
                    if (result.Status != System.Net.NetworkInformation.IPStatus.Success)
                        status = 0;
                    else
                        status = 1;
                }
                Thread.Sleep(5000);
            }
        }
    }
}
