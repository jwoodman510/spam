using System.Collections.Generic;

namespace SPAM
{
    public class Request
    {
        public string HttpMethod { get; set; }

        public List<string> Urls { get; set; }

        public IDictionary<string, string> Headers { get; set; }
    }
}
