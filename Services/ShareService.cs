using System.Text;
using System.Text.Json;
using System.IO.Compression;
using RoadScript.Models;

namespace RoadScript.Services;

/// <summary>
/// Service for generating shareable URLs with compressed roadmap data
/// </summary>
public class ShareService
{
    /// <summary>
    /// Generate a shareable URL from roadmap data
    /// </summary>
    public static string GenerateShareUrl(RoadmapData data, string baseUrl)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var compressed = CompressString(json);
        var base64 = Convert.ToBase64String(compressed);

        // Make URL-safe (replace +, /, = with URL-safe characters)
        var urlSafe = base64
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        return $"{baseUrl}#share/{urlSafe}";
    }

    /// <summary>
    /// Extract roadmap data from a share URL
    /// </summary>
    public static RoadmapData? ParseShareUrl(string shareCode)
    {
        try
        {
            // Restore URL-safe characters to base64
            var base64 = shareCode
                .Replace("-", "+")
                .Replace("_", "/");

            // Add padding if needed
            var padding = (4 - base64.Length % 4) % 4;
            base64 += new string('=', padding);

            var compressed = Convert.FromBase64String(base64);
            var json = DecompressString(compressed);

            return JsonSerializer.Deserialize<RoadmapData>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Compress a string using GZip
    /// </summary>
    private static byte[] CompressString(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);

        using var memoryStream = new MemoryStream();
        using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
        {
            gzipStream.Write(bytes, 0, bytes.Length);
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Decompress a GZip compressed byte array to string
    /// </summary>
    private static string DecompressString(byte[] compressed)
    {
        using var memoryStream = new MemoryStream(compressed);
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream, Encoding.UTF8);

        return reader.ReadToEnd();
    }

    /// <summary>
    /// Get estimated URL length for a roadmap
    /// </summary>
    public static int EstimateUrlLength(RoadmapData data, string baseUrl)
    {
        var shareUrl = GenerateShareUrl(data, baseUrl);
        return shareUrl.Length;
    }

    /// <summary>
    /// Check if roadmap is small enough to share via URL (under 2000 chars is safe)
    /// </summary>
    public static bool CanShareViaUrl(RoadmapData data, string baseUrl)
    {
        return EstimateUrlLength(data, baseUrl) < 2000;
    }
}
