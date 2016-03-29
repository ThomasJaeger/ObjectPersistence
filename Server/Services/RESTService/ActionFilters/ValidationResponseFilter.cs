using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace RESTService.ActionFilters
{
    public class ValidationResponseFilter : ActionFilterAttribute
    {
        /// <summary>
        ///     Called when [action executing].
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
//                actionContext.Response = actionContext
//                    .Request
//                    .CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState);

                // Use our ErrorMap to handle validation error responses
                var list = new List<string>();
                foreach (var state in actionContext.ModelState)
                {
                    list.Add(state.Key + ": " + state.Value.Errors[0].ErrorMessage);
                }
                actionContext.Response = ErrorCodeMap.CreateResponse(actionContext.Request, 10004, "", list);
            }
        }
    }
}