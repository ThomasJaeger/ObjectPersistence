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
    [RoutePrefix("api/v1/accounts")]
    [Route("{action=Get}")]
    public class AccountController : ApiController
    {
        [Route("")]
        public HttpResponseMessage Get()
        {
            List<Account> allObjects = ApplicationFacade.GetAllAccounts();
            List<AccountDTO> list = Mapper.Map<List<Account>, List<AccountDTO>>(allObjects);
            return Request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return ErrorCodeMap.CreateResponse(Request, 10003, "id is null");
            var obj = Persistence.Instance.Provider.GetObjectById<Account>(id);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10100, "account does not exist");
            var dto = Mapper.Map<AccountDTO>(obj);
            return Request.CreateResponse(HttpStatusCode.OK, dto);
        }

        [HttpGet]
        [Route("account")] // api/v1/accounts/account?email=johndoe@gmail.com
        public HttpResponseMessage GetByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return ErrorCodeMap.CreateResponse(Request, 10004, "email is null or empty");
            Account account = Persistence.Instance.Provider.GetAccountByAccountOwnerEmail(email);
            if (account == null)
                return ErrorCodeMap.CreateResponse(Request, 10100, "account does not exist");
            var dto = Mapper.Map<AccountDTO>(account);
            return Request.CreateResponse(HttpStatusCode.OK, dto);
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Post(AccountDTO dto)
        {
            HttpResponseMessage failedValidation = ValidateDto(dto);
            if (failedValidation != null) return failedValidation;
            Account obj = Persistence.Instance.Provider.GetAccountByAccountNumber(dto.AccountNumber);
            if (obj != null)
                return ErrorCodeMap.CreateResponse(Request, 10101, dto.AccountNumber + " already exists");
            obj = ApplicationFacade.CreateAccount(dto);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10001, "Unable to create account");
            return Request.CreateResponse(HttpStatusCode.Created, Mapper.Map<AccountDTO>(obj));
        }

        [HttpPut]
        [Route("{id}")]
        public HttpResponseMessage Put(string id, AccountDTO dto)
        {
            if (string.IsNullOrEmpty(id))
                return ErrorCodeMap.CreateResponse(Request, 10004, "id is null or empty");
            HttpResponseMessage failedValidation = ValidateDto(dto);
            if (failedValidation != null) return failedValidation;
            var obj = Persistence.Instance.Provider.GetObjectById<Account>(id);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10100,
                                                   "Can not find account # " + dto.AccountNumber + " with id " + dto.Id);
            ApplicationFacade.UpdateAccount(obj, dto);
            return Request.CreateResponse(HttpStatusCode.OK, Mapper.Map<AccountDTO>(obj));
        }

        [HttpDelete]
        [Route("{id}")]
        public HttpResponseMessage Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return ErrorCodeMap.CreateResponse(Request, 10004, "id is null or empty");
            var obj = Persistence.Instance.Provider.GetObjectById<Account>(id);
            if (obj == null)
                return ErrorCodeMap.CreateResponse(Request, 10100, "Can not find account with id " + id);
            ApplicationFacade.DeleteAccount(obj);
            return Request.CreateResponse(HttpStatusCode.OK, id);
        }

        private HttpResponseMessage ValidateDto(AccountDTO dto)
        {
            if (dto == null)
                return ErrorCodeMap.CreateResponse(Request, 10003, "Account is null");
            if (string.IsNullOrEmpty(dto.AccountNumber))
                return ErrorCodeMap.CreateResponse(Request, 10004, "AccountNumber is null or empty");
            if (dto.AccountOwner == null)
                return ErrorCodeMap.CreateResponse(Request, 10004, "AccountOwner is required");
            if (string.IsNullOrEmpty(dto.AccountOwner.Email))
                return ErrorCodeMap.CreateResponse(Request, 10004, "AccountOwner.Email is null or empty");
            if (string.IsNullOrEmpty(dto.AccountOwner.Password))
                return ErrorCodeMap.CreateResponse(Request, 10004, "AccountOwner.Password is null or empty");
            return null;
        }
    }
}
