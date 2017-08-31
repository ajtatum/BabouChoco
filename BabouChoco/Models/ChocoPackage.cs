using System.ComponentModel;

namespace BabouChoco.Models
{
    public class ChocoPackage
    {
        public string Id { get; set; }
        [DisplayName("Installed Version")]
        public string InstalledVersion { get; set; }
        [DisplayName("Latest Version")]
        public string LatestVersion { get; set; }
        public bool Sync { get; set; }
        [DisplayName("Sync With")]
        public string[] SyncComputerNames { get; set; }
    }

    public class ChocoPackageDisplay : ChocoPackage
    {
        [DisplayName("Sync With")]
        public new string SyncComputerNames { get; set; }
    }
}