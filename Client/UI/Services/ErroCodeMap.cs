using System.Collections.Generic;

namespace Services
{
    public static class ErrorCodeMap
    {
        private static readonly Dictionary<int, ErrorCodeType> _map = new Dictionary<int, ErrorCodeType>();

        public static Dictionary<int, ErrorCodeType> Map
        {
            get { return _map; }
        }

        static ErrorCodeMap()
        {
            MapCodes();
        }

        private static void MapCodes()
        {
            _map.Add(0, ErrorCodeType.ServiceInternalError);      // Internal error
            _map.Add(10001, ErrorCodeType.ServiceInternalError);  // Internal error
            _map.Add(10002, ErrorCodeType.InvalidApiCredentials); // You do not have permissions to make this API call
            _map.Add(10003, ErrorCodeType.InvalidRequest);        // Missing argument
            _map.Add(10004, ErrorCodeType.InvalidRequest);        // Invalid argument
            _map.Add(10007, ErrorCodeType.InvalidApiCredentials); // No permissions to make this API call
            _map.Add(10100, ErrorCodeType.ResourceNotFound);      // Object does not exist
            _map.Add(10101, ErrorCodeType.ResourceAlreadyExists); // Object exists already during a creation process
            // etc.
        }
    }
}
