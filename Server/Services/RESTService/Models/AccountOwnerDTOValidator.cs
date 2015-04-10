using FluentValidation;

namespace RESTService.Models
{
    public class AccountOwnerDTOValidator : AbstractValidator<AccountOwnerDTO>
    {
        public AccountOwnerDTOValidator()
        {
            RuleFor(accountOwnerDTO => accountOwnerDTO.Account).SetValidator(new AccountDTOValidator());
            RuleFor(accountOwnerDTO => accountOwnerDTO.Email).NotNull();
            RuleFor(accountOwnerDTO => accountOwnerDTO.Password).NotNull();
        }
    }
}