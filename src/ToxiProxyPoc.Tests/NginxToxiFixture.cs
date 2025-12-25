using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using ToxiProxyWrapper;

namespace ToxiProxyPoc.Tests;

public sealed class NginxToxiFixture : IAsyncLifetime
{
    public ProxyEndpoint NginxProxy { get; private set; } = null!;

    private const string _nginxAlias = "nginx";

    private readonly INetwork _network = new NetworkBuilder().Build();
    private readonly IContainer _nginx;

    public ToxiProxyContainer Toxi { get; }
    
    public NginxToxiFixture()
    {
        _nginx = new ContainerBuilder()
            .WithImage("nginx:alpine")
            .WithNetwork(_network)
            .WithNetworkAliases(_nginxAlias)
            .Build();

        Toxi = new ToxiProxyContainer(
            network: _network);
    }

    public async Task InitializeAsync()
    {
        await _network.CreateAsync();

        await _nginx.StartAsync();
        await Toxi.StartAsync();

        NginxProxy = await Toxi.CreateProxyAsync(
            name: "nginx-proxy",
            proxiedHost:_nginxAlias,
            proxiedPort: 80
            );
    }

    public async Task DisposeAsync()
    {
        try { await Toxi.RestoreAllAsync(); } catch { /* ignore */ }

        await Toxi.DisposeAsync();
        await _nginx.DisposeAsync();
        await _network.DisposeAsync();
    }
}