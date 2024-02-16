using PacketDotNet;
using SharpPcap;

namespace Detector.Components.NetworkAnalyser;

internal class DnsNetworkAnalyser(Data data, ICaptureDevice device, CancellationTokenSource cancellationToken)
{
    private const string Filter = "udp port 53";

    private Data Data { get; } = data ?? throw new ArgumentNullException(nameof(data));

    private ICaptureDevice Device { get; } = device ?? throw new ArgumentNullException(nameof(device));

    private CancellationTokenSource CancellationToken { get; } =
        cancellationToken ?? throw new ArgumentNullException(nameof(cancellationToken));

    internal void SetCapture()
    {
        Device.Open(DeviceModes.Promiscuous);
        Device.Filter = Filter;
        Device.OnPacketArrival += PacketHandler;

        CaptureLoop();
    }

    private void CaptureLoop()
    {
        try
        {
            Device.StartCapture();
            while (!cancellationToken.IsCancellationRequested)
            {
            }
        }
        catch (PcapException ex)
        {
            Console.WriteLine($"PcapException in capture loop: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in capture loop: {ex.Message}");
        }

        Device.StopCapture();
    }

    private void PacketHandler(object sender, PacketCapture packetCapture)
    {
        Packet? packet = Packet.ParsePacket(packetCapture.GetPacket().LinkLayerType, packetCapture.Data.ToArray());
        if (packet != null)
        {
            Data.PacketFilterQueue.Enqueue(packet);
            Console.WriteLine(packet.HeaderData.Length);
        }
    }
}
