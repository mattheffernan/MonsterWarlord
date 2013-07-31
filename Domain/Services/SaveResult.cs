using System;

namespace Domain.Services
{
    public class SaveResult
    {
        public bool Successful { get; set; }
        public Exception Error { get; set; }
    }
}