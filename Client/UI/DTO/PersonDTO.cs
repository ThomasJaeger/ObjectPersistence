namespace DTO
{
    public class PersonDTO: DTOBase
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            return FirstName + " " + LastName + ", " + Email + ", " + Id;
        }
    }
}
