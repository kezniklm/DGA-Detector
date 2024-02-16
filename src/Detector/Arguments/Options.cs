using CommandLine;

namespace Detector.Arguments;

internal class Options
{
    [Option('i', "interface", Required = true, HelpText = "Name of the network interface.")]
    public string NetworkInterface { get; set; } = string.Empty;
}
