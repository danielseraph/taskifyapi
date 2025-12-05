using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskify.Services.DTOs
{
    public class ChatRequest
    {
        public string Message { get; set; }
        public List<string>? History { get; set; }
    }
}
