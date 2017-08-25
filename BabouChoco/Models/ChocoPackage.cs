namespace BabouChoco.Models
{
    public class ChocoPackage
    {
        public ChocoPackage() { }

        public ChocoPackage(string id, bool sync)
        {
            Id = id;
            Sync = sync;
        }

        public string Id { get; set; }
        public bool Sync { get; set; }
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