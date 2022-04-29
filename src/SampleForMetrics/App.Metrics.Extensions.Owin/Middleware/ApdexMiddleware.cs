﻿// Copyright (c) Allan Hardy. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Metrics.Apdex;
using App.Metrics.Extensions.Owin.DependencyInjection.Options;
using App.Metrics.Extensions.Owin.Internal;
using Microsoft.Extensions.Logging;

namespace App.Metrics.Extensions.Owin.Middleware
{
    public class ApdexMiddleware : AppMetricsMiddleware<OwinMetricsOptions>
    {
        private const string ApdexItemsKey = "__App.Metrics.Apdex__";
        private readonly IApdex _apdexTracking;

        public ApdexMiddleware(
            OwinMetricsOptions owinOptions,
            ILoggerFactory loggerFactory,
            IMetrics metrics)
            : base(owinOptions, loggerFactory, metrics)
        {
            _apdexTracking = Metrics.Provider.Apdex.Instance(OwinMetricsRegistry.Contexts.HttpRequests.ApdexScores.Apdex(owinOptions.ApdexTSeconds));
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment) && Options.ApdexTrackingEnabled)
            {
                Logger.MiddlewareExecuting(GetType());

                environment[ApdexItemsKey] = _apdexTracking.NewContext();

                await Next(environment);

                var apdex = environment[ApdexItemsKey];
                using (apdex as IDisposable)
                {
                }
                environment.Remove(ApdexItemsKey);

                Logger.MiddlewareExecuted(GetType());
            }
            else
            {
                await Next(environment);
            }
        }
    }
}