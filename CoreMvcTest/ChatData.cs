using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMvcTest
{
    public class ChatData
    {
        public string Type { get; set; } = "system";
        public DateTime Time { get; set; } = DateTime.Now;
        public string Username { get; set; }
        public string Content { get; set; }        
    }
}
