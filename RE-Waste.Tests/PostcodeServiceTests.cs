
using System.Net;
using System.Net.Http;
using System.Text;
using RE_Waste.Services;
using Xunit;

namespace RE_Waste.Tests
{
    // Tests for PostcodeService.
    // A fake HTTP handler is used so tests never contact Postcodes.io.
    public class PostcodeServiceTests
    {
        // Fake HTTP handler that records requests and returns controlled responses.
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _response;

            public List<string> Requests { get; } = new();

            public FakeHttpMessageHandler(
                Func<HttpRequestMessage, HttpResponseMessage> response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                Requests.Add(request.RequestUri!.PathAndQuery);

                return Task.FromResult(_response(request));
            }
        }

        // Creates a successful fake API response containing coordinates.
        private static HttpResponseMessage SuccessfulResponse()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"result\":{\"latitude\":55.9533,\"longitude\":-3.1883}}",
                    Encoding.UTF8,
                    "application/json")
            };
        }

        // TEST 1: Valid Scottish postcode.
        [Fact]
        public async Task LookupAsync_ValidScottishPostcode_ReturnsCoordinates()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                request => SuccessfulResponse());

            using var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://postcodes.io/")
            };

            var service = new PostcodeService(http);

            // Act
            var result = await service.LookupAsync("EH1 1AB");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(55.9533, result.Latitude, 4);
            Assert.Equal(-3.1883, result.Longitude, 4);
        }

        // TEST 2: Lowercase Scottish postcode is converted to uppercase.
        [Fact]
        public async Task LookupAsync_LowercasePostcode_NormalisesToUppercase()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                request => SuccessfulResponse());

            using var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://postcodes.io/")
            };

            var service = new PostcodeService(http);

            // Act
            var result = await service.LookupAsync("eh1 1ab");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("/postcodes/EH1%201AB", handler.Requests[0]);
        }

        // TEST 3: Leading and trailing spaces are removed.
        [Fact]
        public async Task LookupAsync_PostcodeWithExtraSpaces_TrimsInput()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                request => SuccessfulResponse());

            using var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://postcodes.io/")
            };

            var service = new PostcodeService(http);

            // Act
            var result = await service.LookupAsync("  EH1 1AB  ");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("/postcodes/EH1%201AB", handler.Requests[0]);
        }

        // TEST 4: Invalid Scottish postcode is not recognised.
        [Fact]
        public async Task LookupAsync_InvalidScottishPostcode_ReturnsNull()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                request => new HttpResponseMessage(HttpStatusCode.NotFound));

            using var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://postcodes.io/")
            };

            var service = new PostcodeService(http);

            // Act
            var result = await service.LookupAsync("EH99 9ZZ");

            // Assert
            Assert.Null(result);
        }

        // TEST 5: Empty postcode returns null without making an API request.
        [Fact]
        public async Task LookupAsync_EmptyPostcode_ReturnsNullWithoutApiCall()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                request => SuccessfulResponse());

            using var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://postcodes.io/")
            };

            var service = new PostcodeService(http);

            // Act
            var result = await service.LookupAsync("");

            // Assert
            Assert.Null(result);
            Assert.Empty(handler.Requests);
        }

        // TEST 6: The single letter "a" from the manager log is not recognised.
        [Fact]
        public async Task LookupAsync_SingleLetterA_ReturnsNull()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                request => new HttpResponseMessage(HttpStatusCode.NotFound));

            using var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://postcodes.io/")
            };

            var service = new PostcodeService(http);

            // Act
            var result = await service.LookupAsync("a");

            // Assert
            Assert.Null(result);
            Assert.Single(handler.Requests);
            Assert.Contains("/postcodes/A", handler.Requests[0]);
        }

        // TEST 7: English postcode is accepted when the API returns coordinates.
        [Fact]
        public async Task LookupAsync_EnglishPostcode_ReturnsCoordinates()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                request => SuccessfulResponse());

            using var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://postcodes.io/")
            };

            var service = new PostcodeService(http);

            // Act
            var result = await service.LookupAsync("SW1A 1AA");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(55.9533, result.Latitude, 4);
            Assert.Equal(-3.1883, result.Longitude, 4);
        }

        // TEST 8: Postcode without an internal space is passed to the API.
        [Fact]
        public async Task LookupAsync_PostcodeWithoutSpace_PassesInputToApi()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                request => SuccessfulResponse());

            using var http = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://postcodes.io/")
            };

            var service = new PostcodeService(http);

            // Act
            var result = await service.LookupAsync("EH11AB");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("/postcodes/EH11AB", handler.Requests[0]);
        }
    }
}
