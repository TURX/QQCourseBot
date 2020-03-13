using System;
using cqhttp.Cyan.Clients;
using cqhttp.Cyan.Events.CQEvents;
using cqhttp.Cyan.Events.CQResponses;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Threading;

namespace QQCourseBot
{
    public class Program
    {
        public static List<string> ResponseMentioned;
        public static Dictionary<long, GroupInfo> Groups = new Dictionary<long, GroupInfo>();
        public static int RepeatCount = 5;
        public static Random random = new Random();
        public static string License = "Copyright (C) 2020  Ruixuan Tu\nThis program comes with ABSOLUTELY NO WARRANTY with GNU GPL v3 license. This is free software, and you are welcome to redistribute it under certain conditions; go to https://www.gnu.org/licenses/gpl-3.0.html for details.";
        public static CQHTTPClient client;
        public static List<long> WhiteList;
        public static PersonalInfo Personal;
        public static bool WhiteListEnabled;

        public static void Init()
        {
            RepeatCount = random.Next(1, 10);
            client = new CQHTTPClient(
                access_url: "http://127.0.0.1:5700",
                listen_port: 8080
            );
            if (!File.Exists("personal.config.json"))
            {
                PersonalInfo personal = new PersonalInfo();
                personal.Name = "testname";
                personal.QQ = "123456789";
                File.WriteAllText("personal.config.json", JsonSerializer.Serialize(personal));
            } else
            {
                Personal = JsonSerializer.Deserialize<PersonalInfo>(File.ReadAllText("personal.config.json"));
            }
            if (!File.Exists("whitelist.config.json"))
            {
                WhiteList = new List<long>();
                File.WriteAllText("whitelist.config.json", JsonSerializer.Serialize(WhiteList));
            } else
            {
                WhiteList = JsonSerializer.Deserialize<List<long>>(File.ReadAllText("whitelist.config.json"));
            }
            if(!File.Exists("response.config.json"))
            {
                ResponseMentioned = new List<string>();
                ResponseMentioned.Add("My internet is poor.");
                ResponseMentioned.Add("I am restarting my router.");
                ResponseMentioned.Add("My device has no battery now.");
                File.WriteAllText("response.config.json", JsonSerializer.Serialize(ResponseMentioned));
            } else
            {
                ResponseMentioned = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("response.config.json"));
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
                    string ThisMessage = me.message.ToString().ToLower();
                    if (!Groups.ContainsKey(me.group_id))
                    {
                        Groups.Add(me.group_id, new GroupInfo());
                    }
                    if (ThisMessage == Groups[me.group_id].LastMessage && !ThisMessage.Contains(' '))
                    {
                        Groups[me.group_id].MessageCount++;
                    }
                    else
                    {
                        Groups[me.group_id].MessageCount = 1;
                        Groups[me.group_id].Sent = false;
                    }
                    Groups[me.group_id].LastMessage = me.message.ToString().ToLower();
                    Console.WriteLine("[INFO] MessageCount: " + Groups[me.group_id].MessageCount + "; GroupID: " + me.group_id + "; Message: " + ThisMessage);
                    if (ThisMessage.Contains(Personal.Name) || ThisMessage.Contains("[cq:at,qq=" + Personal.QQ + "]"))
                    {
                        Console.WriteLine("[WARNING] You have been mentioned!!!");
                        Thread.Sleep(random.Next(3000, 6000));
                        Send(me.group_id, new Message(new ElementText(Mentioned())));
                    }
                    if (ThisMessage.Contains("please send"))
                    {
                        bool flagSent = false;
                        int addLen;
                        int start = ThisMessage.IndexOf("please send me the");
                        addLen = 18;
                        if (start == -1)
                        {
                            start = ThisMessage.IndexOf("please send me one");
                            addLen = 18;
                        }
                        if (start == -1)
                        {
                            start = ThisMessage.IndexOf("please send me an");
                            addLen = 17;
                        }
                        if (start == -1)
                        {
                            start = ThisMessage.IndexOf("please send me a");
                            addLen = 16;
                        }
                        if (start == -1)
                        {
                            start = ThisMessage.IndexOf("please send me");
                            addLen = 14;
                        }
                        if (start == -1)
                        {
                            start = ThisMessage.IndexOf("please send");
                            addLen = 11;
                        }
                        start += addLen;
                        int end;
                        for (end = start + 1; end < ThisMessage.Length; end++)
                        {
                            if (ThisMessage[end] == ' ')
                            {
                                Send(me.group_id, new Message(new ElementText(ThisMessage.Substring(start, end - start))));
                                flagSent = true;
                                break;
                            }
                        }
                        if (flagSent == false)
                        {
                            Send(me.group_id, new Message(new ElementText(ThisMessage.Substring(start, ThisMessage.Length - start))));
                        }
                        Groups[me.group_id].Sent = true;
                    }
                    if (Groups[me.group_id].MessageCount > RepeatCount && !Groups[me.group_id].Sent)
                    {
                        Send(me.group_id, new Message(new ElementText(ThisMessage)));
                        RepeatCount = random.Next(1, 10);
                        Groups[me.group_id].Sent = true;
                    }
                }
                return new EmptyResponse();
            };
            Console.ReadLine();
        }
    }
}
