using System.Collections.Concurrent;
using PacketDotNet;

namespace Detector.Components;

internal class Data
{
    internal ConcurrentQueue<Packet> PacketFilterQueue { get; } = new();
}
