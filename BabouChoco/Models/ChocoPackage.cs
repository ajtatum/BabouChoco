using System.ComponentModel;

namespace BabouChoco.Models
{
    public class ChocoPackage
    {
        public string Name { get; set; }

        [DisplayName("Installed Version")]
        public string InstalledVersion { get; set; }
    }
}