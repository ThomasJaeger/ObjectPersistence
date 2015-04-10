using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RESTService.Extensions;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace RESTService.Models
{
    public class AddressTypeDTO : DTOBase, IValidatableObject
    {
        private readonly IValidator _validator;

        public AddressTypeDTO()
        {
            _validator = new AddressTypeDTOValidator();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return _validator.Validate(this).ToValidationResult();
        }
    }
}