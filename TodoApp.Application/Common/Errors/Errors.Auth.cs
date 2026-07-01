using ErrorOr;

namespace TodoApp.Application.Common.Errors;

public static partial class Errors
{
    public static class Auth
    {
        public static Error DuplicateEmail(string email) =>
            Error.Conflict("Auth.DuplicateEmail", $"Email '{email}' is already registered.");

        public static readonly Error InvalidCredentials =
            Error.Validation("Auth.InvalidCredentials", "Invalid email or password.");
    }
}
