using System;
using System.Collections.Generic;
using System.Linq;
using DomainModel;
using DTO;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using PersistenceService;
using Services;

namespace ProviderIntegrationTests
{
    [TestFixture]
    public class ProviderIntegrationTests
    {
        private CompareLogic _compareLogic;

        [TestFixtureSetUp]
        public void SetupAllTests()
        {
            _compareLogic = CreateComparator();
            Persistence.Instance.Provider.Connect();
        }

        [TestFixtureTearDown]
        public void TearDownAfterAllTestsRan()
        {
            Assert.IsTrue(Persistence.Instance.Provider.Disconnect());
        }

        [SetUp]
        public void InitForEachTest()
        {
            Persistence.Instance.Provider.DeleteAllObjects();
            RESTService.ApplicationFacade.CreateSeedData();
        }

        [TearDown]
        public void DisposeAfterEachTest()
        {

        }

        private static CompareLogic CreateComparator()
        {
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.CompareStaticFields = false;
            compareLogic.Config.CompareStaticProperties = false;
            compareLogic.Config.MaxDifferences = int.MaxValue;
            compareLogic.Config.ShowBreadcrumb = true;
            compareLogic.Config.TreatStringEmptyAndNullTheSame = false;
            compareLogic.Config.ClassTypesToIgnore.Add(typeof(DateTime));
            return compareLogic;
        }

        /// <summary>
        /// First thing we should test is the existence of our domain types such as AddressType 
        /// objects. Since these objects will be created the very first time the system starts 
        /// up. So, these types must exist in order to have a functional domain model.
        /// </summary>
        [Test]
        public void GetAddressTypes()
        {
            List<AddressType> list = Persistence.Instance.Provider.GetObjects<AddressType>();
            Assert.AreEqual(2, list.Count);

            foreach (var addressType in list)
            {
                if (addressType.DisplayName == "Home Address")
                {
                    // Do deep object comparison
                    ComparisonResult comparisonResult = _compareLogic.Compare(AddressType.Home, addressType);
                    Assert.AreEqual(true, comparisonResult.AreEqual, comparisonResult.DifferencesString);
                }
                else
                {
                    // Do deep object comparison
                    ComparisonResult comparisonResult = _compareLogic.Compare(AddressType.Work, addressType);
                    Assert.AreEqual(true, comparisonResult.AreEqual, comparisonResult.DifferencesString);
                }
            }
        }

        [Test]
        public void SaveDomainObject()
        {
            DomainObject obj = new DomainObject();
            obj.Name = "Test Name on " + DateTime.Now;
            Assert.IsTrue(Persistence.Instance.Provider.Save(obj));

            DomainObject savedObject = Persistence.Instance.Provider.GetObjectById<DomainObject>(obj.Id);
            Assert.AreEqual(obj.Id, savedObject.Id);
            Assert.AreEqual(obj.Name, savedObject.Name);
        }

        /// <summary>
        /// Create a new person but do not provide HomeAddress nor WorkAddress
        /// This test is run from the client point of view, so, make sure the service is running.
        /// </summary>
        [Test]
        public void CreatePersonExpectFailure()
        {
            // We should have no persons before starting this test
            List<Person> persons = Persistence.Instance.Provider.GetObjects<Person>();
            Assert.AreEqual(0, persons.Count());

            PersonDTO personDto = CreateTestPerson();
            PersonDTO createdPersonDto = RESTServices.CreatePerson(personDto);

            Assert.AreEqual(ResultType.Failure, RESTServices.ResultType);
            Assert.IsNotNull(RESTServices.ErrorInfo);
            Assert.AreEqual(10004, RESTServices.ErrorInfo.Code);
            Assert.AreEqual(ErrorCodeType.InvalidRequest, RESTServices.ErrorInfo.ErrorCodeType);
            Assert.AreEqual("Invalid argument", RESTServices.ErrorInfo.Message);
            Assert.AreEqual("Home address type is null", RESTServices.ErrorInfo.Description);
        }

        /// <summary>
        /// Create a new person with valid HomeAddress and WorkAddress
        /// This test is run from the client point of view, so, make sure the service is running.
        /// </summary>
        [Test]
        public void CreatePerson()
        {
            // We should have no persons before starting this test
            List<Person> persons = Persistence.Instance.Provider.GetObjects<Person>();
            Assert.AreEqual(0, persons.Count());

            PersonDTO personDto = CreateTestPerson();
            personDto.HomeAddress = CreateHomeAddress();
            personDto.WorkAddress = CreateWorkAddress();
            PersonDTO createdPersonDto = RESTServices.CreatePerson(personDto);

            Assert.AreEqual(ResultType.Success, RESTServices.ResultType);
            Assert.IsNotNull(createdPersonDto);
            Assert.IsNull(RESTServices.ErrorInfo);

            Person savedPerson = Persistence.Instance.Provider.GetObjectById<Person>(createdPersonDto.Id);
            Assert.NotNull(savedPerson);
        }

        private PersonDTO CreateTestPerson()
        {
            PersonDTO personDto = new PersonDTO();
            personDto.FirstName = "Thomas";
            personDto.LastName = "Jaeger";
            personDto.Email = "test@email.com";
            personDto.Name = "Test person on " + DateTime.Now;
            return personDto;
        }

        private AddressDTO CreateHomeAddress()
        {
            AddressDTO addressDto = new AddressDTO();
            addressDto.AddressType = new AddressTypeDTO();
            addressDto.AddressType.Id = AddressType.Home.Id;
            addressDto.AddressType.Name = AddressType.Home.DisplayName;
            addressDto.Line1 = "Home Line 1";
            addressDto.Line2 = "Home Line 2";
            addressDto.Line3 = "Home Line 3";
            addressDto.City = "San Francisco";
            addressDto.State = "CA";
            addressDto.Zip = "12345";
            addressDto.Country = "USA";
            return addressDto;
        }

        private AddressDTO CreateWorkAddress()
        {
            AddressDTO addressDto = new AddressDTO();
            addressDto.AddressType = new AddressTypeDTO();
            addressDto.AddressType.Id = AddressType.Work.Id;
            addressDto.AddressType.Name = AddressType.Work.DisplayName;
            addressDto.Line1 = "Work Line 1";
            addressDto.Line2 = "Work Line 2";
            addressDto.Line3 = "Work Line 3";
            addressDto.City = "San Jose";
            addressDto.State = "CA";
            addressDto.Zip = "54321";
            addressDto.Country = "USA";
            return addressDto;
        }
    }
}
