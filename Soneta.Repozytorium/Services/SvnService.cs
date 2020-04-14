using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soneta.Repozytorium
{
    public class SvnService : IRepositoryService
    {
        public Task<IEnumerable<CommitInfo>> GetAllCommits(string repositoryUrl)
        {
            throw new NotImplementedException();
        }
    }
}
