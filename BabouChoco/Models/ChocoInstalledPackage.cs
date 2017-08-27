using System.ComponentModel;

namespace BabouChoco.Models
{
    public class ChocoInstalledPackage : ChocoPackage
    {
        [DisplayName("Is Latest Version")]
        public bool IsLatestVersion { get; set; }
        [DisplayName("Latest Version")]
        public string LatestVersion { get; set; }
    }
}
