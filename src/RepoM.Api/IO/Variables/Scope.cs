namespace RepoM.Api.IO.Variables;

using System;
using System.Collections.Generic;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

public sealed class Scope : IDisposable
{
    private bool _isDisposed;

    private Scope()
    {
        Parent = null;
        Variables = new List<EvaluatedVariable>(0);
    }

    public Scope(Scope? parent, List<EvaluatedVariable> variables)
    {
        Parent = parent;
        Variables = variables;
    }

    public static Scope Empty { get; } = new Scope();

    public Scope? Parent { get; }

    public List<EvaluatedVariable> Variables { get; }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
        }
    }

    public Scope Clone()
    {
        // this is not a real clone as it uses a reference to Variables. this is ok.
        return new Scope(Parent?.Clone(), Variables);
    }
}