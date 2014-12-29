using System.Collections.Generic;
using System.Text.RegularExpressions;
using AutoMapper;
using DomainModel;
using PersistenceService;
using RESTService.Models;

namespace RESTService
{
    /// <summary>
    /// IMPORTANT: Keep in mind that any business rules that can apply to
    /// a domain class, should stay inside that domain class. 
    /// 
    /// System wide rules and processing such as workflows, can be handled
    /// in this ApplicationFacade. Think of the ApplicationFacade as a 
    /// conductor like a conductor in an orchestra.
    /// 
    /// The ApplicationFacade is a great central point to do any kind of
    /// logging, validations, etc. It serves as tne entry point into the
    /// remainder of the sub-system of your server application layer.
    /// </summary>
    public class ApplicationFacade
    {
        private const string _matchEmailPattern = @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@" +
                                                  @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\." +
                                                  @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|" +
                                                  @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

        private static readonly ApplicationFacade _instance = new ApplicationFacade();

        private ApplicationFacade()
        {
            InitializeSystem();
        }

        /// <summary>
        /// Initialize system, this includes the seed data of the system
        /// such as central cloud logging, domain types, company information, etc.
        /// </summary>
        private void InitializeSystem()
        {
            // Connect to our persistence, this connection stays open
            // sicne our provider model for the persistance guaranties only
            // one instance is running (per node).
            Persistence.Instance.Provider.Connect();

            // Detect if we need to create our initial seed data of the system
            CreateSeedData();
        }

        private void CreateSeedData()
        {
            List<AddressType> list = Persistence.Instance.Provider.GetObjects<AddressType>();
            if (list.Count == 0)
            {
                // We need to create our initial seed data of the system
                // If this is the very first node in a cloud-cluster, this 
                // should be the only node running to cold-start the cloud system.
                // Once this process is complete, the rest of the nodes can be started.

                // Create Address Types
                Persistence.Instance.Provider.Save(AddressType.Home);
                Persistence.Instance.Provider.Save(AddressType.Work);

                // Log the completion of creating the new seed data in the central cloud logger.
                // ...
            }
        }

        public static List<Person> GetAllPeople()
        {
            return Persistence.Instance.Provider.GetObjects<Person>();
        }

        public static Person CreatePerson(PersonDTO dto)
        {
            // IMPORTANT:   Do not use AutoMapper to map from an incoming DTO to a domain object
            //              For example, do not do this: Person obj = Mapper.Map<Person>(dto);
            //
            // The reasons for NOT using AutoMapper for incoming DTOs are many:
            //              1. The control of ceating domain objects should stay in the domain layer
            //              2. Processes and workflows may need to be kicked of during creation
            //                 of specific domain objects and embedded objects
            //              3. Validations may need to be executed during the creation process
            //                 such as data cleansing, apply business rules before importing, etc.
            //              4. Security and safety of incoming data may not allow an automated creation
            //              5. Service caching on the service layer may be impacted in clusters

            Person person = Person.NewInstance();  // Uses factory method to apply business rules first
            MapPerson(person, dto);

            // Once mapping the DTO to the domain object is complete, we
            // can do any verifications and additional processing such as logging,
            // sending notifications, kicking off a workflow etc.

            Persistence.Instance.Provider.Save(person);
            return person;
        }

        public static void UpdatePerson(Person person, PersonDTO dto)
        {
            // IMPORTANT:   Do not use AutoMapper to map from an incoming DTO to a domain object
            //              For example, do not do this: Mapper.Map(dto, person);
            //
            // The reasons for NOT using AutoMapper for incoming DTOs are many:
            //              1. The control of updating domain objects should stay in the domain layer
            //              2. Processes and workflows may need to be kicked of during updates
            //                 of specific domain objects and embedded objects
            //              3. Validations may need to be executed during the update process
            //                 such as data cleansing, apply business rules before importing, etc.
            //              4. Security and safety of incoming data may not allow an automated update
            //              5. Service caching on the service layer may be impacted in clusters

            MapPerson(person, dto);
            Persistence.Instance.Provider.Save(person);
        }

        /// <summary>
        /// Manually map DTO to domain object, see notes above why
        /// </summary>
        /// <param name="person"></param>
        /// <param name="dto"></param>
        private static void MapPerson(Person person, PersonDTO dto)
        {
            person.FirstName = dto.FirstName;
            person.LastName = dto.LastName;
            person.Email = dto.Email;

            MapAddress(person.HomeAddress, dto.HomeAddress);
            MapAddress(person.WorkAddress, dto.WorkAddress);
        }

        /// <summary>
        /// Manually map DTO to domain object, see notes above why
        /// </summary>
        /// <param name="address"></param>
        /// <param name="dto"></param>
        private static void MapAddress(Address address, AddressDTO dto)
        {
            if (address == null)
                address = Address.NewInstance();

            // Make sure address type exists in the system, if not, default to home address type
            AddressType addressType = Persistence.Instance.Provider.GetObjectById<AddressType>(dto.AddressType.Id);
            if (addressType != null)
                address.AddressType = addressType;

            address.Line1 = dto.Line1;
            address.Line2 = dto.Line2;
            address.Line3 = dto.Line3;
            address.City = dto.City;
            address.State = dto.State;
            address.Zip = dto.Zip;
            address.Country = dto.Country;
        }

        public static void DeleteAccount(Person person)
        {
            Persistence.Instance.Provider.DeleteById<Person>(person.Id);
        }

        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            return Regex.IsMatch(email, _matchEmailPattern);
        }

        public static List<AddressType> GetAllAddressTypes()
        {
            return Persistence.Instance.Provider.GetObjects<AddressType>();
        }

        public static AddressType CreateAddressType(AddressTypeDTO dto)
        {
            AddressType obj = Mapper.Map<AddressType>(dto);
            Persistence.Instance.Provider.Save(obj);
            return obj;
        }

        public static void DeleteAddressType(AddressType addressType)
        {
            Persistence.Instance.Provider.DeleteById<AddressType>(addressType.Id);
        }

        public static void UpdateAddressType(AddressType addressType, AddressTypeDTO dto)
        {
            Mapper.Map(dto, addressType);
            Persistence.Instance.Provider.Save(addressType);
        }
    }
}