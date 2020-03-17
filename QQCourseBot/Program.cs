using System;
using cqhttp.Cyan.Clients;
using cqhttp.Cyan.Events.CQEvents;
using cqhttp.Cyan.Events.CQResponses;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace QQCourseBot
{
    public class Program
    {
        public static bool WhiteListEnabled;
        public static string License = "Copyright (C) 2020  Ruixuan Tu\nThis program comes with ABSOLUTELY NO WARRANTY with GNU GPL v3 license. This is free software, and you are welcome to redistribute it under certain conditions; go to https://www.gnu.org/licenses/gpl-3.0.html for details.";
        public static Dictionary<long, GroupInfo> Groups = new Dictionary<long, GroupInfo>();
        public static CQHTTPClient client;
        public static List<long> WhiteList;
        public static List<string> ResponseMentioned;
        public static List<TencentScheduledMeeting> ScheduledMeetings;
        public static PersonalInfo Personal;
        public static Random random = new Random();
        public static DateTime EndTime = new DateTime();

        public static void Init()
        {
            client = new CQHTTPClient(
                access_url: "http://127.0.0.1:5700",
                listen_port: 8080
            );
            if (!File.Exists("personal.config.json"))
            {
                PersonalInfo personal = new PersonalInfo();
                personal.Name = "testname";
                personal.QQ = "123456789";
                File.WriteAllText("personal.config.json", JsonConvert.SerializeObject(personal));
            } else
            {
                Personal = JsonConvert.DeserializeObject<PersonalInfo>(File.ReadAllText("personal.config.json"));
            }
            if (!File.Exists("whitelist.config.json"))
            {
                WhiteList = new List<long>();
                File.WriteAllText("whitelist.config.json", JsonConvert.SerializeObject(WhiteList));
            } else
            {
                WhiteList = JsonConvert.DeserializeObject<List<long>>(File.ReadAllText("whitelist.config.json"));
            }
            if(!File.Exists("response.config.json"))
            {
                ResponseMentioned = new List<string>();
                ResponseMentioned.Add("My internet is poor.");
                ResponseMentioned.Add("I am restarting my router.");
                ResponseMentioned.Add("My device has no battery now.");
                File.WriteAllText("response.config.json", JsonConvert.SerializeObject(ResponseMentioned));
            } else
            {
                ResponseMentioned = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("response.config.json"));
            }
            if (!File.Exists("meetings.config.json"))
            {
                ScheduledMeetings = new List<TencentScheduledMeeting>();
                File.WriteAllText("meetings.config.json", JsonConvert.SerializeObject(ScheduledMeetings));
            }
            else
            {
                ScheduledMeetings = JsonConvert.DeserializeObject<List<TencentScheduledMeeting>>(File.ReadAllText("meetings.config.json"));
            }
            WhiteListEnabled = WhiteList.Count > 0;
        }

        public static string Mentioned()
        {
            return ResponseMentioned[random.Next(0, ResponseMentioned.Count)];
        }

        public static async void Send(long groupId, Message message)
        {
            Console.WriteLine("[RESPOND] " + message.ToString());
            await client.SendMessageAsync(
                MessageType.group_,
                groupId,
                message
            );
        }

        public static void Main()
        {
            Console.WriteLine("QQ Course Bot");
            Console.WriteLine();
            Console.WriteLine(License);
            Console.WriteLine();
            Init();
            Console.WriteLine("Running...");
            Console.WriteLine("Press enter key to exit.");
            client.OnEventAsync += async (client, e) =>
            {
                if (e is GroupMessageEvent)
                {
                    var me = (e as GroupMessageEvent);
                    foreach (TencentScheduledMeeting i in ScheduledMeetings)
                    {
                        if (DateTime.Now < EndTime) break;
                        if (i.StartTime < DateTime.Now && i.EndTime > DateTime.Now)
                        {
                            TencentMeeting.InvokeWemeet(i.Url);
                            EndTime = i.EndTime;
                            break;
                        }
                    }
                    if (WhiteListEnabled)
                    {
                        bool inWhiteList = false;
                        foreach (long i in WhiteList) {
                            if (i == me.group_id)
                            {
                                inWhiteList = true;
                                break;
                            }
                        }
                        if (!inWhiteList) return new EmptyResponse();
                    }
                    string ThisMessage = me.message.ToString();
                    if (!Groups.ContainsKey(me.group_id))
                    {
                        Groups.Add(me.group_id, new GroupInfo());
                    }
                    if (ThisMessage.ToLower() == Groups[me.group_id].LastMessage && !ThisMessage.Contains(' '))
                    {
                        Groups[me.group_id].MessageCount++;
                    }
                    else
                    {
                        Groups[me.group_id].MessageCount = 1;
                        Groups[me.group_id].Sent = false;
                        Groups[me.group_id].RepeatCount = random.Next(2, 11);
                    }
                    Groups[me.group_id].LastMessage = me.message.ToString().ToLower();
                    Console.WriteLine("[INFO] Time: " + DateTime.Now + "; Count: " + Groups[me.group_id].MessageCount + "; GroupID: " + me.group_id + "; Message: " + ThisMessage);
                    if (ThisMessage.ToLower().Contains(Personal.Name) || ThisMessage.Contains("[CQ:at,qq=" + Personal.QQ + "]"))
                    {
                        Console.WriteLine("[WARNING] You have been mentioned!!!");
                        Thread.Sleep(random.Next(3000, 6000));
                        Send(me.group_id, new Message(new ElementText(Mentioned())));
                    }
                    if (ThisMessage.Contains("https://meeting.tencent.com"))
                    {
                        ThisMessage = ThisMessage.Replace("会议时间：", "会议时间:");
                        if (ThisMessage.Contains("会议时间:") && ThisMessage.Contains("预定的会议"))
                        {
                            TencentScheduledMeeting meeting = new TencentScheduledMeeting();
                            int start = ThisMessage.IndexOf("会议时间:");
                            int end = ThisMessage.IndexOf(' ', start);
                            string date = ThisMessage.Substring(start + 5, end - start - 5);
                            Console.WriteLine("[WEMEET] Date: " + date);
                            start = end;
                            end = ThisMessage.IndexOf("-", start);
                            string startTime = ThisMessage.Substring(start + 1, end - start - 1);
                            Console.WriteLine("[WEMEET] StartTime: " + startTime);
                            meeting.StartTime = DateTime.Parse(date + " " + startTime);
                            meeting.StartTime = meeting.StartTime.AddMinutes(-random.Next(5, 20));
                            Console.WriteLine("[WEMEET] JoinTime: " + meeting.StartTime.ToString());
                            start = end;
                            end = ThisMessage.IndexOf("。", start);
                            if (end == -1) end = ThisMessage.IndexOf("\r\n", start);
                            if (end == -1) end = ThisMessage.IndexOf("\n", start);
                            if (end == -1) end = ThisMessage.IndexOf("\r", start);
                            string endTime = ThisMessage.Substring(start + 1, end - start - 1);
                            Console.WriteLine("[WEMEET] EndTime: " + endTime);
                            meeting.EndTime = DateTime.Parse(date + " " + endTime);
                            start = ThisMessage.IndexOf("https://meeting.tencent.com");
                            end = ThisMessage.Length;
                            for (int i = start; i < ThisMessage.Length; i++)
                            {
                                if (ThisMessage[i] == ' ' || ThisMessage[i] == '\n' || ThisMessage[i] == '\r' || ThisMessage[i] == ']' || ThisMessage[i] == ',')
                                {
                                    end = i;
                                    break;
                                }
                            }
                            meeting.Url = ThisMessage.Substring(start, end - start);
                            ScheduledMeetings.Add(meeting);
                            File.WriteAllText("meetings.config.json", JsonConvert.SerializeObject(ScheduledMeetings));
                            Console.WriteLine("[WEMEET] Scheduled");
                        }
                        else {
                            int start = ThisMessage.IndexOf("https://meeting.tencent.com");
                            int end = ThisMessage.Length;
                            for (int i = start; i < ThisMessage.Length; i++)
                            {
                                if (ThisMessage[i] == ' ' || ThisMessage[i] == '\n' || ThisMessage[i] == '\r' || ThisMessage[i] == ']' || ThisMessage[i] == ',')
                                {
                                    end = i;
                                    break;
                                }
                            }
                            TencentMeeting.InvokeWemeet(ThisMessage.Substring(start, end - start));
                        }
                    }
                    if (ThisMessage.ToLower().Contains("please send"))
                    {
                        int addLen;
                        int start = ThisMessage.ToLower().IndexOf("please send me the");
                        addLen = 18;
                        if (start == -1)
                        {
                            start = ThisMessage.ToLower().IndexOf("please send me one");
                            addLen = 18;
                        }
                        if (start == -1)
                        {
                            start = ThisMessage.ToLower().IndexOf("please send me an");
                            addLen = 17;
                        }
                        if (start == -1)
                        {
                            start = ThisMessage.ToLower().IndexOf("please send me a");
                            addLen = 16;
                        }
                        if (start == -1)
                        {
                            start = ThisMessage.ToLower().IndexOf("please send me");
                            addLen = 14;
                        }
                        if (start == -1)
                        {
                            start = ThisMessage.ToLower().IndexOf("please send");
                            addLen = 11;
                        }
                        start += addLen;
                        int end = ThisMessage.ToLower().IndexOf(' ', start + 1);
                        if (end == -1)
                        {
                            Send(me.group_id, new Message(new ElementText(ThisMessage.Substring(start, ThisMessage.Length - start))));
                        }
                        else
                        {
                            Send(me.group_id, new Message(new ElementText(ThisMessage.Substring(start, end - start))));
                        }
                        Groups[me.group_id].Sent = true;
                    }
                    if (Groups[me.group_id].MessageCount == Groups[me.group_id].RepeatCount && !Groups[me.group_id].Sent)
                    {
                        Send(me.group_id, new Message(new ElementText(ThisMessage)));
                    }
                }
                return new EmptyResponse();
            };
            Console.ReadLine();
        }
    }
}
