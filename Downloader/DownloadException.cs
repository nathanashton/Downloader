using System.Runtime.Serialization;
using System;

namespace Download
{
    /// <summary>
    /// Description of DownloadException.
    /// </summary>
    [Serializable]
    public class DownloadException : Exception
    {
        public DownloadException()
            : base() { }
        
        public DownloadException(string message)
            : base(message) { }
        
        public DownloadException(string format, params object[] args)
            : base(string.Format(format, args)) { }
        
        public DownloadException(string message, Exception innerException)
            : base(message, innerException) { }
        
        public DownloadException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
        
        protected DownloadException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
