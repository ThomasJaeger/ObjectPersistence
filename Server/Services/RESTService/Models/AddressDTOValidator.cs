using FluentValidation;

namespace RESTService.Models
{
    public class AddressDTOValidator: AbstractValidator<AddressDTO>
    {
        public AddressDTOValidator()
        {
            RuleFor(addressDTO => addressDTO.AddressType)
                .SetValidator(new IsAddressTypeOkValidator())
                .SetValidator(new AddressTypeDTOValidator());
            RuleFor(addressDTO => addressDTO.City).NotNull();
            RuleFor(addressDTO => addressDTO.Country).NotNull();
            RuleFor(addressDTO => addressDTO.Line1).NotNull();
            RuleFor(addressDTO => addressDTO.State).NotNull();
            RuleFor(addressDTO => addressDTO.Zip).NotNull();
        }
    }
}