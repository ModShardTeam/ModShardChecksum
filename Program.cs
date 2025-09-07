using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Security.Cryptography;
using Serilog;

namespace ModShardChecksum;
internal static class Program
{
    public static void MainCommand(string filename)
    {
        if (Path.GetExtension(filename) != ".win")
        {
            Log.Error(
                "A .win file was expected, got {{{0}}} instead",
                filename
            );
            return;
        }
        using MD5 md5 = MD5.Create();
        using FileStream stream = File.OpenRead(filename);
        string hash = Convert.ToHexString(md5.ComputeHash(stream));

        Log.Information("Hash: {{{0}}}", hash);
    }
    static async Task Main(string[] args)
    {
        // create File and Console (controlledby a switch) sinks
        LoggerConfiguration logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console();

        Log.Logger = logger.CreateLogger();

        Option<string> nameOption = new("--file")
        {
            Description = "Name of the output.",
            IsRequired = true
        };
        nameOption.AddAlias("-f");

        RootCommand rootCommand = new("Computing the checksum of file.")
        {
            nameOption,
        };

        rootCommand.SetHandler(MainCommand, nameOption);

        CommandLineBuilder commandLineBuilder = new(rootCommand);
        commandLineBuilder.AddMiddleware(async (context, next) =>
        {
            await next(context);
        });

        commandLineBuilder.UseDefaults();
        Parser parser = commandLineBuilder.Build();

        await parser.InvokeAsync(args);
    }
}
