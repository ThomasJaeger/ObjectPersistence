using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RESTService.Extensions;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace RESTService.Models
{
    public class AddressDTO : DTOBase, IValidatableObject
    {
        public AddressTypeDTO AddressType { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }

        private readonly IValidator _validator;

        public AddressDTO()
        {
            _validator = new AddressDTOValidator();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return _validator.Validate(this).ToValidationResult();
        }
    }
}