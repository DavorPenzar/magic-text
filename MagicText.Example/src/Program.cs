using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using Serilog.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MagicText.Example
{
    public static class Program
    {
        public const int ExitSuccess = 0;
        public const int ExitFailure = -1;

        private static IConfiguration configuration;

        public static IConfiguration Configuration
        {
            get => configuration;
            private set
            {
                configuration = value;
            }
        }

        static Program()
        {
            configuration = null!;
        }

        private static void InitialiseEnvironment(String[]? args = null)
        {
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.SetBasePath(AppContext.BaseDirectory);
                configurationBuilder.AddEnvironmentVariables();
                configurationBuilder.AddJsonFile("appsettings.json");
                configurationBuilder.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true);
                if (!(args is null))
                {
                    configurationBuilder.AddCommandLine(args);
                }

                Configuration = configurationBuilder.Build();
            }

            {
                SelfLog.Enable(Console.Error);

                LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
                loggerConfiguration.ReadFrom.Configuration(Configuration);

                Log.Logger = loggerConfiguration.CreateLogger();

                SelfLog.Disable();
            }
        }

        private static void FinaliseEnvironment()
        {
            Log.CloseAndFlush();
        }

        public static async Task<Int32> Main(String[] args)
        {
            Console.WriteLine("Press Enter to start...");
            Console.ReadLine();

            InitialiseEnvironment(args);

            using ILoggerProvider loggerProvider = new SerilogLoggerProvider(null, true);
            using ILoggerFactory loggerFactory = new SerilogLoggerFactory(null, true, null);

            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger(typeof(Program).FullName!);

            try
            {

                using HttpClientHandler clientHandler = new HttpClientHandler();
                using HttpClient client = new HttpClient(clientHandler, true)
                {
                    BaseAddress = new Uri(Configuration["Text:WebSource:Authority"])
                };

                ITokeniser tokeniser = new RegexSplitTokeniser(
                    Configuration["Tokeniser:Pattern"],
                    false,
                    Configuration.GetValue<RegexOptions>("Tokeniser:Options", RegexTokeniser.DefaultOptions)
                );

                Pen pen;

                using (
                    TextDownloader textDownloader = new TextDownloader(
                        loggerFactory.CreateLogger<TextDownloader>(),
                        client,
                        tokeniser,
                        Configuration.GetValue<ShatteringOptions>("ShatteringOptions"),
                        false
                    )
                )
                {
                    try
                    {
                        pen = new Pen(
                            context: await textDownloader.DownloadTextAsync(
                                Configuration["Text:WebSource:Query"],
                                Encoding.GetEncoding(Configuration.GetValue<String>("Text:WebSource:Encoding", "UTF-8"))
                            ),
                            comparisonType: (StringComparison)Enum.Parse(
                                typeof(StringComparison),
                                Configuration.GetValue<String>("Pen:ComparisonType", nameof(StringComparison.Ordinal)),
                                true
                            ),
                            intern: Configuration.GetValue<Boolean>("Pen:Intern")
                        );
                    }
                    catch (HttpRequestException exception)
                    {
                        logger.LogError(
                            exception,
                            "Failed to download text ({statusCode:D} {status}).",
                                exception.StatusCode.HasValue ?
                                    Convert.ToInt32(exception.StatusCode.Value) :
                                    Convert.ToInt32(HttpStatusCode.BadRequest),
                                exception.StatusCode.HasValue ?
                                    exception.StatusCode.Value.ToString() :
                                    HttpStatusCode.BadRequest.ToString()
                        );

                        FinaliseEnvironment();

                        return ExitFailure;
                    }
                }

                String text = String.Join(
                    String.Empty,
                    pen.Render(
                        Configuration.GetValue<Int32>("Text:RandomGenerator:RelevantTokens"),
                        new Random(Configuration.GetValue<Int32>("Text:RandomGenerator:Seed")),
                        Configuration.GetValue<Nullable<Int32>>("Text:RandomGenerator:FromPosition")
                    ).Take(Configuration.GetValue<Int32>("Text:RandomGenerator:MaxTokens"))
                );
                logger.LogInformation($"Generated text:{Environment.NewLine}{{text}}", text);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "An error occured while running the program.");
            }
            finally
            {
                FinaliseEnvironment();
            }

            return ExitSuccess;
        }
    }
}
