using Moq;
using NUnit.Framework;
using Soneta.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soneta.Repozytorium.Tests
{
    [TestFixture]
    public class FetchCommitsExtenderTest
    {
        private FetchCommitsExtender extender;
        private IAsyncContext context;

        [SetUp]
        public void SetUp()
        {
            extender = new FetchCommitsExtender();
            extender[RepositoryType.Git] = new Mock<IRepositoryService>()
            {
                DefaultValue = DefaultValue.Mock
            }.Object;
            context = new Mock<IAsyncContext>().Object;

        }

        [Test]
        public void Action_TypeNotSupported()
        {
            extender.Repozytorium = new Repozytorium()
            {
                // Typ = 0,
                Adres = "https://github.com/test/Testowy"
            };

            extender.Action(context);

            Assert.IsTrue(extender.IsError());
        }

        [Test]
        public void Action_ActionOk()
        {
            extender.Repozytorium = new Repozytorium()
            {
                Guid = Guid.NewGuid(),
                Typ = RepositoryType.Git,
                Adres = "https://github.com/test/Testowy",
                Opis = "Testowy opis"
            };

            extender.Action(context);

            Assert.IsFalse(extender.IsError());
            Assert.DoesNotThrow(() => { var value = extender.Value; });
        }

        [TestCase(2, "01/01/2020 08:14:32", "01/01/2020 09:32:42", "02/01/2020 14:23:21")]
        [TestCase(3, "01/01/2020 08:14:32", "02/01/2020 11:51:07", "03/01/2020 20:01:09")]
        public void Action_CheckDates(int datesCount, params string[] dates)
        {
            IEnumerable<CommitInfo> commits = dates.Select(d => new CommitInfo()
            {
                Time = DateTime.Parse(d)
            });
            var mockService = new Mock<IRepositoryService>();
            mockService.Setup(x => x.GetAllCommits(It.IsAny<string>()))
                .Returns(Task.FromResult(commits));
            extender[RepositoryType.Git] = mockService.Object;
            extender.Repozytorium = new Repozytorium()
            {
                Guid = Guid.NewGuid(),
                Typ = RepositoryType.Git,
                Adres = "https://github.com/test/Testowy",
                Opis = "Testowy opis"
            };

            extender.Action(context);

            Assert.AreEqual(datesCount, extender.Value.Length);
        }

        public static IEnumerable<TestCaseData> CheckAverageCommitsCountTestCases
        {
            get
            {
                yield return new TestCaseData("user1@abc.pl", 2.0 / 3.0, new CommitInfo[]
                {
                    new CommitInfo() { UserEmail = "user1@abc.pl", Time = DateTime.Parse("01/01/2020 08:14:32") },
                    new CommitInfo() { UserEmail = "2person@cde.pl", Time = DateTime.Parse("02/01/2020 11:51:07") },
                    new CommitInfo() { UserEmail = "user1@abc.pl", Time = DateTime.Parse("03/01/2020 20:01:09") }
                });
                yield return new TestCaseData("2person@cde.pl", 1.0 / 3.0, new CommitInfo[]
                {
                    new CommitInfo() { UserEmail = "user1@abc.pl", Time = DateTime.Parse("01/01/2020 08:14:32") },
                    new CommitInfo() { UserEmail = "2person@cde.pl", Time = DateTime.Parse("02/01/2020 11:51:07") },
                    new CommitInfo() { UserEmail = "user1@abc.pl", Time = DateTime.Parse("04/01/2020 21:59:02") }
                });
                yield return new TestCaseData("user1@abc.pl", 2.0, new CommitInfo[]
                {
                    new CommitInfo() { UserEmail = "user1@abc.pl", Time = DateTime.Parse("01/01/2020 16:15:27") },
                    new CommitInfo() { UserEmail = "user1@abc.pl", Time = DateTime.Parse("01/01/2020 09:41:29") },
                    new CommitInfo() { UserEmail = "user1@abc.pl", Time = DateTime.Parse("03/01/2020 05:44:31") },
                    new CommitInfo() { UserEmail = "user1@abc.pl", Time = DateTime.Parse("03/01/2020 23:59:59") },
                    new CommitInfo() { UserEmail = "2person@cde.pl", Time = DateTime.Parse("03/01/2020 11:51:07") },
                    new CommitInfo() { UserEmail = "user1@abc.pl", Time = DateTime.Parse("06/01/2020 19:14:32") },
                    new CommitInfo() { UserEmail = "user1@abc.pl", Time = DateTime.Parse("06/01/2020 21:59:02") }
                });
            }
        }

        [TestCaseSource(nameof(CheckAverageCommitsCountTestCases))]
        public void Action_CheckAverageCommitsCount(string user, double average, CommitInfo[] commits)
        {
            var mockService = new Mock<IRepositoryService>();
            mockService.Setup(x => x.GetAllCommits(It.IsAny<string>()))
                .Returns(Task.FromResult(commits.AsEnumerable()));
            extender[RepositoryType.Git] = mockService.Object;
            extender.Repozytorium = new Repozytorium()
            {
                Guid = Guid.NewGuid(),
                Typ = RepositoryType.Git,
                Adres = "https://github.com/test/Testowy",
                Opis = "Testowy opis"
            };

            extender.Action(context);

            var expected = extender.Value.SelectMany(x => x.CommitsByUser)
                .First(byUser => byUser.User.Email == user)
                .User.AverageCommitsCount;
            Assert.That(average, Is.EqualTo(expected).Within(0.000001));
        }
    }
}
