using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soneta.Repozytorium
{
    public interface IRepositoryService
    {
        Task<IEnumerable<CommitInfo>> GetAllCommits(string repositoryUrl);
    }
}
