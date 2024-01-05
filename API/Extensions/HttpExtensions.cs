using System.Text.Json;
using API.Helpers;

namespace API.Extensions;

public static class HttpExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public static void AddPaginationHeader(this HttpResponse respnse, PaginationHeader header)
    {
        respnse.Headers.Append("Pagination", JsonSerializer.Serialize(header, _jsonOptions));
        respnse.Headers.Append("Action-Control-Expose-Headers", "Pagination");
    }
}
