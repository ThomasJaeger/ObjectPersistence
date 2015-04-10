using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace RESTService
{
    public static class ErrorCodeMap
    {
        private static readonly Dictionary<int, ErrorCodeType> _map = new Dictionary<int, ErrorCodeType>();

        static ErrorCodeMap()
        {
            MapCodes();
        }

        public static Dictionary<int, ErrorCodeType> Map
        {
            get { return _map; }
        }

        private static void MapCodes()
        {
            _map.Add(10001, ErrorCodeType.ServiceInternalError); // Internal error
            _map.Add(10002, ErrorCodeType.InvalidApiCredentials); // You do not have permissions to make this API call
            _map.Add(10003, ErrorCodeType.InvalidRequest); // Missing argument
            _map.Add(10004, ErrorCodeType.InvalidRequest); // Invalid argument
            _map.Add(10007, ErrorCodeType.InvalidApiCredentials); // No permissions to make this API call
            _map.Add(10100, ErrorCodeType.ResourceNotFound); // Object does not exist
            _map.Add(10101, ErrorCodeType.ResourceAlreadyExists); // Object exists already during a creation process
            // etc.
        }

        public static HttpResponseMessage CreateResponse(HttpRequestMessage request, int code, string message = "",
            List<string> validationErrors = null)
        {
            var errorResponse = new ErrorResponse();

            errorResponse.Code = code;
            errorResponse.Description = message;

            switch (code)
            {
                case 10002:
                {
                    errorResponse.Type = ErrorCodeType.InvalidApiCredentials.ToString();
                    errorResponse.Message = "You do not have permissions to make this API call";
                    return request.CreateResponse(HttpStatusCode.Unauthorized, errorResponse);
                }
                case 10003:
                {
                    errorResponse.Type = ErrorCodeType.InvalidRequest.ToString();
                    errorResponse.Message = "Missing argument";
                    if (validationErrors != null)
                    {
                        errorResponse.Validations = validationErrors;
                        errorResponse.Description = "Validation Errors";
                    }
                    return request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }
                case 10004:
                {
                    errorResponse.Type = ErrorCodeType.InvalidRequest.ToString();
                    errorResponse.Message = "Invalid argument";
                    if (validationErrors != null)
                    {
                        errorResponse.Validations = validationErrors;
                        errorResponse.Description = "Validation Errors";
                    }
                    return request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }
                case 10007:
                {
                    errorResponse.Type = ErrorCodeType.InvalidApiCredentials.ToString();
                    errorResponse.Message = "No permissions to make this API call";
                    return request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }
                case 10100:
                {
                    errorResponse.Type = ErrorCodeType.ResourceNotFound.ToString();
                    errorResponse.Message = "Resource not found";
                    return request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
                }
                case 10101:
                {
                    errorResponse.Type = ErrorCodeType.ResourceAlreadyExists.ToString();
                    errorResponse.Message = "Object exists already during a creation process";
                    return request.CreateResponse(HttpStatusCode.Conflict, errorResponse);
                }
            }

            errorResponse.Code = 10001;
            errorResponse.Type = ErrorCodeType.ServiceInternalError.ToString();
            errorResponse.Message = "Internal server error";
            errorResponse.Description = "Unable to map error internally";

            return request.CreateResponse(HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}