using System;

namespace Soneta.Repozytorium
{
    public sealed class CommitInfo
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string BranchName { get; set; }
    }
}
