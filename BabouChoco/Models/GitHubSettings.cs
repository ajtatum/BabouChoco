namespace BabouChoco.Models
{
    public class GitHubSettings
    {
        public string GitHubKey { get; set; }
        public string GitHubGistId { get; set; }
        public string GitHubGistDesc { get; set; }

        public GitHubSettings() { }

        public GitHubSettings(string gitHubKey, string gitHubGistId, string gitHubGistDesc)
        {
            GitHubKey = gitHubKey;
            GitHubGistId = gitHubGistId;
            GitHubGistDesc = gitHubGistDesc;
        }

        public void Deconstruct(out string gitHubKey, out string gitHubGistId, out string gitHubGistDesc)
        {
            gitHubKey = GitHubKey;
            gitHubGistId = GitHubGistId;
            gitHubGistDesc = GitHubGistDesc;
        }
    }
}