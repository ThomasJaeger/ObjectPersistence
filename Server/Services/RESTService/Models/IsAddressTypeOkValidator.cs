using DomainModel;
using FluentValidation.Validators;
using PersistenceService;

namespace RESTService.Models
{
    public class IsAddressTypeOkValidator : PropertyValidator
    {
        public IsAddressTypeOkValidator() : base("Property {PropertyName} must be an existing type!")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var addressTypeDTO = context.PropertyValue as AddressTypeDTO;
            if (addressTypeDTO == null)
                return false;

            var addressType = Persistence.Instance.Provider.GetObjectById<AddressType>(addressTypeDTO.Id);
            return addressType != null;
        }
    }
}