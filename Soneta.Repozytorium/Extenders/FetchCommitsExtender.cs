using Soneta.Business;
using Soneta.Repozytorium;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: Worker(typeof(FetchCommitsExtender))]

namespace Soneta.Repozytorium
{
    [AsyncWorker(IsConcurrent = true)]
    public sealed class FetchCommitsExtender : IAsyncWorker, IAsyncIsReady
    {
        private Repozytorium _repozytorium;
        private CommitsByDate[] _value;

        private readonly Dictionary<RepositoryType, IRepositoryService> services;
        public IRepositoryService this[RepositoryType type]
        {
            get { return services[type]; }
            set { services[type] = value; }
        }

        public FetchCommitsExtender()
        {
            services = new Dictionary<RepositoryType, IRepositoryService>()
            {
                { RepositoryType.Git, new GitHubService() },
                { RepositoryType.SVN, new SvnService() }
            };
        }

        [Context]
        public Repozytorium Repozytorium
        {
            get { return _repozytorium; }
            set
            {
                if (_repozytorium != value) _value = null;
                _repozytorium = value;
            }
        }

        public CommitsByDate[] Value
        {
            get
            {
                if (_value == null) throw new InProgressException();
                return _value;
            }
        }

        public bool IsActionReady(IAsyncContext acx) => IsLoading();

        public void Action(IAsyncContext acx) => _value = FetchCommits(acx);

        public bool IsLoaded() => _value != null;

        public bool IsLoading() => !IsLoaded() && !IsError();

        private bool error;
        public bool IsError() => error;

        private CommitsByDate[] FetchCommits(IAsyncContext acx)
        {
            try
            {
                if (!services.ContainsKey(Repozytorium.Typ))
                    throw new NotSupportedException("Typ repozytorium nieobsługiwany");
                var commitsInfo = services[Repozytorium.Typ].GetAllCommits(Repozytorium.Adres).GetAwaiter().GetResult();
                // Ilość dni do średniej - kwestia sporna, przyjąłem tylko te dni w których były wrzucane commity
                int days = commitsInfo.GroupBy(c => c.Time.Date).Count();
                var users = commitsInfo.GroupBy(c => c.UserEmail).Select(byEmail => new RepositoryUser
                {
                    Email = byEmail.Key,
                    Name = byEmail.First().UserName,
                    AverageCommitsCount = (float)byEmail.Count() / days
                });
                var commits = commitsInfo.GroupBy(c => c.Time.Date).Select(byDate => new CommitsByDate
                {
                    Date = byDate.Key,
                    CommitsByUser = byDate.GroupBy(c => c.UserEmail).Select(byUser => new CommitsByUser
                    {
                        User = users.First(u => u.Email == byUser.Key),
                        Commits = byUser.Select(c => new Commit
                        {
                            Time = c.Time,
                            Message = c.Message,
                            Branch = c.BranchName
                        }).ToArray()
                    }).ToArray()
                }).ToArray();
                return commits;
            }
            catch (Exception)
            {
                // Tu należy dołożyć logowanie
                error = true;
                return null;
            }
        }
    }
}