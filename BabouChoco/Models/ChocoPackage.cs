namespace BabouChoco.Models
{
    public class ChocoPackage
    {
        public ChocoPackage() { }

        public ChocoPackage(string id, string category)
        {
            Id = id;
            Category = category;
        }

        public string Id { get; set; }
        public string Category { get; set; }
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