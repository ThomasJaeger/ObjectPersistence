namespace RESTService.Models
{
    public class PersonDTO : DTOBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public AddressDTO HomeAddress { get; set; }
        public AddressDTO WorkAddress { get; set; }
    }
}
