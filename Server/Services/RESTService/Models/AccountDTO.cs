using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RESTService.Extensions;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace RESTService.Models
{
    public class AccountDTO : DTOBase, IValidatableObject
    {
        public string AccountNumber { get; set; }
        public AccountOwnerDTO AccountOwner { get; set; }

        private readonly IValidator _validator;

        public AccountDTO()
        {
            _validator = new AccountDTOValidator();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return _validator.Validate(this).ToValidationResult();
        }
    }
}