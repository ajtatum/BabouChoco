using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Management.Automation;
using System.Threading;
using System.Windows.Controls;
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
        private static string _gitHubKey;
        private static string _gitHubGistId;
        private static string _gitHubGistDesc;

        public string ProjectDirectory => Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.FullName;
        public string GitHubSettingsFile => $@"{ProjectDirectory}\Settings\GitHubSettings.json";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            GetGitHubSettings();
            await GetInstalledPackages();
        }

        private async void BtnSyncToGitHub_OnClickAsync(object sender, RoutedEventArgs e)
        {
            var chocoPackages = ((List<ChocoPackageDisplay>) DgInstalledChocoPackages.ItemsSource)
                .Select(x => new ChocoPackage
                    {
                        Id = x.Id,
                        InstalledVersion = x.InstalledVersion,
                        Sync = true,
                        SyncComputerNames = x.SyncComputerNames.Split(',')
                    }
                );

            var jsonChocoPackages = JsonConvert.SerializeObject(chocoPackages, Formatting.Indented);

            await UpdateChocoPackages(jsonChocoPackages);
        }

        private GitHubClient GetGitHubClient()
        {
            return new GitHubClient(new ProductHeaderValue("babo-choco-for-you"))
            {
                Credentials = new Credentials(_gitHubKey)
            };
        }

        private async Task<List<ChocoPackage>> GetGitHubGistChocoPackages()
        {
            var client = GetGitHubClient();

            var gist = await client.Gist.Get(_gitHubGistId);

            return JsonConvert.DeserializeObject<List<ChocoPackage>>(gist.Files["packages.json"].Content);
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
                Description = _gitHubGistDesc,
                Files = {new KeyValuePair<string, GistFileUpdate>("packages.json", gistFileUpdate)}
            };

            await client.Gist.Edit(_gitHubGistId, gistUpdate);
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

            (_gitHubKey, _gitHubGistId, _gitHubGistDesc) = JsonConvert.DeserializeObject<GitHubSettings>(gitHubSettingsText);
        }

        private async Task<List<string>> GetInstalledPackages()
        {
            var chocoPackages = new List<ChocoPackageDisplay>();
            var gitHubGistChocoPackages = await GetGitHubGistChocoPackages();

            using (var ps = PowerShell.Create())
            {
                ps.AddScript("choco list -lo");

                // prepare a new collection to store output stream objects
                var outputCollection = new PSDataCollection<PSObject>();
                outputCollection.DataAdded += OutputCollection_DataAdded;

                // the streams (Error, Debug, Progress, etc) are available on the PowerShell instance.
                // we can review them during or after execution.
                // we can also be notified when a new item is written to the stream (like this):
                ps.Streams.Error.DataAdded += Error_DataAdded;

                var result = ps.BeginInvoke<PSObject, PSObject>(null, outputCollection);
                while (!result.IsCompleted)
                {
                    Console.WriteLine(@"Waiting for pipeline to finish...");
                    Thread.Sleep(1000);
                }
                Console.WriteLine($@"Execution has stopped. The pipeline state: {ps.InvocationStateInfo.State}");

                foreach (var outputItem in outputCollection)
                {
                    var outputString = outputItem.BaseObject.ToString();

                    if (!string.IsNullOrWhiteSpace(outputString))
                    {
                        if (!outputString.Contains("packages installed."))
                        {
                            Console.WriteLine($@"{outputString} - adding to list");

                            var outputArray = outputString.Split(' ');

                            var package = new ChocoPackageDisplay()
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
                    else
                    {
                        Console.WriteLine(@"No items!");
                    }
                }
            }

            chocoPackages.ForEach(x =>
            {
                var gitHubChocoPackage = gitHubGistChocoPackages.FirstOrDefault(y => y.Id == x.Id);
                var syncComputerNames = new List<string>();
                if (gitHubChocoPackage != null)
                {
                    syncComputerNames.AddRange(gitHubChocoPackage.SyncComputerNames);
                    if (syncComputerNames.Contains(Environment.MachineName))
                    {
                        x.Sync = gitHubChocoPackage.Sync;
                    }
                }
                syncComputerNames.Add(Environment.MachineName);

                x.SyncComputerNames = string.Join(",", syncComputerNames.Distinct());
            });

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
