using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using RE_Waste.Models;

namespace RE_Waste.Services
{
    // R3: turns a postcode the user typed (e.g. "EH1 2NG") into a latitude and longitude using the free postcodes.io API (no API key needed).
    // Also accepts a district on its own (e.g. "EH1"), because saved responses only keep the district for privacy (U5).
    public class PostcodeService : IPostcodeService
    {
        // A UK district (outcode) is 1-2 letters, a digit, then an optional letter or digit, e.g. G1, G12, EH1, KW15, EC1A
        private static readonly Regex DistrictPattern = new(@"^[A-Z]{1,2}[0-9][A-Z0-9]?$", RegexOptions.Compiled);

        private readonly HttpClient _http;

        // HttpClient is given to us by ASP.NET (registered with AddHttpClient in Program.cs), so we never create one ourselves
        public PostcodeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<GeoLocation?> LookupAsync(string postcode)
        {
            // a. Tidy the input, and don't call the internet for an empty value
            var cleaned = (postcode ?? string.Empty).Trim().ToUpperInvariant();
            if (cleaned.Length == 0 || cleaned.Length > 8) // longest UK postcode is 8 characters with the space, so anything longer can't be valid
            {
                return null;
            }

            try
            {
                // b. Try it as a full postcode first
                using var response = await _http.GetAsync($"postcodes/{Uri.EscapeDataString(cleaned)}");

                if (response.IsSuccessStatusCode)
                {
                    return await ReadLocationAsync(response);
                }

                // c. Not a full postcode, but it might be a district on its own, so ask for the district's centre instead
                if (response.StatusCode == HttpStatusCode.NotFound && DistrictPattern.IsMatch(cleaned))
                {
                    using var districtResponse = await _http.GetAsync($"outcodes/{Uri.EscapeDataString(cleaned)}");
                    if (districtResponse.IsSuccessStatusCode)
                    {
                        return await ReadLocationAsync(districtResponse);
                    }
                }

                // e. 404 or any other error status means we don't recognise it
                return null;
            }
            // e. Network problem, timeout or unexpected JSON: return null so the page shows a message instead of crashing
            catch (HttpRequestException)
            {
                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (Exception ex) when (ex is JsonException or NotSupportedException)
            {
                return null;
            }
        }

        // d. Reads only the two fields we need and turns them into a GeoLocation. Null if the coordinates are missing.
        private static async Task<GeoLocation?> ReadLocationAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadFromJsonAsync<PostcodesIoResponse>();
            var result = body?.Result;

            if (result?.Latitude is not double latitude || result.Longitude is not double longitude)
            {
                return null;
            }

            return new GeoLocation { Latitude = latitude, Longitude = longitude };
        }

        // Small private classes matching the parts of the postcodes.io JSON we read: { "result": { "latitude": ..., "longitude": ... } }
        // Latitude and Longitude are nullable because postcodes.io returns null for a few postcodes it can't place on the map
        private class PostcodesIoResponse
        {
            public PostcodesIoResult? Result { get; set; }
        }

        private class PostcodesIoResult
        {
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
        }
    }
}
