using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DTO;
using Newtonsoft.Json;

namespace Services
{
    public static class RESTServices
    {
        private static string _serviceBase = @"http://localhost:51824/";
        private static string _personUrl = @"api/v1/persons/";
        private static string _addressTypeUrl = @"api/v1/addresstypes/";
        private static string _lastJsonResult;
        private static ResultType _resultType;
        private static List<string> _successfullResponseCodes = new List<string>();
        private static ErrorInfo _errorInfo;

        private const int TIMEOUT = 120000;   // in milliseconds, 30,000 = 30 seconds

        static RESTServices()
        {
            SetupHttpSuccessCodes();
        }

        /// <summary>
        /// Any errors returned from the service or caused by the service
        /// will be contained in the ErrorInfo object for further processing
        /// by the client.
        /// </summary>
        public static ErrorInfo ErrorInfo
        {
            get { return _errorInfo; }
            set { _errorInfo = value; }
        }

        /// <summary>
        /// The result of the service call: 
        /// Success, SuccessWithWarning, Failure, Unsupported
        /// </summary>
        public static ResultType ResultType
        {
            get { return _resultType; }
            set { _resultType = value; }
        }

        /// <summary>
        /// A list of acceptable hhtp success codes.
        /// We'll use this to dertemine if we had a 
        /// succesfull service call or not.
        /// </summary>
        private static void SetupHttpSuccessCodes()
        {
            _successfullResponseCodes.Add(HttpStatusCode.OK.ToString("F"));
            _successfullResponseCodes.Add(HttpStatusCode.Created.ToString("F"));
        }

        public static List<PersonDTO> GetAllPeople()
        {
            List<PersonDTO> list = new List<PersonDTO>();
            try
            {
                list = GETList<PersonDTO>(_serviceBase + _personUrl);
            }
            catch (Exception ex)
            {
                HandleErrors(ex);
            }
            return list;
        }

        public static PersonDTO CreatePerson(PersonDTO dto)
        {
            try
            {
                return POST<PersonDTO>(_serviceBase + _personUrl, dto);
            }
            catch (Exception ex)
            {
                HandleErrors(ex);
            }
            return null;
        }

        private static T GET<T>(string url) where T : DTOBase, new()
        {
            _errorInfo = null;  // reset ErrorInfo object

            using (var restClient = new HttpClient())
            {
                restClient.BaseAddress = new Uri(url);
                restClient.Timeout = TimeSpan.FromMilliseconds(TIMEOUT);
                restClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                using (var request = new HttpRequestMessage(HttpMethod.Get, restClient.BaseAddress.AbsoluteUri))
                {
                    var response = restClient.SendAsync(request).Result; // blocking call
                    _lastJsonResult = response.Content.ReadAsStringAsync().Result;
                    HandleResponse(response.StatusCode.ToString("F"));
                    T obj = JsonConvert.DeserializeObject<T>(_lastJsonResult);
                    return obj;
                }
            }
        }

        private static List<T> GETList<T>(string url) where T : DTOBase, new()
        {
            _errorInfo = null;  // reset ErrorInfo object

            using (var restClient = new HttpClient())
            {
                restClient.BaseAddress = new Uri(url);
                restClient.Timeout = TimeSpan.FromMilliseconds(TIMEOUT);
                restClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var request = new HttpRequestMessage(HttpMethod.Get, restClient.BaseAddress.AbsoluteUri))
                {
                    var response = restClient.SendAsync(request).Result; // blocking call
                    _lastJsonResult = response.Content.ReadAsStringAsync().Result;
                    HandleResponse(response.StatusCode.ToString("F"));
                    List<T> obj = JsonConvert.DeserializeObject<List<T>>(_lastJsonResult);
                    return obj;
                }
            }
        }

        private static T POST<T>(string url, object requestObject) where T : DTOBase, new()
        {
            _errorInfo = null;  // reset ErrorInfo object

            using (var restClient = new HttpClient())
            {
                restClient.BaseAddress = new Uri(url);
                restClient.Timeout = TimeSpan.FromMilliseconds(TIMEOUT);
                restClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Use Formatting.Indented for pretty JSON format
                string json = JsonConvert.SerializeObject(requestObject, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,  // is useful if objects are nested but not indefinitely
                    //ReferenceLoopHandling = ReferenceLoopHandling.Ignore     // will not serialize an object if it is a child object of itself
                    TypeNameHandling = TypeNameHandling.All
                });

                using (HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    var response = restClient.PostAsync(restClient.BaseAddress.AbsoluteUri, httpContent).Result;
                    _lastJsonResult = response.Content.ReadAsStringAsync().Result;
                    HandleResponse(response.StatusCode.ToString("F"));
                    T obj = JsonConvert.DeserializeObject<T>(_lastJsonResult);
                    return obj;
                }
            }
        }

        private static T PUT<T>(string url, object requestObject) where T : DTOBase, new()
        {
            _errorInfo = null;  // reset ErrorInfo object

            using (var restClient = new HttpClient())
            {
                restClient.BaseAddress = new Uri(url);
                restClient.Timeout = TimeSpan.FromMilliseconds(TIMEOUT);
                restClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = JsonConvert.SerializeObject(requestObject);

                using (HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    var response = restClient.PutAsync(restClient.BaseAddress.AbsoluteUri, httpContent).Result;
                    _lastJsonResult = response.Content.ReadAsStringAsync().Result;
                    HandleResponse(response.StatusCode.ToString("F"));
                    T obj = JsonConvert.DeserializeObject<T>(_lastJsonResult);
                    return obj;
                }
            }
        }

        private static void DELETE(string url)
        {
            _errorInfo = null;  // reset ErrorInfo object

            using (var restClient = new HttpClient())
            {
                restClient.BaseAddress = new Uri(url);
                restClient.Timeout = TimeSpan.FromMilliseconds(TIMEOUT);
                restClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = restClient.DeleteAsync(restClient.BaseAddress.AbsoluteUri).Result;
                _lastJsonResult = response.Content.ReadAsStringAsync().Result;
                HandleResponse(response.StatusCode.ToString("F"));
            }
        }

        public static void HandleResponse(string statusCode)
        {
            _resultType = ResultType.Success;

            if (string.IsNullOrEmpty(statusCode))
            {
                _errorInfo = new ErrorInfo();
                _errorInfo.ErrorSeverityType = ErrorSeverityType.Error;
                ErrorData errorData = new ErrorData();
                errorData.Id = "";
                errorData.Message = "statusCode is null when trying to process service status code.";
                _errorInfo.Message = errorData.Message;
                _errorInfo.ErrorDatas.Add(errorData);
                _resultType = ResultType.Failure;
                return;
            }

            if (!_successfullResponseCodes.Contains(statusCode))
            {
                _errorInfo = new ErrorInfo();
                _resultType = ResultType.Failure;
                ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(_lastJsonResult);

                if (errorResponse == null)
                {
                    // Unable to parse _lastJsonResult (empty?)
                    _errorInfo.Code = errorResponse.Code;
                    _errorInfo.Description = errorResponse.Description;
                    _errorInfo.ErrorCodeType = ErrorCodeType.Unclassified;
                    _errorInfo.ErrorSeverityType = ErrorSeverityType.Error;
                    ErrorData errorData = new ErrorData();
                    errorData.Id = "";
                    errorData.Message = "Unable to parse JSON.";
                    _errorInfo.Message = errorData.Message;
                    _errorInfo.ErrorDatas.Add(errorData);
                    return;
                }

                if (ErrorCodeMap.Map.ContainsKey(errorResponse.Code))
                {
                    _errorInfo.Code = errorResponse.Code;
                    if (!string.IsNullOrEmpty(errorResponse.Message))
                        _errorInfo.Message = errorResponse.Message;
                    _errorInfo.Description = errorResponse.Description;
                    _errorInfo.ErrorCodeType = ErrorCodeMap.Map[errorResponse.Code];
                    _errorInfo.ErrorSeverityType = ErrorSeverityType.Error;

                    foreach (var validation in errorResponse.Validations)
                    {
                        ErrorData errorData = new ErrorData();
                        errorData.Id = errorResponse.Code.ToString();
                        errorData.Message = validation;
                        _errorInfo.ErrorDatas.Add(errorData);
                    }
                }
            }
        }

        private static ResultType HandleErrors(Exception ex)
        {
            _resultType = ResultType.Failure;

            if (ex != null)
            {
                _errorInfo = new ErrorInfo();
                _errorInfo.Code = 0;
                _errorInfo.ErrorSeverityType = ErrorSeverityType.Error;
                _errorInfo.Message = ex.Message;

                ErrorData errorData = new ErrorData();
                errorData.Id = "";
                errorData.Message = ex.Message;
                _errorInfo.ErrorDatas.Add(errorData);

                errorData = new ErrorData();
                errorData.Id = "";
                errorData.Message = ex.ToString();
                _errorInfo.ErrorDatas.Add(errorData);
            }

            return _resultType;
        }

        public static void SavePerson(PersonDTO dto)
        {
            try
            {
                PUT<PersonDTO>(_serviceBase + _personUrl + dto.Id, dto);
            }
            catch (Exception ex)
            {
                HandleErrors(ex);
            }
        }

        public static void DeletePerson(string id)
        {
            try
            {
                DELETE(_serviceBase + _personUrl + id);
            }
            catch (Exception ex)
            {
                HandleErrors(ex);
            }
        }

        public static List<AddressTypeDTO> GetAllAddressTypes()
        {
            List<AddressTypeDTO> list = new List<AddressTypeDTO>();
            try
            {
                list = GETList<AddressTypeDTO>(_serviceBase + _addressTypeUrl);
            }
            catch (Exception ex)
            {
                HandleErrors(ex);
            }
            return list;
        }
    }
}
