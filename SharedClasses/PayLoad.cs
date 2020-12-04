using MongoDB.Bson.Serialization.Serializers;
using System.Collections.Generic;

namespace SharedClasses
{
    public class PayLoad
    {
        public string CashierID { get; set; }
        public string CurrencyCode { get; set; }
        public int Length { get; set; }
        public string SessionId { get; set; }
        //public List<PairInfo> AdditionalInfo { get; set; }
        public Dictionary<string,string> AdditionalInfo { get; set; }
    }
}