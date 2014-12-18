namespace DomainModel
{
    public class Person: DomainObject
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public Person(string name) : base(name)
        {
        }

        public Person()
        {
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
    }
}
