using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Management.Automation;
using System.Threading;
using BabouExtensions;
using BabouChoco.Models;
using Newtonsoft.Json;
using Octokit;


namespace BabouChoco
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string GitHubKey;
        private string GitHubGistId;
        private string GitHubGistDesc;

        public string ProjectDirectory => Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.FullName;
        public string GitHubSettingsFile => $@"{ProjectDirectory}\Settings\GitHubSettings.json";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            GetInstalledPackages();
        }

        private async void BtnSyncToGitHub_OnClickAsync(object sender, RoutedEventArgs e)
        {
            var chocoPackages = ((List<ChocoInstalledPackage>) DgInstalledChocoPackages.ItemsSource)
                .Select(x => new ChocoPackage
                    {
                        Id = x.Id,
                        Sync = true
                    }
                );

            var jsonChocoPackages = JsonConvert.SerializeObject(chocoPackages, Formatting.Indented);

            GetGitHubSettings();

            await UpdateChocoPackages(jsonChocoPackages).ConfigureAwait(false);
            //foreach (var item in (List<ChocoInstalledPackage>)DgInstalledChocoPackages.ItemsSource)
            //{
            //    var chocoInstalledPackage = (ChocoInstalledPackage) item;
            //}
        }

        private GitHubClient GetGitHubClient()
        {
            var client = new GitHubClient(new ProductHeaderValue("babo-choco-for-you"));

            var tokenAuth = new Credentials(GitHubKey);
            client.Credentials = tokenAuth;

            return client;
        }

        private async Task<string> GetChocoPackages()
        {
            var client = GetGitHubClient();

            var gist = await client.Gist.Get(GitHubGistId);
            return gist.Files["packages.json"].Content;
        }

        private async Task UpdateChocoPackages(string packages)
        {
            var client = GetGitHubClient();

            var gistFileUpdate = new GistFileUpdate()
            {
                NewFileName = "packages.json",
                Content = packages
            };

            var gistUpdate = new GistUpdate()
            {
                Description = GitHubGistDesc,
                Files = {new KeyValuePair<string, GistFileUpdate>("packages.json", gistFileUpdate)}
            };

            await client.Gist.Edit(GitHubGistId, gistUpdate);
        }

        private void CreateGitHubSettings(string gitHubKey, string gitHubGistId, string gitHubGistDesc)
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

        private void GetGitHubSettings()
        {
            var gitHubSettingsText = File.ReadAllText(GitHubSettingsFile);

            (GitHubKey, GitHubGistId, GitHubGistDesc) = JsonConvert.DeserializeObject<GitHubSettings>(gitHubSettingsText);
        }

        private List<string> GetInstalledPackages()
        {
            var chocoPackages = new List<ChocoPackage>();

            using (var ps = PowerShell.Create())
            {
                ps.CreateNestedPowerShell();
                ps.AddScript("choco list -lo");

                // prepare a new collection to store output stream objects
                var outputCollection = new PSDataCollection<PSObject>();
                outputCollection.DataAdded += OutputCollection_DataAdded;

                // the streams (Error, Debug, Progress, etc) are available on the PowerShell instance.
                // we can review them during or after execution.
                // we can also be notified when a new item is written to the stream (like this):
                ps.Streams.Error.DataAdded += Error_DataAdded;

                var result = ps.BeginInvoke<PSObject, PSObject>(null, outputCollection);
                while (result.IsCompleted == false)
                {
                    Console.WriteLine(@"Waiting for pipeline to finish...");
                    Thread.Sleep(1000);
                }
                Console.WriteLine($@"Execution has stopped. The pipeline state: {ps.InvocationStateInfo.State}");

                foreach (var outputItem in outputCollection)
                {
                    var outputString = outputItem.BaseObject.ToString();

                    if (!outputString.Contains("packages installed."))
                    {
                        Console.WriteLine($@"{outputString} - adding to list");

                        var outputArray = outputString.Split(' ');

                        var package = new ChocoInstalledPackage()
                        {
                            Id = outputArray[0].Trim(),
                            InstalledVersion = outputArray[1].Trim()
                        };

                        chocoPackages.Add(package);
                    }
                    else
                    {
                        Console.WriteLine($@"{outputString} - not adding to list");
                    }
                }
            }

            DgInstalledChocoPackages.AutoGenerateColumns = true;
            DgInstalledChocoPackages.ItemsSource = chocoPackages;

            return chocoPackages.Select(x => x.Id).ToList();
        }

        /// <summary>
        /// Event handler for when data is added to the output stream.
        /// </summary>
        /// <param name="sender">Contains the complete PSDataCollection of all output items.</param>
        /// <param name="e">Contains the index ID of the added collection item and the ID of the PowerShell instance this event belongs to.</param>
        private void OutputCollection_DataAdded(object sender, DataAddedEventArgs e)
        {
            // do something when an object is written to the output stream
            Console.WriteLine(@"Object added to output.");
        }

        /// <summary>
        /// Event handler for when Data is added to the Error stream.
        /// </summary>
        /// <param name="sender">Contains the complete PSDataCollection of all error output items.</param>
        /// <param name="e">Contains the index ID of the added collection item and the ID of the PowerShell instance this event belongs to.</param>
        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            // do something when an error is written to the error stream
            Console.WriteLine(@"An error was written to the Error stream!");
        }

        
    }
}
