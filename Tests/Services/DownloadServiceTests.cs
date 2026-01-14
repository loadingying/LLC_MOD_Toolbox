using LLC_MOD_Toolbox.Services;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace LLC_MOD_Toolbox.Tests.Services
{
    public class DownloadServiceTests
    {
        [Fact]
        public async System.Threading.Tasks.Task DownloadStringAsync_ShouldReturnContent()
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("hello", Encoding.UTF8, "text/plain")
            });
            var service = new DownloadService(handler);

            var result = await service.DownloadStringAsync("http://localhost/test");

            Assert.Equal("hello", result);
        }

        [Fact]
        public async System.Threading.Tasks.Task DownloadJsonAsync_ShouldDeserializeContent()
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"name\":\"test\"}", Encoding.UTF8, "application/json")
            });
            var service = new DownloadService(handler);

            var result = await service.DownloadJsonAsync<TestPayload>("http://localhost/test");

            Assert.NotNull(result);
            Assert.Equal("test", result.Name);
        }

        [Fact]
        public void CalculateSHA256_ShouldReturnExpectedHash()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"download_hash_{Guid.NewGuid()}.txt");
            File.WriteAllText(tempFile, "abc");
            try
            {
                var service = new DownloadService(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
                var hash = service.CalculateSHA256(tempFile);
                Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

            public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            {
                _handler = handler;
            }

            protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                System.Threading.CancellationToken cancellationToken)
            {
                return System.Threading.Tasks.Task.FromResult(_handler(request));
            }
        }

        private sealed class TestPayload
        {
            public string? Name { get; set; }
        }
    }
}
