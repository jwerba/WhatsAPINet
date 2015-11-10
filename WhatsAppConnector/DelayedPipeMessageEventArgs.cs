using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatsAppApi.Facades
{
    public class DelayedPipeMessageEventArgs : EventArgs
    {
        public Message Message { get; set; } 
    }
}
