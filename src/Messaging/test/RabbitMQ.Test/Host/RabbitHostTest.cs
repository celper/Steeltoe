﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Steeltoe.Common.Lifecycle;
using Steeltoe.Messaging.RabbitMQ.Config;
using System.Collections.Generic;
using Xunit;

namespace Steeltoe.Messaging.RabbitMQ.Host
{
    public class RabbitHostTest
    {
        [Fact]
        public void HostCanBeStarted()
        {
            MockRabbitHostedService hostedService;

            using (var host = RabbitHost.CreateDefaultBuilder()
                                .ConfigureServices(svc => svc.AddSingleton<IHostedService, MockRabbitHostedService>())
                                .Start())
            {
                Assert.NotNull(host);
                hostedService = (MockRabbitHostedService)host.Services.GetRequiredService<IHostedService>();
                Assert.NotNull(hostedService);
                Assert.Equal(1, hostedService.StartCount);
                Assert.Equal(0, hostedService.StopCount);
                Assert.Equal(0, hostedService.DisposeCount);
            }

            Assert.Equal(1, hostedService.StartCount);
            Assert.Equal(0, hostedService.StopCount);
            Assert.Equal(1, hostedService.DisposeCount);
        }

        [Fact]
        public void HostShouldInitializeServices()
        {
            using (var host = RabbitHost.CreateDefaultBuilder().Start())
            {
                var lifecycleProcessor = host.Services.GetRequiredService<ILifecycleProcessor>();
                var rabbitHostService = (RabbitHostService)host.Services.GetRequiredService<IHostedService>();

                Assert.True(lifecycleProcessor.IsRunning);
                Assert.NotNull(rabbitHostService);
            }
        }

        [Fact]
        public void HostShouldAddRabbitOptionsConfiguration()
        {
            var hostBuilder = RabbitHost.CreateDefaultBuilder();

            var appSettings = new Dictionary<string, string>()
            {
                [$"{RabbitOptions.PREFIX}:host"] = "ThisIsATest",
                [$"{RabbitOptions.PREFIX}:port"] = "1234",
                [$"{RabbitOptions.PREFIX}:username"] = "TestUser",
                [$"{RabbitOptions.PREFIX}:password"] = "TestPassword",
            };

            hostBuilder.ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder.AddInMemoryCollection(appSettings);
            });

            using (var host = hostBuilder.Start())
            {
                var rabbitOptions = host.Services.GetService<IOptions<RabbitOptions>>()?.Value;

                Assert.NotNull(rabbitOptions);
                Assert.Equal("ThisIsATest", rabbitOptions.Host);
                Assert.Equal(1234, rabbitOptions.Port);
                Assert.Equal("TestUser", rabbitOptions.Username);
                Assert.Equal("TestPassword", rabbitOptions.Password);
            }
        }
    }
}
