using Moq;
using NUnit.Framework;
using Octokit;
using System;

namespace Soneta.Repozytorium.Tests
{
    [TestFixture]
    public class GitHubServiceTest
    {
        private GitHubService service;

        [OneTimeSetUp]
        public void SetUp()
        {
            service = new GitHubService()
            {
                Client = new Mock<IGitHubClient>()
                {
                    DefaultValue = DefaultValue.Mock
                }.Object
            };
        }

        [Test]
        public void GetAllCommits_ThrowsNullReferenceException()
        {
            Assert.ThrowsAsync<NullReferenceException>(() => service.GetAllCommits(null));
        }

        [TestCase("git.com")]
        [TestCase("github.com/test/")]
        [TestCase("http://git/test/Testowy/github.com")]
        public void GetAllCommits_ThrowsArgumentException(string repositoryUrl)
        {
            Assert.ThrowsAsync<ArgumentException>(() => service.GetAllCommits(repositoryUrl));
        }

        [TestCase("github.com/test/Testowy")]
        [TestCase("http://github.com/test/Testowy/branch")]
        [TestCase("https://github.com/test/Testowy/branch/plik")]
        public void GetAllCommits_NotThrowExceptions(string repositoryUrl)
        {
            Assert.DoesNotThrowAsync(() => service.GetAllCommits(repositoryUrl));
        }
    }
}
