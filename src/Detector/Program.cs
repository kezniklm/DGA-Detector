using Detector.Arguments;
using Detector.Components;
using Detector.Components.NetworkAnalyser;

Arguments arguments = new(args);
arguments.Parse();
Data data = new();
CancellationTokenSource cancellationToken = new();
DnsNetworkAnalyser dnsNetworkAnalyser = new(data, arguments.Device, cancellationToken);

Task.WaitAll(Task.Run(() => dnsNetworkAnalyser.SetCapture()));
