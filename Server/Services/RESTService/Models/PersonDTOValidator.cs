using FluentValidation;

namespace RESTService.Models
{
    public class PersonDTOValidator : AbstractValidator<PersonDTO>
    {
        public PersonDTOValidator()
        {
            RuleFor(personDTO => personDTO.Email)
                .NotNull()
                .EmailAddress()
                .WithName("Email")
                .WithMessage("Please ensure you have entered a valid {PropertyName}");
            RuleFor(personDTO => personDTO.FirstName).NotNull();
            RuleFor(personDTO => personDTO.LastName).NotNull();
            RuleFor(personDTO => personDTO.HomeAddress).SetValidator(new AddressDTOValidator());
            RuleFor(personDTO => personDTO.WorkAddress).SetValidator(new AddressDTOValidator());
        }
    }
}