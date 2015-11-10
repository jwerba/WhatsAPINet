using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatsAppApi.Facades
{
    public class MediaEventArgs : EventArgs
    {
        public string From { get; set; }
        public string Id { get; set; }
        public string FileName { get; set; }
        public int Size { get; set; }
        
        public string Url { get; set; }

        public byte[] Preview { get; set; }
        public byte[]  Bytes { get; set; }

    }
}
