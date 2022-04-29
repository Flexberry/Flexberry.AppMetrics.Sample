﻿// Copyright (c) Allan Hardy. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using App.Metrics.Extensions.Owin.DependencyInjection.Options;
using App.Metrics.Extensions.Owin.Extensions;
using App.Metrics.Formatters;
using Microsoft.Extensions.Logging;

namespace App.Metrics.Extensions.Owin.Middleware
{
    public class MetricsEndpointMiddleware : AppMetricsMiddleware<OwinMetricsOptions>
    {
        private const string MetricsMimeType = "application/vnd.app.metrics.v1.metrics+json";
        private readonly IMetricsOutputFormatter _serializer;

        public MetricsEndpointMiddleware(
            OwinMetricsOptions owinOptions,
            ILoggerFactory loggerFactory,
            IMetrics metrics,
            IMetricsOutputFormatter serializer)
            : base(owinOptions, loggerFactory, metrics)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            _serializer = serializer;

        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var requestPath = environment["owin.RequestPath"] as string;

            if (Options.MetricsEndpointEnabled && Options.MetricsEndpoint.IsPresent() && Options.MetricsEndpoint == requestPath)
            {
                Logger.MiddlewareExecuting(GetType());


                var result = string.Empty;
                using (var stream = new MemoryStream())
                {
                    var metricsData = Metrics.Snapshot.Get();
                    await _serializer.WriteAsync(stream, metricsData);

                    result = Encoding.UTF8.GetString(stream.ToArray());
                }

                await WriteResponseAsync(environment, result, MetricsMimeType);

                Logger.MiddlewareExecuted(GetType());

                return;
            }

            await Next(environment);
        }
    }
}