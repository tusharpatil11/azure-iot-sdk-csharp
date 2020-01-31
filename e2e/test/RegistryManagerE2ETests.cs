// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Shared;
using System;
using System.Diagnostics.Tracing;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Devices.E2ETests
{
    public class RegistryManagerE2ETests
    {
        private readonly string DevicePrefix = $"E2E_{nameof(RegistryManagerE2ETests)}_";
        private readonly ConsoleEventListener _listener;

        public RegistryManagerE2ETests()
        {
            _listener = TestConfig.StartEventListener();
        }

        [Fact]
        [IotHub]
        public async Task RegistryManager_AddAndRemoveDeviceWithScope()
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(Configuration.IoTHub.ConnectionString);

            string deviceId = DevicePrefix + Guid.NewGuid();

            var edgeDevice = new Device(deviceId)
            {
                Capabilities = new DeviceCapabilities { IotEdge = true }
            };
            edgeDevice = await registryManager.AddDeviceAsync(edgeDevice).ConfigureAwait(false);

            var leafDevice = new Device(Guid.NewGuid().ToString()) { Scope = edgeDevice.Scope };
            Device receivedDevice = await registryManager.AddDeviceAsync(leafDevice).ConfigureAwait(false);

            Assert.NotNull(receivedDevice);
            Assert.Equal(leafDevice.Id, receivedDevice.Id);
            Assert.Equal(leafDevice.Scope, receivedDevice.Scope);
            await registryManager.RemoveDeviceAsync(leafDevice.Id).ConfigureAwait(false);
            await registryManager.RemoveDeviceAsync(edgeDevice.Id).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task RegistryManager_AddDeviceWithTwinWithDeviceCapabilities()
        {
            string deviceId = DevicePrefix + Guid.NewGuid();

            using (RegistryManager registryManager = RegistryManager.CreateFromConnectionString(Configuration.IoTHub.ConnectionString))
            {
                var twin = new Twin
                {
                    Tags = new TwinCollection(@"{ companyId: 1234 }"),
                };

                var iotEdgeDevice = new Device(deviceId)
                {
                    Capabilities = new DeviceCapabilities { IotEdge = true }
                };

                await registryManager.AddDeviceWithTwinAsync(iotEdgeDevice, twin).ConfigureAwait(false);

                Device actual = await registryManager.GetDeviceAsync(deviceId).ConfigureAwait(false);
                await registryManager.RemoveDeviceAsync(deviceId).ConfigureAwait(false);

                Assert.True(actual.Capabilities != null && actual.Capabilities.IotEdge);
            }
        }

        [Fact]
        [IotHub]
        public async Task RegistryManager_AddDeviceWithProxy()
        {
            string deviceId = DevicePrefix + Guid.NewGuid();
            HttpTransportSettings transportSettings = new HttpTransportSettings
            {
                Proxy = new WebProxy(Configuration.IoTHub.ProxyServerAddress)
            };
            using (RegistryManager registryManager = RegistryManager.CreateFromConnectionString(Configuration.IoTHub.ConnectionString, transportSettings))
            {
                Device device = new Device(deviceId);
                await registryManager.AddDeviceAsync(device).ConfigureAwait(false);
            }
        }
    }
}
