using System;

namespace DaOAuthCore.Service
{
    public class CodeDto
    {
        public string UserName { get; set; }
        public string Scope { get; set; }
        public bool IsValid { get; set; }
        public Guid UserPublicId { get; set; }
    }
}
