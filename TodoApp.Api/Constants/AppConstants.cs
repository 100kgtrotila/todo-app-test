namespace TodoApp.Api.Constants;

public static class AppConstants
{
    public static class Cors
    {
        public const string AngularDevPolicy = "AllowAngularDev";
        public const string AngularDevOrigin = "http://localhost:4200";
    }

    public static class Health
    {
        public const string Endpoint = "/health";
        public const string DatabaseTag = "database";
    }

    public static class Swagger
    {
        public const string Version = "v1";
        public const string Title = "TodoApp API";
        public const string Endpoint = "/swagger/v1/swagger.json";
        public const string SecurityScheme = "Bearer";
    }
}