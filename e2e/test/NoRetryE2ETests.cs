using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Devices.E2ETests
{
    public class NoRetryE2ETests
    {
        private readonly string DevicePrefix = $"E2E_{nameof(NoRetryE2ETests)}_";
        private static TestLogging _log = TestLogging.GetInstance();
        private readonly ConsoleEventListener _listener;

        public NoRetryE2ETests()
        {
            _listener = TestConfig.StartEventListener();
        }

        [Fact]
        [IotHub]
        public async Task FaultInjection_NoRecovery()
        {
            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix, TestDeviceType.Sasl).ConfigureAwait(false);

            using (DeviceClient deviceClient = testDevice.CreateDeviceClient(Client.TransportType.Amqp_Tcp_Only))
            {
                _log.WriteLine($"{nameof(FaultInjection_NoRecovery)}: deviceId={testDevice.Id}");
                deviceClient.SetRetryPolicy(new NoRetry());

                ConnectionStatus? lastConnectionStatus = null;
                Dictionary<ConnectionStatus, int> connectionStatusChanges = new Dictionary<ConnectionStatus, int>();
                deviceClient.SetConnectionStatusChangesHandler((status, reason) =>
                {
                    connectionStatusChanges.TryGetValue(status, out int count);
                    count++;
                    connectionStatusChanges[status] = count;
                    lastConnectionStatus = status;
                });

                _log.WriteLine($"{nameof(FaultInjection_NoRecovery)}: calling OpenAsync...");
                await deviceClient.OpenAsync().ConfigureAwait(false);

                _log.WriteLine($"{nameof(FaultInjection_NoRecovery)}: injecting fault {FaultInjection.FaultType_Tcp}...");
                await FaultInjection.ActivateFaultInjection(
                    Client.TransportType.Amqp_Tcp_Only, 
                    FaultInjection.FaultType_Tcp,
                    FaultInjection.FaultCloseReason_Boom,
                    FaultInjection.DefaultDelayInSec, 
                    FaultInjection.DefaultDurationInSec, 
                    deviceClient
                ).ConfigureAwait(false);

                await Task.Delay(FaultInjection.DefaultDelayInSec).ConfigureAwait(false);

                _log.WriteLine($"{nameof(FaultInjection_NoRecovery)}: waiting fault injection occurs...");
                for (int i = 0; i < FaultInjection.LatencyTimeBufferInSec; i++)
                {
                    if (connectionStatusChanges.ContainsKey(ConnectionStatus.Disconnected))
                    {
                        break;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                }

                Assert.Equal(ConnectionStatus.Disconnected, lastConnectionStatus);
                Assert.False(connectionStatusChanges.ContainsKey(ConnectionStatus.Disconnected_Retrying), $"Shouldn't get {ConnectionStatus.Disconnected_Retrying} status change.");
                int connected = connectionStatusChanges[ConnectionStatus.Connected];
                Assert.Equal(1, connected);
                int disconnected = connectionStatusChanges[ConnectionStatus.Disconnected];
                Assert.Equal(1, disconnected);
            }
        }

        [Fact]
        [IotHub]
        public async Task Duplicated_NoPingpong()
        {
            TestDevice testDevice = await TestDevice.GetTestDeviceAsync(DevicePrefix, TestDeviceType.Sasl).ConfigureAwait(false);

            _log.WriteLine($"{nameof(Duplicated_NoPingpong)}: 2 device client instances with the same deviceId={testDevice.Id}.");

            using (DeviceClient deviceClient1 = testDevice.CreateDeviceClient(Client.TransportType.Amqp_Tcp_Only), deviceClient2 = testDevice.CreateDeviceClient(Client.TransportType.Amqp_Tcp_Only))
            {
                _log.WriteLine($"{nameof(Duplicated_NoPingpong)}: set device client instance 1 to no retry.");
                deviceClient1.SetRetryPolicy(new NoRetry());

                ConnectionStatus? lastConnectionStatus = null;
                Dictionary<ConnectionStatus, int> connectionStatusChanges = new Dictionary<ConnectionStatus, int>();
                deviceClient1.SetConnectionStatusChangesHandler((status, reason) =>
                {
                    connectionStatusChanges.TryGetValue(status, out int count);
                    count++;
                    connectionStatusChanges[status] = count;
                    lastConnectionStatus = status;
                });

                _log.WriteLine($"{nameof(FaultInjection_NoRecovery)}: device client instance 1 calling OpenAsync...");
                await deviceClient1.OpenAsync().ConfigureAwait(false);
                await deviceClient1.SetMethodHandlerAsync(
                    "dummy_method",
                    (methodRequest, userContext) => Task.FromResult(new MethodResponse(200)),
                    deviceClient1
                ).ConfigureAwait(false);

                _log.WriteLine($"{nameof(FaultInjection_NoRecovery)}: device client instance 2 calling OpenAsync...");
                await deviceClient2.OpenAsync().ConfigureAwait(false);
                await deviceClient2.SetMethodHandlerAsync(
                    "dummy_method",
                    (methodRequest, userContext) => Task.FromResult(new MethodResponse(200)),
                    deviceClient2
                ).ConfigureAwait(false);

                _log.WriteLine($"{nameof(Duplicated_NoPingpong)}: waiting device client instance 1 to be kicked off...");
                for (int i = 0; i < FaultInjection.LatencyTimeBufferInSec; i++)
                {
                    if (connectionStatusChanges.ContainsKey(ConnectionStatus.Disconnected))
                    {
                        break;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                }

                Assert.Equal(ConnectionStatus.Disconnected, lastConnectionStatus);
                Assert.False(connectionStatusChanges.ContainsKey(ConnectionStatus.Disconnected_Retrying), $"Shouldn't get {ConnectionStatus.Disconnected_Retrying} status change.");
                int connected = connectionStatusChanges[ConnectionStatus.Connected];
                Assert.Equal(1, connected);
                int disconnected = connectionStatusChanges[ConnectionStatus.Disconnected];
                Assert.Equal(1, disconnected);
            }
        }
    }
}
