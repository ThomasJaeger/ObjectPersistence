namespace RESTService.Models
{
    public class AccountDTO: DTOBase
    {
        public string AccountNumber { get; set; }
        public AccountOwnerDTO AccountOwner { get; set; }
    }
}