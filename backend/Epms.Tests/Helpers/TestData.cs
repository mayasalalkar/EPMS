using System.Text.Json;
using System.Text.Json.Serialization;

namespace Epms.Tests.Helpers;

public static class TestData
{
    public const string AdminEmail = "admin@epms.local";
    public const string AdminPassword = "Admin@123";
    public const string PriyaEmail = "priya.sharma@epms.local";
    public const string RahulEmail = "rahul.verma@epms.local";
    public const string EmployeePassword = "Employee@123";

    public static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public static string UniqueEmail(string prefix = "user") =>
        $"{prefix}.{Guid.NewGuid():N}@test.local";

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
