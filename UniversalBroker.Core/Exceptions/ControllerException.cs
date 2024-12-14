using System.Net;

namespace UniversalBroker.Core.Exceptions
{
    public class ControllerException: Exception
    {
        private int _statusCode = 400;
        public HttpStatusCode StatusCode
        {
            get => (HttpStatusCode)_statusCode;
            set => _statusCode = (int)value;
        }
        public int StatusCodeInt
        {
            get => _statusCode;
            set => _statusCode = value;
        }

        public ControllerException(int? statusCode = null)
        {
            _statusCode = statusCode ?? _statusCode;
        }

        public ControllerException(string message, int? statusCode = null)
            : base(message)
        {
            _statusCode = statusCode ?? _statusCode;
        }

        public ControllerException(string message, Exception inner, int? statusCode = null)
            : base(message, inner)
        {
            _statusCode = statusCode ?? _statusCode;
        }
    }
}
