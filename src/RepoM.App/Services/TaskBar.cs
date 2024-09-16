namespace RepoM.App.Services;

using System.Windows;
using System.Windows.Forms;

public static class TaskBar
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

    public static Point GetWindowPlacement(Rect workArea, double height, double width, Screen? primaryScreen)
    {
        var topY = workArea.Top;
        var bottomY = workArea.Height - height;
        var leftX = workArea.Left;
        var rightX = workArea.Width - width;

        return GetTaskBarLocation(primaryScreen) switch
            {
                TaskBarLocation.Top => new Point(rightX, topY),
                TaskBarLocation.Left => new Point(leftX, bottomY),
                TaskBarLocation.Bottom or TaskBarLocation.Right => new Point(rightX, bottomY),
                _ => new Point(rightX, bottomY),
            };
    }
}