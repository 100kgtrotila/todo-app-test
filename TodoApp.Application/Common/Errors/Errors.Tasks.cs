using ErrorOr;

namespace TodoApp.Application.Common.Errors;

public static partial class Errors
{
    public static class Tasks
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound("Tasks.NotFound", $"Task {id} not found.");

        public static readonly Error ForbiddenAccess =
            Error.Forbidden("Tasks.Forbidden", "You do not own this task.");
    }
}
