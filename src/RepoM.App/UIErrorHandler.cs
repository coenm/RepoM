namespace RepoM.App;

using System.Windows;
using RepoM.Api.Common;

public class UIErrorHandler : IErrorHandler
{
    public void Handle(string error)
    {
        MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}