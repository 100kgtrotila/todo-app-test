using ErrorOr;

namespace TodoApp.Application.Common.Errors;

public static partial class Errors
{
    public static class Categories
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound("Categories.NotFound", $"Category {id} not found.");

        public static readonly Error ForbiddenAccess =
            Error.Forbidden("Categories.Forbidden", "You do not own this category.");
    }
}
