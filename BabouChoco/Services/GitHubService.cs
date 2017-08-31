using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BabouChoco.Models;
using BabouExtensions;
using Newtonsoft.Json;
using Octokit;

namespace BabouChoco.Services
{
    public class GitHubService : IGitHubService
    {
        private static string _gitHubKey;
        private static string _gitHubGistId;
        private static string _gitHubGistDesc;

        private static string ProjectDirectory => Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.FullName;
        private static string GitHubSettingsFile => $@"{ProjectDirectory}\Settings\GitHubSettings.json";

        public GitHubClient GetClient() => new GitHubClient(new ProductHeaderValue("babo-choco-for-you"))
        {
            Credentials = new Credentials(_gitHubKey)
        };

        public async Task<List<ChocoPackage>> GetGist()
        {
            var client = GetClient();

            var gist = await client.Gist.Get(_gitHubGistId);

            return JsonConvert.DeserializeObject<List<ChocoPackage>>(gist.Files["packages.json"].Content);
        }

        public async Task UpdateGist(string packages)
        {
            var client = GetClient();

            var gistFileUpdate = new GistFileUpdate()
            {
                NewFileName = "packages.json",
                Content = packages
            };

            var gistUpdate = new GistUpdate()
            {
                Description = _gitHubGistDesc,
                Files = { new KeyValuePair<string, GistFileUpdate>("packages.json", gistFileUpdate) }
            };

            await client.Gist.Edit(_gitHubGistId, gistUpdate);
        }

        public void CreateSettings(string gitHubKey, string gitHubGistId, string gitHubGistDesc)
        {
            var gitHubSettings = new GitHubSettings()
            {
                GitHubKey = gitHubKey.ThrowIfNullOrEmpty(nameof(gitHubKey)),
                GitHubGistId = gitHubGistId.ThrowIfNullOrEmpty(nameof(gitHubGistId)),
                GitHubGistDesc = gitHubGistDesc.ThrowIfNullOrEmpty(nameof(gitHubGistDesc))
            };

            using (var file = File.CreateText(GitHubSettingsFile))
            {
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(file, gitHubSettings);
            }
        }

        public void GetSettings()
        {
            var gitHubSettingsText = File.ReadAllText(GitHubSettingsFile);

            (_gitHubKey, _gitHubGistId, _gitHubGistDesc) = JsonConvert.DeserializeObject<GitHubSettings>(gitHubSettingsText);
        }
    }
}