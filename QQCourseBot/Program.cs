using System;
using cqhttp.Cyan.Clients;
using cqhttp.Cyan.Events.CQEvents;
using cqhttp.Cyan.Events.CQResponses;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;
using System.Collections.Generic;

namespace QQCourseBot
{
    public class Program
    {
        public static string[] response;
        public static Dictionary<long, GroupInfo> Groups = new Dictionary<long, GroupInfo>();
        public static int RepeatCount = 5;
        public static Random random = new Random();
        public static string License = "Copyright (C) 2020  Ruixuan Tu\nThis program comes with ABSOLUTELY NO WARRANTY with GNU GPL v3 license. This is free software, and you are welcome to redistribute it under certain conditions; go to https://www.gnu.org/licenses/gpl-3.0.html for details.";

        public static void Init()
        {
            response = new string[] {
                "My internet is poor.",
                "I am restarting my router.",
                "My device has no battery now."
            };
            RepeatCount = random.Next(1, 10);
        }

        public static string Mentioned()
        {
            return response[random.Next(0, response.Length)];
        }

        public static void Main()
        {
            Console.WriteLine("QQ Course Bot");
            Console.WriteLine();
            Console.WriteLine(License);
            Console.WriteLine();
            Init();
            CQHTTPClient client = new CQHTTPClient(
                access_url: "http://127.0.0.1:5700",
                listen_port: 8080
            );
            Console.WriteLine("Running...");
            Console.WriteLine("Press enter key to exit.");
            client.OnEventAsync += async (client, e) =>
            {
                if (e is GroupMessageEvent)
                {
                    var me = (e as GroupMessageEvent);
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
                    }
                    Groups[me.group_id].LastMessage = me.message.ToString().ToLower();
                    Console.WriteLine("[INFO] MessageCount: " + Groups[me.group_id].MessageCount + "; GroupID: " + me.group_id + "; Message: " + ThisMessage);
                    if (ThisMessage.Contains(PersonalInfo.name) || ThisMessage.Contains("@" + PersonalInfo.nickname))
                    {
                        Console.WriteLine("[WARNING] You have been mentioned!!!");
                        await client.SendMessageAsync(
                            MessageType.group_,
                            me.group_id,
                            new Message(
                                new ElementText(Mentioned())
                            ));
                    }
                    if (ThisMessage.Contains("please send"))
                    {
                        bool flagSent = false;
                        int start = ThisMessage.IndexOf("please send me");
                        if (start == -1) start = ThisMessage.IndexOf("please send") + 11;
                        else start += 14;
                        int end;
                        for (end = start + 1; end < ThisMessage.Length; end++)
                        {
                            if (ThisMessage[end] == ' ')
                            {
                                Console.WriteLine("[RESPOND] Send: " + ThisMessage.Substring(start, end - start));
                                await client.SendMessageAsync(
                                    MessageType.group_,
                                    me.group_id,
                                    new Message(
                                        new ElementText(ThisMessage.Substring(start, end - start))
                                    )
                                );
                                flagSent = true;
                                break;
                            }
                        }
                        if (flagSent == false)
                        {
                            Console.WriteLine("[RESPOND] Send: " + ThisMessage.Substring(start, ThisMessage.Length - start));
                            await client.SendMessageAsync(
                                MessageType.group_,
                                me.group_id,
                                new Message(
                                    new ElementText(ThisMessage.Substring(start, ThisMessage.Length - start))
                                )
                            );
                        }
                    }
                    if (Groups[me.group_id].MessageCount > RepeatCount)
                    {
                        Console.WriteLine("[RESPOND] Repeat: " + ThisMessage);
                        await client.SendMessageAsync(
                            MessageType.group_,
                            me.group_id,
                            me.message);
                        RepeatCount = random.Next(1, 10);
                    }
                }
                return new EmptyResponse();
            };
            Console.ReadLine();
        }
    }
}
