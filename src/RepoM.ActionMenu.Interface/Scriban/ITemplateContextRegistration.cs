namespace RepoM.ActionMenu.Interface.Scriban;

public abstract class TemplateContextRegistrationBase : ITemplateContextRegistration
{
    public virtual void RegisterFunctionsAuto(IContextRegistration contextRegistration)
    {
    }
}

public interface ITemplateContextRegistration
{
    void RegisterFunctionsAuto(IContextRegistration contextRegistration);
}