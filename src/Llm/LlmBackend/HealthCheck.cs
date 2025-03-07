﻿using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LlmBackend
{
    public sealed class HealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // All is well!
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
