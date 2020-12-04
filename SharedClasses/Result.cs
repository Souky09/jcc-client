using MongoDB.Bson.Serialization.Attributes;

namespace SharedClasses
{
    public class Result
    {
        public string OperationType { get; set; }
        public string ErrorCode { get; set; }
        [BsonElement("_id")]
        public string OperationID { get; set; }
        public string Status { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseContent { get; set; }

    }
}