using System;
using openrmf_msg_audit.Classes;
using Xunit;

namespace tests.Classes;

public class CompressionTests
{
    [Fact]
    public void CompressAndDecompress_RoundTripsOriginalText()
    {
        const string original = "OpenRMF audit payload with spaces and 12345 symbols.";

        var compressed = Compression.CompressString(original);
        var decompressed = Compression.DecompressString(compressed);

        Assert.False(string.IsNullOrWhiteSpace(compressed));
        Assert.Equal(original, decompressed);
        Assert.NotEqual(original, compressed);
    }

    [Fact]
    public void CompressAndDecompress_HandlesEmptyString()
    {
        const string original = "";

        var compressed = Compression.CompressString(original);
        var decompressed = Compression.DecompressString(compressed);

        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void DecompressString_WithInvalidBase64_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Compression.DecompressString("not-base64"));
    }

    [Fact]
    public void DecompressString_WithCorruptPayload_ThrowsException()
    {
        var compressed = Compression.CompressString("valid data");
        var corrupt = compressed[..^2];

        Assert.ThrowsAny<Exception>(() => Compression.DecompressString(corrupt));
    }
}
