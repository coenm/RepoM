namespace RepoM.ActionMenu.CodeGen.Models
{
    public abstract class KalkDescriptorToGenerateBase : KalkDescriptorBase
    {
        public bool IsModule { get; set; }

        public bool IsBuiltin { get; set; }
    }
}