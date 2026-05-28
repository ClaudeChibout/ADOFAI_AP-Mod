using System;

namespace ADOFAI_AP
{
    internal class APMessage
    {
        public string Time { get; }
        public string Category { get; }
        public string Text { get; }
        public string RichText { get; }

        public APMessage(string category, string text, string richText = null)
        {
            Time = DateTime.Now.ToString("HH:mm:ss");
            Category = category;
            Text = text;
            RichText = richText ?? text;
        }
    }
}
