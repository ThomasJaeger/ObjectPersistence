using System.Collections.ObjectModel;

namespace Services
{
    public class ErrorInfo
    {
        private Collection<ErrorData> _errorDatas = new Collection<ErrorData>();
        private ErrorSeverityType _errorSeverityType = ErrorSeverityType.Error;
        private ErrorCodeType _errorCodeType = ErrorCodeType.Unclassified;
        private int _code = 0;
        private string _message = "";
        private string _description = "";

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public int Code
        {
            get { return _code; }
            set { _code = value; }
        }

        public ErrorSeverityType ErrorSeverityType
        {
            get { return _errorSeverityType; }
            set { _errorSeverityType = value; }
        }

        public ErrorCodeType ErrorCodeType
        {
            get { return _errorCodeType; }
            set { _errorCodeType = value; }
        }

        public Collection<ErrorData> ErrorDatas
        {
            get { return _errorDatas; }
            set { _errorDatas = value; }
        }
    }
}