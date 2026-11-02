using PageKrafter.Components;

namespace PageKrafter.Tests;

internal static class TestHost
{
    public static IServiceProvider Create(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPageKrafter();
        configure?.Invoke(services);
        return services.BuildServiceProvider(validateScopes: true).CreateScope().ServiceProvider;
    }

    public static BlockRegistry Registry(Action<IServiceCollection>? configure = null) =>
        Create(configure).GetRequiredService<BlockRegistry>();

    public static PageDocument Page(params BlockNode[] children) => new() { Root = Nodes.Page(children) };
}

internal sealed class ManualTimeProvider : TimeProvider
{
    public DateTimeOffset Now { get; set; } = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => Now;
}
