using System.Buffers.Text;
using Api.Pagination;
using System.Text;
using Common.Models;

namespace Tests.Api.Pagination;

public class ProductCursorCodecTests
{
    public static TheoryData<DateTimeOffset, Guid> ExactRoundTripCases()
    {
        var fractional = new DateTimeOffset(2026, 7, 8, 9, 10, 11, TimeSpan.Zero)
            .AddTicks(1_234_567);

        return new TheoryData<DateTimeOffset, Guid>
        {
            {
                fractional,
                Guid.Parse("12345678-90ab-cdef-1234-567890abcdef")
            },
            {
                fractional.AddTicks(1),
                Guid.Parse("00000001-0000-0000-0000-000000000000")
            },
            {
                new DateTimeOffset(0, TimeSpan.Zero),
                Guid.Empty
            },
            {
                DateTimeOffset.MaxValue,
                Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff")
            }
        };
    }

    [Theory]
    [MemberData(nameof(ExactRoundTripCases))]
    public void EncodeThenDecode_PreservesExactUtcTicksAndGuid(
        DateTimeOffset lastDate,
        Guid lastId)
    {
        var cursor = new ProductCursor { LastDate = lastDate, LastId = lastId };

        var token = ProductCursorCodec.Encode(cursor);
        var decoded = Decode(token);

        Assert.Equal(lastDate.UtcTicks, decoded.LastDate.UtcTicks);
        Assert.Equal(lastDate.Offset, decoded.LastDate.Offset);
        Assert.Equal(lastId, decoded.LastId);
    }

    [Fact]
    public void EncodeThenDecode_EquivalentNonUtcInstant_PreservesSameUtcTicks()
    {
        var utcDate = new DateTimeOffset(2026, 8, 9, 10, 11, 12, TimeSpan.Zero)
            .AddTicks(7_654_321);
        var offsetDate = utcDate.ToOffset(TimeSpan.FromMinutes(330));
        var id = Guid.Parse("fedcba98-7654-3210-fedc-ba9876543210");

        var utcDecoded = Decode(ProductCursorCodec.Encode(
            new ProductCursor { LastDate = utcDate, LastId = id }));
        var offsetDecoded = Decode(ProductCursorCodec.Encode(
            new ProductCursor { LastDate = offsetDate, LastId = id }));

        Assert.Equal(utcDate.UtcTicks, offsetDate.UtcTicks);
        Assert.Equal(utcDate.UtcTicks, utcDecoded.LastDate.UtcTicks);
        Assert.Equal(utcDate.UtcTicks, offsetDecoded.LastDate.UtcTicks);
        Assert.Equal(offsetDate.Offset, offsetDecoded.LastDate.Offset);
        Assert.Equal(id, utcDecoded.LastId);
        Assert.Equal(id, offsetDecoded.LastId);
    }

    [Theory]
    [InlineData("0001-01-01T00:00:00+00:00", "12345678-90ab-cdef-1234-567890abcdef")]
    [InlineData("2026-07-08T09:10:11.1234567+05:30", "00000000-0000-0000-0000-000000000000")]
    [InlineData("0001-01-01T00:00:00+00:00", "00000000-0000-0000-0000-000000000000")]
    [InlineData("9999-12-31T23:59:59.9999999+00:00", "ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public void TryDecode_IndependentJson_AcceptsExactBoundary(string date, string id)
    {
        var json = $$"""{"LastDate":"{{date}}","LastId":"{{id}}"}""";

        var decoded = Decode(Base64Url.EncodeToString(Encoding.UTF8.GetBytes(json)));

        var expectedDate = DateTimeOffset.Parse(date, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(expectedDate.UtcTicks, decoded.LastDate.UtcTicks);
        Assert.Equal(expectedDate.Offset, decoded.LastDate.Offset);
        Assert.Equal(Guid.Parse(id), decoded.LastId);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("ab+c")]
    [InlineData("ab/c")]
    [InlineData("ab==")]
    [InlineData("ab c")]
    [InlineData(" ")]
    [InlineData(" ab")]
    [InlineData("ab\nc")]
    [InlineData("*")]
    public void TryDecode_InvalidBase64Url_ReturnsFalse(string token)
    {
        Assert.False(ProductCursorCodec.TryDecode(token, out var cursor));
        Assert.Null(cursor);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("""{"LastId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("""{"LastDate":"0001-01-01T00:00:00+00:00"}""")]
    [InlineData("""{"LastDate":null,"LastId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("""{"LastDate":"0001-01-01T00:00:00+00:00","LastId":null}""")]
    [InlineData("""{"LastDate":0,"LastId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("""{"LastDate":"0001-01-01T00:00:00+00:00","LastId":0}""")]
    [InlineData("""{"LastDate":"not-a-date","LastId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("""{"LastDate":"2026-02-30T00:00:00+00:00","LastId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("""{"LastDate":"10000-01-01T00:00:00+00:00","LastId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("""{"LastDate":"0001-01-01T00:00:00+00:00","LastId":"not-a-guid"}""")]
    [InlineData("""{"LastDate":"0001-01-01T00:00:00+00:00","LastId":"00000000000000000000000000000000"}""")]
    [InlineData("""{"utcTicks":0,"lastId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("""{"LastDate":"0001-01-01T00:00:00+00:00","LastId":"\uD800"}""")]
    public void TryDecode_InvalidJsonBoundary_ReturnsFalse(string json)
    {
        var token = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(json));

        Assert.False(ProductCursorCodec.TryDecode(token, out var cursor));
        Assert.Null(cursor);
    }

    [Fact]
    public void TryDecode_InvalidUtf8String_ReturnsFalse()
    {
        var bytes = """{"LastDate":"2026-01-01T00:00:00+00:00","LastId":"x"}"""u8.ToArray();
        bytes[^3] = 0xff;
        var token = Base64Url.EncodeToString(bytes);

        Assert.False(ProductCursorCodec.TryDecode(token, out var cursor));
        Assert.Null(cursor);
    }

    [Fact]
    public void TryDecode_PreviouslyValidProtobuf_ReturnsFalse()
    {
        // Present ticks 0 and a 16-byte empty GUID were valid under the previous codec.
        var bytes = Convert.FromHexString("0800121000000000000000000000000000000000");
        var token = Base64Url.EncodeToString(bytes);

        Assert.False(ProductCursorCodec.TryDecode(token, out var cursor));
        Assert.Null(cursor);
    }

    private static ProductCursor Decode(string token)
    {
        Assert.True(ProductCursorCodec.TryDecode(token, out var cursor));
        return Assert.IsType<ProductCursor>(cursor);
    }
}
