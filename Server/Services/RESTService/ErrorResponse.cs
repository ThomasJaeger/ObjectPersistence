using System.Collections.Generic;

namespace RESTService
{
    public class ErrorResponse
    {
        public int Code { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public List<string> Validations { get; set; }
        public string ItemPotency { get; set; } 
    }
}