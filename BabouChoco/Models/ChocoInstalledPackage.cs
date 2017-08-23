using System.ComponentModel;

namespace BabouChoco.Models
{
    public class ChocoInstalledPackage : ChocoPackage
    {
        [DisplayName("Installed Version")]
        public string InstalledVersion { get; set; }
        [DisplayName("Is Latest Version")]
        public bool IsLatestVersion { get; set; }
        [DisplayName("Latest Version")]
        public string LatestVersion { get; set; }
    }
}
