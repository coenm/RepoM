namespace UiTests.VisualStudioCode.WebSockets.Commands;

public static class CommandTypes
{
    public static class WorkBench
    {
        public static class Action
        {
            public const string GO_TO_LINE = "workbench.action.gotoLine";

            public const string FOCUS_ACTIVE_EDITOR_GROUP = "workbench.action.focusActiveEditorGroup";
            public const string FOCUS_FIRST_EDITOR_GROUP = "workbench.action.focusFirstEditorGroup";
            public const string FOCUS_LAST_EDITOR_GROUP = "workbench.action.focusLastEditorGroup";
            public const string FOCUS_PREVIOUS_GROUP = "workbench.action.focusPreviousGroup";
            public const string CLOSE_OTHER_EDITORS = "workbench.action.closeOtherEditors";
        }
    }
}