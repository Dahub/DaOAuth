using System;

namespace DaOAuth.Service
{
    public class DaOauthServiceException : Exception
    {
        public DaOauthServiceException() : base() { }
        public DaOauthServiceException(string msg) : base(msg) { }
        public DaOauthServiceException(string msg, Exception ex) : base(msg, ex) { }
    }
}
