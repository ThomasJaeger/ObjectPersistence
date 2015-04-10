using FluentValidation;

namespace RESTService.Models
{
    public class AccountDTOValidator : AbstractValidator<AccountDTO>
    {
        public AccountDTOValidator()
        {
            RuleFor(accountDTO => accountDTO.AccountNumber).NotNull();
            RuleFor(accountDTO => accountDTO.AccountOwner).SetValidator(new AccountOwnerDTOValidator());
        }
    }
}