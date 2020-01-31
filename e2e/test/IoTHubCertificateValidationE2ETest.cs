// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using System;
using System.Diagnostics.Tracing;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Devices.E2ETests
{
    public class IoTHubCertificateValidationE2ETest : IDisposable
    {
        private readonly TestLogging _log = TestLogging.GetInstance();
        private readonly ConsoleEventListener _listener;

        public IoTHubCertificateValidationE2ETest()
        {
            _listener = TestConfig.StartEventListener();
        }

        [Fact]
        [IotHub]
        public async Task RegistryManager_QueryDevicesInvalidServiceCertificateHttp_Fails()
        {
            var rm = RegistryManager.CreateFromConnectionString(Configuration.IoTHub.ConnectionStringInvalidServiceCertificate);
            IQuery query = rm.CreateQuery("select * from devices");
            var exception = await Assert.ThrowsAsync<IotHubCommunicationException>(
                () => query.GetNextAsTwinAsync()).ConfigureAwait(false);

#if NET451 || NET47
            Assert.Equal(typeof(AuthenticationException), exception.InnerException.InnerException.InnerException.GetType());
#else
            Assert.Equal(typeof(AuthenticationException), exception.InnerException.InnerException.GetType());
#endif
        }

        [Fact]
        [IotHub]
        public async Task ServiceClient_SendMessageToDeviceInvalidServiceCertificateAmqpTcp_Fails()
        {
            var transport = TransportType.Amqp;
            await Assert.ThrowsAsync<AuthenticationException>(
                () => TestServiceClientInvalidServiceCertificate(transport)).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task ServiceClient_SendMessageToDeviceInvalidServiceCertificateAmqpWs_Fails()
        {
            var transport = TransportType.Amqp_WebSocket_Only;
            var exception = await Assert.ThrowsAsync<WebSocketException>(
                () => TestServiceClientInvalidServiceCertificate(transport)).ConfigureAwait(false);

            Assert.Equal(typeof(AuthenticationException), exception.InnerException.InnerException.GetType());
        }

        private static async Task TestServiceClientInvalidServiceCertificate(TransportType transport)
        {
            var service = ServiceClient.CreateFromConnectionString(
                Configuration.IoTHub.ConnectionStringInvalidServiceCertificate,
                transport);
            await service.SendAsync("testDevice1", new Message()).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task JobClient_ScheduleTwinUpdateInvalidServiceCertificateHttp_Fails()
        {
            var job = JobClient.CreateFromConnectionString(Configuration.IoTHub.ConnectionStringInvalidServiceCertificate);
            var exception = await Assert.ThrowsAsync<IotHubCommunicationException>(
                () => job.ScheduleTwinUpdateAsync(
                    "testDevice",
                    "DeviceId IN ['testDevice']",
                    new Shared.Twin(),
                    DateTime.UtcNow,
                    60)).ConfigureAwait(false);

#if NET451 || NET47
            Assert.Equal(typeof(AuthenticationException), exception.InnerException.InnerException.InnerException.GetType());
#else
            Assert.Equal(typeof(AuthenticationException), exception.InnerException.InnerException.GetType());
#endif
        }

        [Fact]
        [IotHub]
        public async Task DeviceClient_SendAsyncInvalidServiceCertificateAmqpTcp_Fails()
        {
            var transport = Client.TransportType.Amqp_Tcp_Only;
            await Assert.ThrowsAsync<AuthenticationException>(
                () => TestDeviceClientInvalidServiceCertificate(transport)).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task DeviceClient_SendAsyncInvalidServiceCertificateMqttTcp_Fails()
        {
            var transport = Client.TransportType.Mqtt_Tcp_Only;
            await Assert.ThrowsAsync<AuthenticationException>(
                () => TestDeviceClientInvalidServiceCertificate(transport)).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task DeviceClient_SendAsyncInvalidServiceCertificateHttp_Fails()
        {
            var transport = Client.TransportType.Http1;
            var exception = await Assert.ThrowsAsync<AuthenticationException>(
                () => TestDeviceClientInvalidServiceCertificate(transport)).ConfigureAwait(false);

#if NET451 || NET47
            Assert.Equal(typeof(AuthenticationException), exception.InnerException.InnerException.InnerException.GetType());
#else
            Assert.Equal(typeof(AuthenticationException), exception.InnerException.InnerException.GetType());
#endif
        }

        [Fact]
        [IotHub]
        public async Task DeviceClient_SendAsyncInvalidServiceCertificateAmqpWs_Fails()
        {
            var transport = Client.TransportType.Amqp_WebSocket_Only;
            var exception = await Assert.ThrowsAsync<AuthenticationException>(
                () => TestDeviceClientInvalidServiceCertificate(transport)).ConfigureAwait(false);

            Assert.Equal(typeof(AuthenticationException), exception.GetType());
        }

        [Fact]
        [IotHub]
        public async Task DeviceClient_SendAsyncInvalidServiceCertificateMqttWs_Fails()
        {
            var transport = Client.TransportType.Mqtt_WebSocket_Only;
            var exception = await Assert.ThrowsAsync<AuthenticationException>(
                () => TestDeviceClientInvalidServiceCertificate(transport)).ConfigureAwait(false);

            Assert.Equal(typeof(AuthenticationException), exception.GetType());
        }

        private static async Task TestDeviceClientInvalidServiceCertificate(Client.TransportType transport)
        {
            using (DeviceClient deviceClient = 
                DeviceClient.CreateFromConnectionString(
                    Configuration.IoTHub.DeviceConnectionStringInvalidServiceCertificate, 
                    transport))
            {
                await deviceClient.SendEventAsync(new Client.Message()).ConfigureAwait(false);
                await deviceClient.CloseAsync().ConfigureAwait(false);
            }
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
