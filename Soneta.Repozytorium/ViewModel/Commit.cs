using System;

namespace Soneta.Repozytorium
{
    public sealed class Commit
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public string Branch { get; set; }
    }
}
