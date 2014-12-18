using System.Collections.Generic;
using System.Text.RegularExpressions;
using AutoMapper;
using DomainModel;
using PersistenceService;
using RESTService.Models;

namespace RESTService
{
    public class ApplicationFacade
    {
        private const string _matchEmailPattern = @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@" +
                                                  @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\." +
                                                  @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|" +
                                                  @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

        private static readonly ApplicationFacade _instance = new ApplicationFacade();

        /// <summary>
        /// The ApplicationFacade is a great central point to do any kind of
        /// loggin, validations, etc. It serves as tne entry point into the
        /// remainder of the sub-system of your server application layer.
        /// </summary>
        private ApplicationFacade()
        {
            // Do any initialization here
        }

        public static List<Person> GetAllPeople()
        {
            return Persistence.Instance.Provider.GetObjects<Person>();
        }

        public static Person CreatePerson(PersonDTO dto)
        {
            Person person = Mapper.Map<Person>(dto);
            Persistence.Instance.Provider.Save(person);
            return person;
        }

        public static void UpdatePerson(Person person, PersonDTO dto)
        {
            person.FirstName = dto.FirstName;
            person.LastName = dto.LastName;
            person.Email = dto.Email;
            Persistence.Instance.Provider.Save(person);
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
    }
}