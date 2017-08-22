using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management.Automation;
using System.Threading;
using BabouChoco.Models;


namespace BabouChoco
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            GetInstalledPackages();
        }

        public List<string> GetInstalledPackages()
        {
            //var chocoPackages = new List<string>();

            var chocoPackages = new List<ChocoPackage>();

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

                        var package = new ChocoPackage()
                        {
                            Name = outputArray[0].Trim(),
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

            return chocoPackages.Select(x => x.Name).ToList();
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
