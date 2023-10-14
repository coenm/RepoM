namespace RepoM.ActionMenu.CodeGen.Models
{
    public abstract class KalkDescriptorToGenerate : KalkDescriptor
    {
        public bool IsModule { get; set; }

        public bool IsBuiltin { get; set; }
    }
}