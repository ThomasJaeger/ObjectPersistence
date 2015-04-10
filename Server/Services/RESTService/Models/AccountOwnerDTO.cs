using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RESTService.Extensions;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace RESTService.Models
{
    public class AccountOwnerDTO : PersonDTO, IValidatableObject
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public AccountDTO Account { get; set; }

        private readonly IValidator _validator;

        public AccountOwnerDTO()
        {
            _validator = new AccountOwnerDTOValidator();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return _validator.Validate(this).ToValidationResult();
        }
    }
}