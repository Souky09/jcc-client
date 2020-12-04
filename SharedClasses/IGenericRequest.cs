namespace SharedClasses
{
    public interface IGenericRequest
    {
        Info Info { get; set; }
        string OperationType { get; set; }
        PayLoad PayLoad { get; set; }
    }
}