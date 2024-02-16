using System.Net.NetworkInformation;
using CommandLine;
using Detector.Exceptions;
using SharpPcap;

namespace Detector.Arguments;

internal class Arguments(string[] args)
{
    internal ILiveDevice Device { get; set; } = null!;

    internal string RabbitMqConnectionString { get; set; } = null!;

    internal void Parse()
    {
        Parser parser = new(with => with.HelpWriter = Console.Out);

        ParserResult<Options>? result = parser.ParseArguments<Options>(args);

        result.WithParsed(options =>
        {
            try
            {
                FindInterface(options.NetworkInterface);
            }
            catch (MatchingCaptureDeviceNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //TODO CheckRabbitMqConnection();
        });

        result.WithNotParsed(errors =>
        {
            Console.WriteLine("Error parsing command-line arguments.");
            Console.WriteLine("Use --help to see available options.");
            //Environment.Exit(errorExitCode);
        });
    }

    private void FindInterface(string interfaceName)
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        NetworkInterface? matchingInterface =
            networkInterfaces.FirstOrDefault(networkInterface => networkInterface.Name == interfaceName);

        CaptureDeviceList? devices = CaptureDeviceList.Instance;


        ILiveDevice? matchingCaptureDevice = devices.FirstOrDefault(device =>
            device.Name == matchingInterface?.Name || device.Name == $"\\Device\\NPF_{matchingInterface?.Id}");

        Device = matchingCaptureDevice ?? throw new MatchingCaptureDeviceNotFoundException();
    }

    private void CheckRabbitMqConnection(string rabbitMqConnectionString) =>
        //TODO
        RabbitMqConnectionString = rabbitMqConnectionString;
}
