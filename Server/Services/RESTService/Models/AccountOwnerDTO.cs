namespace RESTService.Models
{
    public class AccountOwnerDTO: PersonDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public AccountDTO Account { get; set; }
    }
}