using System;
using Newtonsoft.Json;

namespace QuantConnect.Brokerages.Bybit
{
    public class Messages
    {
        public Messages()
        {
        }

        public class Request
        {
            [JsonProperty("op")]
            public string Operation { get; set; }

            [JsonProperty("args")]
            public string[] Args { get; set; }
        }

        public class BaseMessage
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("conn_id")]
            public Guid ConnexionId { get; set; }

            [JsonProperty("ret_msg")]
            public string RetMsg { get; set; }

            public Request Request { get; set; }
        }
    }
}
