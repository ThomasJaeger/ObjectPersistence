using System.Web.Http;
using AutoMapper;
using DomainModel;
using RESTService.Models;

namespace RESTService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            Mapper.CreateMap<Person, PersonDTO>();

            // We don't want the Id to be overwritten by an incoming DTO
            // such as an update to a person because the Id is handled by
            // the domain. For example, the Id is generated in the base class
            // DomainObject and we want this business rule to stay in the
            // domain model. We need to tell AutoMapper to ignore the Id
            // property when mapping the properties for us.
            Mapper.CreateMap<PersonDTO, Person>().
                   ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
