using Shouldly;
using System.Diagnostics;

namespace ToxiProxyPoc.Tests;

using Xunit;

public sealed class NginxToxiTests : IClassFixture<NginxToxiFixture>, IAsyncLifetime
{
    private readonly NginxToxiFixture _fx;
    private readonly Uri _toxiProxyUri;

    public NginxToxiTests(NginxToxiFixture fx)
    {
        _fx = fx;
        _toxiProxyUri = new Uri($"http://{_fx.NginxToxiProxy.MappedHost}:{_fx.NginxToxiProxy.MappedPort}/");
    }

    public Task InitializeAsync() => _fx.Toxi.RestoreAllAsync();

    public Task DisposeAsync() => _fx.Toxi.RestoreAllAsync();

    [Fact]
    public async Task Request_works()
    {
        using HttpClient http = createHttpClient();

        string html = await http.GetStringAsync("/");

        html.Contains("nginx", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();
    }

    [Fact]
    public async Task Latency_can_be_injected()
    {
        await _fx.NginxToxiProxy.AddLatencyAsync(latencyMs: 800, jitterMs: 100);

        using HttpClient http = createHttpClient();

        Stopwatch sw = Stopwatch.StartNew();
        _ = await http.GetStringAsync("/");
        sw.Stop();

        sw.ElapsedMilliseconds.ShouldBeGreaterThan(650);
    }

    [Fact]
    public async Task Disabling_proxy_breaks_connectivity()
    {
        await _fx.NginxToxiProxy.DisableAsync();

        using HttpClient httpClient = createHttpClient(TimeSpan.FromSeconds(2));

        Func<Task> willFail = () => httpClient.GetStringAsync("/");

        await willFail.ShouldThrowAsync<Exception>();

        await _fx.NginxToxiProxy.EnableAsync();
    }

    private HttpClient createHttpClient(TimeSpan? timeout = null)
    {
        return new HttpClient { BaseAddress = _toxiProxyUri, Timeout = timeout ?? TimeSpan.FromSeconds(5) };
    }
}