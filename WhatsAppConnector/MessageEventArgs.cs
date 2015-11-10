using System;

namespace WhatsAppApi.Facades
{
    public class MessageEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string From { get; set; }
        public string Text { get; set; }


        public MessageEventArgs() { }
        public MessageEventArgs(string from, string name, string text)
        {
            this.From = from;
            this.Text = text;
            this.Name = name;
        }
    }
}
