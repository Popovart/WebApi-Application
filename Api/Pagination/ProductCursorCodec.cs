using System.Buffers.Text;
using System.Text.Json;
using System.Text.Unicode;
using Common.Models;

namespace Api.Pagination;

internal static class ProductCursorCodec
{
    internal static string Encode(ProductCursor cursor)
    {
        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(cursor);
        return Base64Url.EncodeToString(payloadBytes);
    }

    internal static bool TryDecode(string value, out ProductCursor? cursor)
    {
        cursor = null;

        try
        {
            var payloadBytes = Base64Url.DecodeFromChars(value);
            if (!Utf8.IsValid(payloadBytes))
                return false;

            cursor = JsonSerializer.Deserialize<ProductCursor>(payloadBytes);
            return cursor is not null;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
