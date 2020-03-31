using System;

namespace QQCourseBot
{
    public class GroupInfo
    {
        public int MessageCount = 1;
        public string LastMessage = string.Empty;
        public int RepeatCount = 10;
        public bool Sent = false;
        public DateTime LastRepeatTime = new DateTime();
    }
}
