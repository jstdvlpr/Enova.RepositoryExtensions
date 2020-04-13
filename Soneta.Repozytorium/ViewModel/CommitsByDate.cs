using Soneta.Types;

namespace Soneta.Repozytorium
{
    public sealed class CommitsByDate
    {
        public Date Date { get; set; }
        public CommitsByUser[] CommitsByUser { get; set; }
    }
}
