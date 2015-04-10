using FluentValidation;

namespace RESTService.Models
{
    public class AddressTypeDTOValidator: AbstractValidator<AddressTypeDTO>
    {
        public AddressTypeDTOValidator()
        {
            RuleFor(addressTypeDTO => addressTypeDTO.Name).NotNull();
        }
    }
}