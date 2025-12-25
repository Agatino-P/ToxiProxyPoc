using Toxiproxy.Net.Toxics;

namespace ToxiProxyWrapper;

public sealed class ProxyEndpoint
{
    public string Name { get; }
    public string Host { get; }
    public int Port { get; }

    private readonly Func<Task> _disable;
    private readonly Func<Task> _enable;
    private readonly Func<string, int, int, ToxicDirection, Task> _addLatency;
    private readonly Func<string, int, ToxicDirection, Task> _addTimeout;
    private readonly Func<string, Task> _removeToxic;

    internal ProxyEndpoint(
        string name,
        string host,
        int port,
        Func<Task> disable,
        Func<Task> enable,
        Func<string, int, int, ToxicDirection, Task> addLatency,
        Func<string, int, ToxicDirection, Task> addTimeout,
        Func<string, Task> removeToxic)
    {
        Name = name;
        Host = host;
        Port = port;
        _disable = disable;
        _enable = enable;
        _addLatency = addLatency;
        _addTimeout = addTimeout;
        _removeToxic = removeToxic;
    }

    public Task DisableAsync() => _disable();
    public Task EnableAsync() => _enable();

    public Task AddLatencyAsync(
        int latencyMs,
        int jitterMs = 0,
        string toxicName = "latency",
        ToxicDirection direction = ToxicDirection.DownStream)
        => _addLatency(toxicName, latencyMs, jitterMs, direction);

    public Task AddTimeoutAsync(
        int timeoutMs,
        string toxicName = "timeout",
        ToxicDirection direction = ToxicDirection.DownStream)
        => _addTimeout(toxicName, timeoutMs, direction);

    public Task RemoveToxicAsync(string toxicName) => _removeToxic(toxicName);
}