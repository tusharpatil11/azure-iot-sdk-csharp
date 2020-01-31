// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Client;
using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Devices.E2ETests
{
    public class MethodE2ETests : IDisposable
    {
        public const string DeviceResponseJson = "{\"name\":\"e2e_test\"}";
        public const string ServiceRequestJson = "{\"a\":123}";

        private readonly string DevicePrefix = $"E2E_{nameof(MethodE2ETests)}_";
        private const string MethodName = "MethodE2ETest";
        private static TestLogging _log = TestLogging.GetInstance();

        private static TimeSpan DefaultMethodTimeoutMinutes = TimeSpan.FromMinutes(1);

        private readonly ConsoleEventListener _listener;

        public MethodE2ETests()
        {
            _listener = TestConfig.StartEventListener();
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponse_Mqtt()
        {
            await SendMethodAndRespond(Client.TransportType.Mqtt_Tcp_Only, SetDeviceReceiveMethod).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponse_MqttWs()
        {
            await SendMethodAndRespond(Client.TransportType.Mqtt_WebSocket_Only, SetDeviceReceiveMethod).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponseWithObseletedSetMethodHandler_Mqtt()
        {
            await SendMethodAndRespond(Client.TransportType.Mqtt_Tcp_Only, SetDeviceReceiveMethodObsoleteHandler).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponseWithObseletedSetMethodHandler_MqttWs()
        {
            await SendMethodAndRespond(Client.TransportType.Mqtt_WebSocket_Only, SetDeviceReceiveMethodObsoleteHandler).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponseWithDefaultMethodHandler_Mqtt()
        {
            await SendMethodAndRespond(Client.TransportType.Mqtt_Tcp_Only, SetDeviceReceiveMethodDefaultHandler).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponseWithDefaultMethodHandler_MqttWs()
        {
            await SendMethodAndRespond(Client.TransportType.Mqtt_WebSocket_Only, SetDeviceReceiveMethodDefaultHandler).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponse_Amqp()
        {
            await SendMethodAndRespond(Client.TransportType.Amqp_Tcp_Only, SetDeviceReceiveMethod).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponse_AmqpWs()
        {
            await SendMethodAndRespond(Client.TransportType.Amqp_WebSocket_Only, SetDeviceReceiveMethod).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponseWithObseletedSetMethodHandler_Amqp()
        {
            await SendMethodAndRespond(Client.TransportType.Amqp_Tcp_Only, SetDeviceReceiveMethodObsoleteHandler).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponseWithObseletedSetMethodHandler_AmqpWs()
        {
            await SendMethodAndRespond(Client.TransportType.Amqp_WebSocket_Only, SetDeviceReceiveMethodObsoleteHandler).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponseWithDefaultMethodHandler_Amqp()
        {
            await SendMethodAndRespond(Client.TransportType.Amqp_Tcp_Only, SetDeviceReceiveMethodDefaultHandler).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_DeviceReceivesMethodAndResponseWithDefaultMethodHandler_AmqpWs()
        {
            await SendMethodAndRespond(Client.TransportType.Amqp_WebSocket_Only, SetDeviceReceiveMethodDefaultHandler).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_ServiceSendsMethodThroughProxyWithDefaultTimeout()
        {
            ServiceClientTransportSettings serviceClientTransportSettings = new ServiceClientTransportSettings
            {
                HttpProxy = new WebProxy(Configuration.IoTHub.ProxyServerAddress)
            };

            await SendMethodAndRespond(Client.TransportType.Mqtt_Tcp_Only, SetDeviceReceiveMethod, serviceClientTransportSettings).ConfigureAwait(false);
        }

        [Fact]
        [IotHub]
        public async Task Method_ServiceSendsMethodThroughProxyWithCustomTimeout()
        {
            ServiceClientTransportSettings serviceClientTransportSettings = new ServiceClientTransportSettings
            {
                HttpProxy = new WebProxy(Configuration.IoTHub.ProxyServerAddress)
            };

            await SendMethodAndRespond(Client.TransportType.Mqtt_Tcp_Only, SetDeviceReceiveMethod, TimeSpan.FromMinutes(5), serviceClientTransportSettings).ConfigureAwait(false);
        }

        public static async Task ServiceSendMethodAndVerifyResponse(string deviceName, string methodName, string respJson, string reqJson)
        {
            using (ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(Configuration.IoTHub.ConnectionString))
            {
                _log.WriteLine($"{nameof(ServiceSendMethodAndVerifyResponse)}: Invoke method {methodName}.");
                CloudToDeviceMethodResult response =
                    await serviceClient.InvokeDeviceMethodAsync(
                        deviceName,
                        new CloudToDeviceMethod(methodName, DefaultMethodTimeoutMinutes).SetPayloadJson(reqJson)).ConfigureAwait(false);

                _log.WriteLine($"{nameof(ServiceSendMethodAndVerifyResponse)}: Method status: {response.Status}.");
                Assert.Equal(200, response.Status);
                string payload = response.GetPayloadAsJson();
                Assert.Equal(respJson, payload);

                await serviceClient.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task ServiceSendMethodAndVerifyResponse(string deviceName, string methodName, string respJson, string reqJson, TimeSpan responseTimeout, ServiceClientTransportSettings serviceClientTransportSettings)
        {
            using (ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(Configuration.IoTHub.ConnectionString, TransportType.Amqp, serviceClientTransportSettings))
            {
                _log.WriteLine($"{nameof(ServiceSendMethodAndVerifyResponse)}: Invoke method {methodName}.");
                CloudToDeviceMethodResult response =
                    await serviceClient.InvokeDeviceMethodAsync(
                        deviceName,
                        new CloudToDeviceMethod(methodName, responseTimeout).SetPayloadJson(reqJson)).ConfigureAwait(false);

                _log.WriteLine($"{nameof(ServiceSendMethodAndVerifyResponse)}: Method status: {response.Status}.");
                Assert.Equal(200, response.Status);
                string payload = response.GetPayloadAsJson();
                Assert.Equal(respJson, payload);

                await serviceClient.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<Task> SetDeviceReceiveMethod(DeviceClient deviceClient, string methodName)
        {
            var methodCallReceived = new TaskCompletionSource<bool>();

            await deviceClient.SetMethodHandlerAsync(methodName,
                (request, context) =>
                {
                    _log.WriteLine($"{nameof(SetDeviceReceiveMethod)}: DeviceClient method: {request.Name} {request.ResponseTimeout}.");

                    try
                    {
                        Assert.Equal(methodName, request.Name);
                        Assert.Equal(ServiceRequestJson, request.DataAsJson);

                        methodCallReceived.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        methodCallReceived.SetException(ex);
                    }

                    return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(DeviceResponseJson), 200));
                },
                null).ConfigureAwait(false);

            // Return the task that tells us we have received the callback.
            return methodCallReceived.Task;
        }

        public static async Task<Task> SetDeviceReceiveMethodDefaultHandler(DeviceClient deviceClient, string methodName)
        {
            var methodCallReceived = new TaskCompletionSource<bool>();

            await deviceClient.SetMethodDefaultHandlerAsync(
                (request, context) =>
                {
                    _log.WriteLine($"{nameof(SetDeviceReceiveMethodDefaultHandler)}: DeviceClient method: {request.Name} {request.ResponseTimeout}.");

                    try
                    {
                        Assert.Equal(methodName, request.Name);
                        Assert.Equal(ServiceRequestJson, request.DataAsJson);

                        methodCallReceived.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        methodCallReceived.SetException(ex);
                    }

                    return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(DeviceResponseJson), 200));
                },
                null).ConfigureAwait(false);

            return methodCallReceived.Task;
        }

        private Task<Task> SetDeviceReceiveMethodObsoleteHandler(DeviceClient deviceClient, string methodName)
        {
            var methodCallReceived = new TaskCompletionSource<bool>();

#pragma warning disable CS0618
            deviceClient.SetMethodHandler(methodName, (request, context) =>
            {
                _log.WriteLine($"{nameof(SetDeviceReceiveMethodObsoleteHandler)}: DeviceClient method: {request.Name} {request.ResponseTimeout}.");

                try
                {
                    Assert.Equal(methodName, request.Name);
                    Assert.Equal(ServiceRequestJson, request.DataAsJson);

                    methodCallReceived.SetResult(true);
                }
                catch (Exception ex)
                {
                    methodCallReceived.SetException(ex);
                }

                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(DeviceResponseJson), 200));
            }, null);
#pragma warning restore CS0618

            return Task.FromResult<Task>(methodCallReceived.Task);
        }

        private async Task SendMethodAndRespond(Client.TransportType transport, Func<DeviceClient, string, Task<Task>> setDeviceReceiveMethod)
        {
            await SendMethodAndRespond(transport, setDeviceReceiveMethod, new ServiceClientTransportSettings()).ConfigureAwait(false);
        }

        private async Task SendMethodAndRespond(Client.TransportType transport, Func<DeviceClient, string, Task<Task>> setDeviceReceiveMethod, ServiceClientTransportSettings serviceClientTransportSettings)
        {
            await SendMethodAndRespond(transport, setDeviceReceiveMethod, DefaultMethodTimeoutMinutes, serviceClientTransportSettings).ConfigureAwait(false);
        }

        private async Task SendMethodAndRespond(Client.TransportType transport, Func<DeviceClient, string, Task<Task>> setDeviceReceiveMethod, TimeSpan responseTimeout, ServiceClientTransportSettings serviceClientTransportSettings)
        {
            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix).ConfigureAwait(false);

            using (DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(testDevice.ConnectionString, transport))
            {
                Task methodReceivedTask = await setDeviceReceiveMethod(deviceClient, MethodName).ConfigureAwait(false);

                await Task.WhenAll(
                    ServiceSendMethodAndVerifyResponse(testDevice.Id, MethodName, DeviceResponseJson, ServiceRequestJson, responseTimeout, serviceClientTransportSettings),
                    methodReceivedTask).ConfigureAwait(false);

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
