namespace Soneta.Repozytorium
{
    public sealed class CommitsByUser
    {
        public RepositoryUser User { get; set; }
        public Commit[] Commits { get; set; }
    }
}
