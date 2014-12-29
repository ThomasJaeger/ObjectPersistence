namespace DTO
{
    public class DTOBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Default { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// We need to override Equals so that we can use the DTO for certain
        /// UI funcationalities such as comboBox.Items.IndexOf to work correctly
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            var other = obj as DTOBase;
            if (other == null) return false;
            return Id == other.Id;
        }
    }
}
