using System.Collections.Concurrent;
using System.Diagnostics;

namespace FxExpert.Blazor.Client.Services;

/// <summary>
/// Performance monitoring service for tracking application performance metrics
/// Provides real-time monitoring of component performance, API calls, and user interactions
/// </summary>
public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly ConcurrentDictionary<string, PerformanceMetric> _metrics = new();
    private readonly ConcurrentDictionary<string, List<long>> _timings = new();
    private readonly Timer _metricsReportingTimer;
    
    // Performance thresholds (configurable)
    private const long SlowOperationThresholdMs = 1000;
    private const long VerySlowOperationThresholdMs = 3000;
    private const int MaxTimingHistoryCount = 100;

    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
    {
        _logger = logger;
        
        // Report performance metrics every 5 minutes
        _metricsReportingTimer = new Timer(ReportMetrics, null, 
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Start tracking an operation's performance
    /// </summary>
    public IDisposable TrackOperation(string operationName, string? category = null)
    {
        return new OperationTracker(this, operationName, category);
    }

    /// <summary>
    /// Record a completed operation's performance
    /// </summary>
    public void RecordOperation(string operationName, long elapsedMs, string? category = null, bool success = true)
    {
        var metricKey = $"{category ?? "General"}:{operationName}";
        
        _metrics.AddOrUpdate(metricKey, 
            new PerformanceMetric 
            { 
                OperationName = operationName,
                Category = category ?? "General",
                TotalCalls = 1,
                TotalElapsedMs = elapsedMs,
                MinElapsedMs = elapsedMs,
                MaxElapsedMs = elapsedMs,
                SuccessfulCalls = success ? 1 : 0,
                FailedCalls = success ? 0 : 1,
                LastCallTime = DateTime.UtcNow
            },
            (key, existing) =>
            {
                existing.TotalCalls++;
                existing.TotalElapsedMs += elapsedMs;
                existing.MinElapsedMs = Math.Min(existing.MinElapsedMs, elapsedMs);
                existing.MaxElapsedMs = Math.Max(existing.MaxElapsedMs, elapsedMs);
                existing.LastCallTime = DateTime.UtcNow;
                
                if (success)
                    existing.SuccessfulCalls++;
                else
                    existing.FailedCalls++;
                
                return existing;
            });

        // Track timing history for trend analysis
        _timings.AddOrUpdate(metricKey,
            new List<long> { elapsedMs },
            (key, existing) =>
            {
                existing.Add(elapsedMs);
                
                // Keep only the most recent timings
                if (existing.Count > MaxTimingHistoryCount)
                {
                    existing.RemoveRange(0, existing.Count - MaxTimingHistoryCount);
                }
                
                return existing;
            });

        // Log slow operations
        if (elapsedMs > VerySlowOperationThresholdMs)
        {
            _logger.LogWarning("Very slow operation detected: {Operation} took {ElapsedMs}ms", 
                operationName, elapsedMs);
        }
        else if (elapsedMs > SlowOperationThresholdMs)
        {
            _logger.LogInformation("Slow operation detected: {Operation} took {ElapsedMs}ms", 
                operationName, elapsedMs);
        }
    }

    /// <summary>
    /// Record a custom metric value
    /// </summary>
    public void RecordMetric(string metricName, double value, string? category = null)
    {
        var metricKey = $"{category ?? "Custom"}:{metricName}";
        
        _metrics.AddOrUpdate(metricKey,
            new PerformanceMetric
            {
                OperationName = metricName,
                Category = category ?? "Custom",
                TotalCalls = 1,
                TotalElapsedMs = (long)value,
                MinElapsedMs = (long)value,
                MaxElapsedMs = (long)value,
                LastCallTime = DateTime.UtcNow
            },
            (key, existing) =>
            {
                existing.TotalCalls++;
                existing.TotalElapsedMs += (long)value;
                existing.MinElapsedMs = Math.Min(existing.MinElapsedMs, (long)value);
                existing.MaxElapsedMs = Math.Max(existing.MaxElapsedMs, (long)value);
                existing.LastCallTime = DateTime.UtcNow;
                return existing;
            });
    }

    /// <summary>
    /// Get performance metrics for a specific operation
    /// </summary>
    public PerformanceMetric? GetMetric(string operationName, string? category = null)
    {
        var metricKey = $"{category ?? "General"}:{operationName}";
        return _metrics.TryGetValue(metricKey, out var metric) ? metric : null;
    }

    /// <summary>
    /// Get all performance metrics, optionally filtered by category
    /// </summary>
    public IEnumerable<PerformanceMetric> GetAllMetrics(string? category = null)
    {
        var metrics = _metrics.Values.AsEnumerable();
        
        if (!string.IsNullOrEmpty(category))
        {
            metrics = metrics.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }
        
        return metrics.OrderByDescending(m => m.LastCallTime);
    }

    /// <summary>
    /// Get performance summary statistics
    /// </summary>
    public PerformanceSummary GetPerformanceSummary()
    {
        var allMetrics = _metrics.Values.ToList();
        
        if (allMetrics.Count == 0)
        {
            return new PerformanceSummary();
        }

        var totalOperations = allMetrics.Sum(m => m.TotalCalls);
        var totalTime = allMetrics.Sum(m => m.TotalElapsedMs);
        var successfulOperations = allMetrics.Sum(m => m.SuccessfulCalls);
        var failedOperations = allMetrics.Sum(m => m.FailedCalls);

        var slowOperations = allMetrics
            .Where(m => m.AverageElapsedMs > SlowOperationThresholdMs)
            .OrderByDescending(m => m.AverageElapsedMs)
            .Take(5)
            .ToList();

        var mostFrequentOperations = allMetrics
            .OrderByDescending(m => m.TotalCalls)
            .Take(5)
            .ToList();

        return new PerformanceSummary
        {
            TotalOperations = totalOperations,
            TotalTimeMs = totalTime,
            AverageOperationTimeMs = totalOperations > 0 ? totalTime / (double)totalOperations : 0,
            SuccessRate = totalOperations > 0 ? successfulOperations / (double)totalOperations : 0,
            SlowOperations = slowOperations,
            MostFrequentOperations = mostFrequentOperations,
            UniqueOperations = allMetrics.Count,
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get timing trend data for an operation
    /// </summary>
    public List<long> GetTimingTrend(string operationName, string? category = null)
    {
        var metricKey = $"{category ?? "General"}:{operationName}";
        return _timings.TryGetValue(metricKey, out var timings) ? new List<long>(timings) : new List<long>();
    }

    /// <summary>
    /// Reset all performance metrics
    /// </summary>
    public void Reset()
    {
        _metrics.Clear();
        _timings.Clear();
        _logger.LogInformation("Performance metrics have been reset");
    }

    /// <summary>
    /// Report performance metrics to logs
    /// </summary>
    private void ReportMetrics(object? state)
    {
        try
        {
            var summary = GetPerformanceSummary();
            
            if (summary.TotalOperations == 0)
                return;

            _logger.LogInformation("Performance Summary: {TotalOps} operations, {AvgTime:F1}ms avg, {SuccessRate:P1} success rate",
                summary.TotalOperations, summary.AverageOperationTimeMs, summary.SuccessRate);

            // Log slow operations
            if (summary.SlowOperations.Count > 0)
            {
                _logger.LogWarning("Top slow operations:");
                foreach (var op in summary.SlowOperations)
                {
                    _logger.LogWarning("  {Operation}: {AvgTime:F1}ms avg ({Calls} calls)",
                        op.OperationName, op.AverageElapsedMs, op.TotalCalls);
                }
            }

            // Log most frequent operations
            _logger.LogInformation("Most frequent operations:");
            foreach (var op in summary.MostFrequentOperations.Take(3))
            {
                _logger.LogInformation("  {Operation}: {Calls} calls, {AvgTime:F1}ms avg",
                    op.OperationName, op.TotalCalls, op.AverageElapsedMs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting performance metrics");
        }
    }

    public void Dispose()
    {
        _metricsReportingTimer?.Dispose();
    }
}

/// <summary>
/// Interface for performance monitoring service
/// </summary>
public interface IPerformanceMonitoringService : IDisposable
{
    IDisposable TrackOperation(string operationName, string? category = null);
    void RecordOperation(string operationName, long elapsedMs, string? category = null, bool success = true);
    void RecordMetric(string metricName, double value, string? category = null);
    PerformanceMetric? GetMetric(string operationName, string? category = null);
    IEnumerable<PerformanceMetric> GetAllMetrics(string? category = null);
    PerformanceSummary GetPerformanceSummary();
    List<long> GetTimingTrend(string operationName, string? category = null);
    void Reset();
}

/// <summary>
/// Operation tracker that automatically measures execution time
/// </summary>
public class OperationTracker : IDisposable
{
    private readonly PerformanceMonitoringService _service;
    private readonly string _operationName;
    private readonly string? _category;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public OperationTracker(PerformanceMonitoringService service, string operationName, string? category)
    {
        _service = service;
        _operationName = operationName;
        _category = category;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _service.RecordOperation(_operationName, _stopwatch.ElapsedMilliseconds, _category);
            _disposed = true;
        }
    }
}

/// <summary>
/// Performance metric for a specific operation
/// </summary>
public class PerformanceMetric
{
    public string OperationName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long TotalCalls { get; set; }
    public long SuccessfulCalls { get; set; }
    public long FailedCalls { get; set; }
    public long TotalElapsedMs { get; set; }
    public long MinElapsedMs { get; set; }
    public long MaxElapsedMs { get; set; }
    public DateTime LastCallTime { get; set; }
    
    public double AverageElapsedMs => TotalCalls > 0 ? TotalElapsedMs / (double)TotalCalls : 0;
    public double SuccessRate => TotalCalls > 0 ? SuccessfulCalls / (double)TotalCalls : 0;
    public double FailureRate => TotalCalls > 0 ? FailedCalls / (double)TotalCalls : 0;
}

/// <summary>
/// Overall performance summary
/// </summary>
public class PerformanceSummary
{
    public long TotalOperations { get; set; }
    public long TotalTimeMs { get; set; }
    public double AverageOperationTimeMs { get; set; }
    public double SuccessRate { get; set; }
    public int UniqueOperations { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<PerformanceMetric> SlowOperations { get; set; } = new();
    public List<PerformanceMetric> MostFrequentOperations { get; set; } = new();
}