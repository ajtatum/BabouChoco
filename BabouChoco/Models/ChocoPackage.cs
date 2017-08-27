using System.Collections.Generic;
using System.Windows.Documents;

namespace BabouChoco.Models
{
    public class ChocoPackage
    {
        public ChocoPackage() { }

        public ChocoPackage(string id, bool sync)
        {
            Id = id;
            Sync = sync;
            SyncComputerNames = new[] {"ALL"};
        }

        public ChocoPackage(string id, string[] syncComputerNames)
        {
            Id = id;
            Sync = true;
            SyncComputerNames = syncComputerNames;
        }

        public string Id { get; set; }
        public bool Sync { get; set; }
        public string[] SyncComputerNames { get; set; }
    }

    //public class ChocoCategory
    //{
    //    public ChocoCategory() { }

    //    public ChocoCategory(string name)
    //    {
    //        Name = name;
    //    }
    //    public string Name { get; set; }
    //    //public virtual ICollection<ChocoPackage> ChocoPackages { get; set; }
    //}
}