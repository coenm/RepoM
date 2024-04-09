namespace RepoM.App.Services;

using System.Windows.Forms;

public static class TaskBarLocator
{
    public enum TaskBarLocation
    {
        Top,
        Bottom,
        Left,
        Right,
    }

    public static TaskBarLocation GetTaskBarLocation(Screen? primaryScreen)
    {
        if (primaryScreen == null)
        {
            return TaskBarLocation.Bottom;
        }

        var taskBarOnTopOrBottom = primaryScreen.WorkingArea.Width == primaryScreen.Bounds.Width;

        if (taskBarOnTopOrBottom)
        {
            return primaryScreen.WorkingArea.Top > 0
                ? TaskBarLocation.Top
                : TaskBarLocation.Bottom;
        }

        return primaryScreen.WorkingArea.Left > 0
            ? TaskBarLocation.Left
            : TaskBarLocation.Right;
    }
}