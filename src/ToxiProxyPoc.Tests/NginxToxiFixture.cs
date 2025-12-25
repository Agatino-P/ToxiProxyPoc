using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using ToxiProxyWrapper;

namespace ToxiProxyPoc.Tests;

public sealed class NginxToxiFixture : IAsyncLifetime
{
    public ProxyEndpoint NginxProxy { get; private set; } = null!;

    private const string NginxAlias = "nginx";

    private readonly INetwork _network = new NetworkBuilder().Build();
    private readonly IContainer _nginx;

    public ToxiProxyContainer Toxi { get; }
    
    public NginxToxiFixture()
    {
        _nginx = new ContainerBuilder()
            .WithImage("nginx:alpine")
            .WithNetwork(_network)
            .WithNetworkAliases(NginxAlias)
            .Build();

        Toxi = new ToxiProxyContainer(
            network: _network);
    }

    public async Task InitializeAsync()
    {
        await _network.CreateAsync();

        await _nginx.StartAsync();
        await Toxi.StartAsync();

        // Create proxy ONCE per fixture
        NginxProxy = await Toxi.CreateProxyAsync(
            name: "nginx-proxy",
            upstreamHostAndPort: $"{NginxAlias}:80");

        // Start in a known-clean state
        await Toxi.RestoreAllAsync();
    }

    public async Task DisposeAsync()
    {
        // Best effort cleanup: reset before shutting down
        try { await Toxi.RestoreAllAsync(); } catch { /* ignore */ }

        await Toxi.DisposeAsync();
        await _nginx.DisposeAsync();
        await _network.DisposeAsync();
    }
}
