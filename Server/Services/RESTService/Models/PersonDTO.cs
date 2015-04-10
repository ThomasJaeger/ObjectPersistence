using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RESTService.Extensions;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace RESTService.Models
{
    public class PersonDTO : DTOBase, IValidatableObject
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public AddressDTO HomeAddress { get; set; }
        public AddressDTO WorkAddress { get; set; }

        private readonly IValidator _validator;

        public PersonDTO()
        {
            _validator = new PersonDTOValidator();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return _validator.Validate(this).ToValidationResult();
        }
    }
}
