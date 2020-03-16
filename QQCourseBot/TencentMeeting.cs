using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;

namespace QQCourseBot
{
    public class TencentMeeting
    {
        private static string GetWemeet(string Url)
        {
            var client = new HttpClient();
            var result = client.GetAsync(Url);
            result.Wait();
            return "wemeet://page/inmeeting" + result.Result.RequestMessage.RequestUri.Query.Replace("meetingcode", "meeting_code");
        }

        public static void InvokeWemeet(string Url)
        {
            Console.WriteLine("[WEMEET] Meeting URL: " + GetWemeet(Url));
            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process.StartInfo.FileName = GetWemeet(Url);
                process.Start();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                process.StartInfo.FileName = "xdg-open";
                process.StartInfo.Arguments = GetWemeet(Url);
                process.Start();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                process.StartInfo.FileName = "open";
                process.StartInfo.Arguments = GetWemeet(Url);
                process.Start();
            }
            else
            {
                Console.WriteLine("[WEMEET] Your OS does not support Tencent Meeting.");
            }
        }
    }
}
