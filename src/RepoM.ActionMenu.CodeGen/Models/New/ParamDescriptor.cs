namespace RepoM.ActionMenu.CodeGen.Models.New
{
    public class ParamDescriptor
    {
        public ParamDescriptor(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsOptional { get; set; }
    }
}