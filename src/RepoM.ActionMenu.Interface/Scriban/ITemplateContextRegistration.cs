namespace RepoM.ActionMenu.Interface.Scriban;

public abstract class TemplateContextRegistrationBase : ITemplateContextRegistration
{
    public virtual void RegisterFunctions(IContextRegistration contextRegistration)
    {
    }
}

public interface ITemplateContextRegistration
{
    void RegisterFunctions(IContextRegistration contextRegistration);
}