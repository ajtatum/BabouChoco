using System.Collections.Generic;
using System.Threading.Tasks;
using BabouChoco.Models;
using Octokit;

namespace BabouChoco.Services
{
    public interface IGitHubService
    {
        GitHubClient GetClient();
        Task<List<ChocoPackage>> GetGist();
        Task UpdateGist(string packages);
        void CreateSettings(string gitHubKey, string gitHubGistId, string gitHubGistDesc);
        void GetSettings();
    }
}