using MongoDB.Bson.Serialization.Serializers;
using System.Collections.Generic;

namespace SharedClasses
{
    public class PayLoadResponse
    {
        public string CashierID { get; set; }
        public string CurrencyCode { get; set; }
        //public List<PairInfo> AdditionalInfo { get; set; }
        public List<IDictionary<string,string>> AdditionalInfo { get; set; }
    }
}