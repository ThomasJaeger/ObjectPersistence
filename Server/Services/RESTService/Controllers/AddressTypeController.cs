using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using DomainModel;
using PersistenceService;
using RESTService.ActionFilters;
using RESTService.Models;

namespace RESTService.Controllers
{
    [RoutePrefix("api/v1/addresstypes")]
    [Route("{action=Get}")]
    public class AddressTypeController : ApiController
    {
        [Route("")]
        public HttpResponseMessage Get()
        {
            List<AddressType> all = ApplicationFacade.GetAllAddressTypes();
            List<AddressTypeDTO> list = Mapper.Map<List<AddressType>, List<AddressTypeDTO>>(all);
            return Request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return ErrorCodeMap.CreateResponse(Request, 10004, "id is null or empty");
            var obj = Persistence.Instance.Provider.GetObjectById<AddressType>(id);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10100, "AddressType does not exist");
            var dto = Mapper.Map<AddressTypeDTO>(obj);
            return Request.CreateResponse(HttpStatusCode.OK, dto);
        }

        [HttpPost]
        [Route("")]
        [ValidationResponseFilter]
        public HttpResponseMessage Post(AddressTypeDTO dto)
        {
            AddressType obj = Persistence.Instance.Provider.GetObjectByName<AddressType>(dto.Name);
            if (obj != null)
                return ErrorCodeMap.CreateResponse(Request, 10101, dto.Name + " already exists");
            obj = ApplicationFacade.CreateAddressType(dto);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10001, "Unable to create address type");
            return Request.CreateResponse(HttpStatusCode.Created, Mapper.Map<AddressTypeDTO>(obj));
        }

        [HttpPut]
        [Route("{id}")]
        [ValidationResponseFilter]
        public HttpResponseMessage Put(string id, AddressTypeDTO dto)
        {
            if (string.IsNullOrEmpty(id))
                return ErrorCodeMap.CreateResponse(Request, 10004, "id is null or empty");
            var obj = Persistence.Instance.Provider.GetObjectById<AddressType>(id);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10100,
                                                   "Can not find " + dto.Name + " with id " + dto.Id);
            ApplicationFacade.UpdateAddressType(obj, dto);
            return Request.CreateResponse(HttpStatusCode.OK, Mapper.Map<AddressTypeDTO>(obj));
        }

        [HttpDelete]
        [Route("{id}")]
        public HttpResponseMessage Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return ErrorCodeMap.CreateResponse(Request, 10004, "id is null or empty");
            var obj = Persistence.Instance.Provider.GetObjectById<AddressType>(id);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10100, "Can not find address type with id " + id);
            ApplicationFacade.DeleteAddressType(obj);
            return Request.CreateResponse(HttpStatusCode.OK, id);
        }
    }
}
