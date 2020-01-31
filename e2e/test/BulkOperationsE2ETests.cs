// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Shared;

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Devices.E2ETests
{
    public class BulkOperationsE2ETests : IDisposable
    {
        private readonly string DevicePrefix = $"E2E_{nameof(BulkOperationsE2ETests)}_";
        private readonly string ModulePrefix = $"E2E_{nameof(BulkOperationsE2ETests)}_";

        [Fact]
        [IotHub]
        public async Task BulkOperations_UpdateTwins2Device_Ok()
        {
            var tagName = Guid.NewGuid().ToString();
            var tagValue = Guid.NewGuid().ToString();

            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

            using (RegistryManager registryManager = RegistryManager.CreateFromConnectionString(Configuration.IoTHub.ConnectionString))
            {
                Twin twin = await registryManager.GetTwinAsync(testDevice.Id).ConfigureAwait(false);

                twin.Tags = new TwinCollection();
                twin.Tags[tagName] = tagValue;

                var result = await registryManager.UpdateTwins2Async(new List<Twin> { twin }, true).ConfigureAwait(false);
                Assert.True(result.IsSuccessful, $"UpdateTwins2Async error:\n{ResultErrorsToString(result)}");

                Twin twinUpd = await registryManager.GetTwinAsync(testDevice.Id).ConfigureAwait(false);

                Assert.Equal(twin.DeviceId, twinUpd.DeviceId);
                Assert.NotNull(twinUpd.Tags);
                Assert.True(twinUpd.Tags.Contains(tagName), "Twin doesn't contain the tag");
                Assert.Equal((string)twin.Tags[tagName], (string)twinUpd.Tags[tagName]);

                await registryManager.CloseAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        [IotHub]
        public async Task BulkOperations_UpdateTwins2DevicePatch_Ok()
        {
            var tagName = Guid.NewGuid().ToString();
            var tagValue = Guid.NewGuid().ToString();

            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

            using (RegistryManager registryManager = RegistryManager.CreateFromConnectionString(Configuration.IoTHub.ConnectionString))
            {
                Twin twin = new Twin();
                twin.DeviceId = testDevice.Id;
                twin.Tags = new TwinCollection();
                twin.Tags[tagName] = tagValue;

                var result = await registryManager.UpdateTwins2Async(new List<Twin> { twin }, true).ConfigureAwait(false);
                Assert.True(result.IsSuccessful, $"UpdateTwins2Async error:\n{ResultErrorsToString(result)}");

                Twin twinUpd = await registryManager.GetTwinAsync(testDevice.Id).ConfigureAwait(false);

                Assert.Equal(twin.DeviceId, twinUpd.DeviceId);
                Assert.NotNull(twinUpd.Tags);
                Assert.True(twinUpd.Tags.Contains(tagName), "Twin doesn't contain the tag");
                Assert.Equal((string)twin.Tags[tagName], (string)twinUpd.Tags[tagName]);

                await registryManager.CloseAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        [IotHub]
        public async Task BulkOperations_UpdateTwins2Module_Ok()
        {
            var tagName = Guid.NewGuid().ToString();
            var tagValue = Guid.NewGuid().ToString();

            TestModule testModule = await TestModule.GetTestModuleAsync(DevicePrefix, ModulePrefix).ConfigureAwait(false);

            using (RegistryManager registryManager = RegistryManager.CreateFromConnectionString(Configuration.IoTHub.ConnectionString))
            {
                Twin twin = await registryManager.GetTwinAsync(testModule.DeviceId, testModule.Id).ConfigureAwait(false);

                twin.Tags = new TwinCollection();
                twin.Tags[tagName] = tagValue;

                var result = await registryManager.UpdateTwins2Async(new List<Twin> { twin }, true).ConfigureAwait(false);
                Assert.True(result.IsSuccessful, $"UpdateTwins2Async error:\n{ResultErrorsToString(result)}");

                Twin twinUpd = await registryManager.GetTwinAsync(testModule.DeviceId, testModule.Id).ConfigureAwait(false);

                Assert.Equal(twin.DeviceId, twinUpd.DeviceId);
                Assert.Equal(twin.ModuleId, twinUpd.ModuleId);
                Assert.NotNull(twinUpd.Tags);
                Assert.True(twinUpd.Tags.Contains(tagName), "Twin doesn't contain the tag");
                Assert.Equal((string)twin.Tags[tagName], (string)twinUpd.Tags[tagName]);

                await registryManager.CloseAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        [IotHub]
        public async Task BulkOperations_UpdateTwins2ModulePatch_Ok()
        {
            var tagName = Guid.NewGuid().ToString();
            var tagValue = Guid.NewGuid().ToString();

            TestModule testModule = await TestModule.GetTestModuleAsync(DevicePrefix, ModulePrefix).ConfigureAwait(false);

            using (RegistryManager registryManager = RegistryManager.CreateFromConnectionString(Configuration.IoTHub.ConnectionString))
            {
                Twin twin = new Twin();
                twin.DeviceId = testModule.DeviceId;
                twin.ModuleId = testModule.Id;
                twin.Tags = new TwinCollection();
                twin.Tags[tagName] = tagValue;

                var result = await registryManager.UpdateTwins2Async(new List<Twin> { twin }, true).ConfigureAwait(false);
                Assert.True(result.IsSuccessful, $"UpdateTwins2Async error:\n{ResultErrorsToString(result)}");

                Twin twinUpd = await registryManager.GetTwinAsync(testModule.DeviceId, testModule.Id).ConfigureAwait(false);

                Assert.Equal(twin.DeviceId, twinUpd.DeviceId);
                Assert.Equal(twin.ModuleId, twinUpd.ModuleId);
                Assert.NotNull(twinUpd.Tags);
                Assert.True(twinUpd.Tags.Contains(tagName), "Twin doesn't contain the tag");
                Assert.Equal((string)twin.Tags[tagName], (string)twinUpd.Tags[tagName]);

                await registryManager.CloseAsync().ConfigureAwait(false);
            }
        }

        private string ResultErrorsToString(BulkRegistryOperationResult result)
        {
            var errorString = "";
            
            foreach (var error in result.Errors)
            {
                errorString += $"\t{error.ErrorCode} : {error.ErrorStatus}\n";
            }

            return errorString;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
