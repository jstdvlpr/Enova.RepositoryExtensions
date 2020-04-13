using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soneta.Repozytorium
{
    public class GitHubService : IRepositoryService
    {
        // Konto GitHub utworzone na potrzeby autentykacji do zwkiększenia liczby requestów
        private const string login = "jstdvlpr";
        private const string password = "justd3v3l0p3r";

        public IGitHubClient Client { get; set; }

        public GitHubService()
        {
            Client = new GitHubClient(new ProductHeaderValue(login))
            {
                Credentials = new Credentials(login, password, AuthenticationType.Basic)
            };
        }

        public async Task<IEnumerable<CommitInfo>> GetAllCommits(string repositoryUrl)
        {
            repositoryUrl = repositoryUrl.ToLower();
            if (!(repositoryUrl.StartsWith("github.com")
                || repositoryUrl.StartsWith("http://github.com")
                || repositoryUrl.StartsWith("https://github.com")))
                throw new ArgumentException($"Niepoprawny parametr {nameof(repositoryUrl)}");
            string[] splittedAddress = repositoryUrl.Substring(repositoryUrl.IndexOf(@"github.com") + 10)
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedAddress.Length < 2)
                throw new ArgumentException($"Niepoprawny parametr {nameof(repositoryUrl)}");

            string owner = splittedAddress[0], name = splittedAddress[1];
            var commitsInfo = new List<CommitInfo>();

            var branches = await Client.Repository.Branch.GetAll(owner, name);
            foreach (var branch in branches)
            {
                var request = new CommitRequest { Sha = branch.Name };
                var commits = await Client.Repository.Commit.GetAll(owner, name, request);
                commitsInfo.AddRange(commits.Select(c => new CommitInfo
                {
                    Time = c.Commit.Author.Date.DateTime,
                    Message = c.Commit.Message,
                    UserEmail = c.Commit.Author.Email,
                    UserName = c.Commit.Author.Name,
                    BranchName = branch.Name
                }));
            }
            return commitsInfo;
        }
    }
}
