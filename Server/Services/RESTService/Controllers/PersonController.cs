using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using DomainModel;
using PersistenceService;
using RESTService.Models;

namespace RESTService.Controllers
{
    [RoutePrefix("api/v1/persons")]
    [Route("{action=Get}")]
    public class PersonController : ApiController
    {
        [Route("")]
        public HttpResponseMessage Get()
        {
            List<Person> allPeople = ApplicationFacade.GetAllPeople();
            List<PersonDTO> list = Mapper.Map<List<Person>, List<PersonDTO>>(allPeople);
            return Request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "id is null");
            var obj = Persistence.Instance.Provider.GetObjectById<Person>(id);
            if (obj == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "person does not exist");
            var dto = Mapper.Map<PersonDTO>(obj);
            return Request.CreateResponse(HttpStatusCode.OK, dto);
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Post(PersonDTO dto)
        {
            if (dto == null)
                return ErrorCodeMap.CreateResponse(Request, 10003, "Person is null");
            if (string.IsNullOrEmpty(dto.Email))
                return ErrorCodeMap.CreateResponse(Request, 10004, "email is null or empty");
            if (!ApplicationFacade.ValidateEmail(dto.Email))
                return ErrorCodeMap.CreateResponse(Request, 10004, "email is invalid");
            if (string.IsNullOrEmpty(dto.FirstName))
                return ErrorCodeMap.CreateResponse(Request, 10004, "FirstName is required");
            if (string.IsNullOrEmpty(dto.LastName))
                return ErrorCodeMap.CreateResponse(Request, 10004, "LastName is required");
            Person obj = Persistence.Instance.Provider.GetPersonByEmail(dto.Email);
            if (obj != null)
                return ErrorCodeMap.CreateResponse(Request, 10101, dto.Email + " already exists");
            obj = ApplicationFacade.CreatePerson(dto);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10001, "Unable to create person");
            return Request.CreateResponse(HttpStatusCode.Created, Mapper.Map<PersonDTO>(obj));
        }

        [HttpPut]
        [Route("{id}")]
        public HttpResponseMessage Put(string id, PersonDTO dto)
        {
            if (string.IsNullOrEmpty(id))
                return ErrorCodeMap.CreateResponse(Request, 10004, "id is null or empty");
            if (dto == null)
                return ErrorCodeMap.CreateResponse(Request, 10003, "Person is null");
            var obj = Persistence.Instance.Provider.GetObjectById<Person>(id);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10100,
                                                   "Can not find " + dto.FirstName + " " + dto.LastName + " with id " +
                                                   dto.Id);
            ApplicationFacade.UpdatePerson(obj, dto);
            return Request.CreateResponse(HttpStatusCode.OK, Mapper.Map<PersonDTO>(obj));
        }

        [Route("{id}")]
        public HttpResponseMessage Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return ErrorCodeMap.CreateResponse(Request, 10004, "id is null or empty");
            var obj = Persistence.Instance.Provider.GetObjectById<Person>(id);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10101, "Can not find person with id " + id);
            ApplicationFacade.DeleteAccount(obj);
            return Request.CreateResponse(HttpStatusCode.OK, id);
        }
    }
}