using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatsAppApi.Facades
{
    public class EndPointAllocator
    {
        private Dictionary<string, List<string>> connections = new Dictionary<string, List<string>>();
        private Dictionary<string, EndPointAssignment> assignments = new Dictionary<string, EndPointAssignment>();

        public void Add(EndPointAssignment assignment)
        {
            this.assignments[assignment.EndUserPhoneNumber] =  assignment;
        }

        public EndPointAssignment GetAssignment(string endUserPhoneNumber)
        {
            if (!this.assignments.ContainsKey(endUserPhoneNumber)) return null;
            return this.assignments[endUserPhoneNumber];
        }

        public void Add(string from, string to)
        {
            if (!connections.ContainsKey(to))
            {
                connections.Add(to, new List<string>());
            }
            connections[to].Add(from);
        }

        public bool ShouldWarn(string from, string to)
        {
            return true;
        }

        public bool CanConnect(string from, string to)
        {
            return (connections.ContainsKey(to) && connections[to].Contains(from));
        }
    }

    public class EndPointAssignment
    {
        public string EndUserPhoneNumber { get; set; }
        public string WhatsAppPhoneNumber { get; set; }

    }

}
