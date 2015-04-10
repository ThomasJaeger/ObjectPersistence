namespace Services
{
    public class ErrorData
    {
        public string Id { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return Id + ": " + Message;
        }
    }
}