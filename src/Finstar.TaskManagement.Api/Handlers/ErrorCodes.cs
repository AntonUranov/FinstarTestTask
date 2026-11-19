namespace Finstar.TaskManagement.Api.Handlers;

public static class ErrorCodes
{
    public static class CreateTaskItem
    {
        public const string UserNotFound = "UserNotFound";
    }

    public static class GetTaskItem
    {
        public const string ItemNotFound = "ItemNotFound";
    }
}