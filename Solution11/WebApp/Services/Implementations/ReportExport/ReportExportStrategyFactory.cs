namespace WebApplication1.Services.Implementations.ReportExport;

/// <summary>Selects the export strategy for the requested format.</summary>
public sealed class ReportExportStrategyFactory
{
    private readonly IReadOnlyDictionary<string, IReportExportStrategy> _strategies;

    public ReportExportStrategyFactory(IEnumerable<IReportExportStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(
            strategy => strategy.Format,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReportExportStrategy GetStrategy(string format)
    {
        if (_strategies.TryGetValue(format, out var strategy))
            return strategy;

        throw new ArgumentException($"Unsupported report format: {format}", nameof(format));
    }
}
