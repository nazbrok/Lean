using System;
using Newtonsoft.Json;

namespace QuantConnect.Brokerages.Bybit.Messages
{
    public class Test
    {
        public int Toto { get; set; }
    }

    public class SubscriptionRequest
    {
        [JsonProperty("op")]
        public string Operation { get; set; }

        [JsonProperty("args")]
        public string[] Args { get; set; }
    }


    public class SubscriptionResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("conn_id")]
        public Guid ConnexionId { get; set; }

        [JsonProperty("ret_msg")]
        public string RetMsg { get; set; }

        public SubscriptionRequest Request { get; set; }
    }

    public class BaseTopic
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }

    #region TradeData
    public class TradeData
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("trade_time_ms")]
        public long TradeTime { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("tick_direction")]
        public string TickDirection { get; set; }

        [JsonProperty("trade_id")]
        public Guid TradeId { get; set; }

        [JsonProperty("cross_seq")]
        public int CrossSeq { get; set; }

    }

    public class TradeTopic : BaseTopic
    {
        [JsonProperty("data")]
        public TradeData Data { get; set; }
    }
    #endregion
}
