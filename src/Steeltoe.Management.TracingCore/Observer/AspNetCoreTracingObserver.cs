﻿// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Steeltoe.Common.Diagnostics;
using Steeltoe.Management.Census.Trace;
using Steeltoe.Management.Census.Trace.Propagation;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace Steeltoe.Management.Tracing.Observer
{
    public abstract class AspNetCoreTracingObserver : DiagnosticObserver
    {
        private const string DIAGNOSTIC_NAME = "Microsoft.AspNetCore";

        protected internal static void EndSpanIfNeeded(SpanContext previousContext, SpanContext newContext, bool threadContextChanged)
        {
            if (!threadContextChanged)
            {
                return;
            }

            if (previousContext == null)
            {
                return;
            }

            var previous = previousContext.Active;
            if (previous is Span span &&
                newContext == null &&
                !span.HasEnded)
            {
                span.End();
            }
        }

        protected static void HandleValueChangedEvent(AsyncLocalValueChangedArgs<SpanContext> arg)
        {
            EndSpanIfNeeded(arg.PreviousValue, arg.CurrentValue, arg.ThreadContextChanged);
        }

        protected ITracing Tracing { get; }

        protected ITextFormat Propagation { get; }

        protected ITracer Tracer { get; }

        protected ITracingOptions Options { get; }

        protected Regex PathMatcher { get; }

        protected AspNetCoreTracingObserver(string observerName, ITracingOptions options, ITracing tracing, ILogger logger)
            : base(observerName, DIAGNOSTIC_NAME, logger)
        {
            Options = options;
            Tracing = tracing;
            Propagation = tracing.PropagationComponent.TextFormat;
            Tracer = tracing.Tracer;
            PathMatcher = new Regex(options.IngressIgnorePattern);
        }

        protected internal virtual bool ShouldIgnoreRequest(PathString pathString)
        {
            string path = pathString.Value;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return PathMatcher.IsMatch(path);
        }

        protected internal virtual string GetExceptionMessage(Exception exception)
        {
            return exception.GetType().Name + " : " + exception.Message;
        }

        protected internal virtual string GetExceptionStackTrace(Exception exception)
        {
            if (exception.StackTrace != null)
            {
                return exception.StackTrace.ToString();
            }

            return string.Empty;
        }

        public class SpanContext
        {
            public SpanContext(ISpan active, ISpan previous)
            {
                Active = active;
                Previous = previous;
            }

            public ISpan Active { get; }

            public ISpan Previous { get; }
        }
    }
}
