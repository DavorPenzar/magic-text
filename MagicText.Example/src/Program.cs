using MagicText.Example.BLL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using Serilog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MagicText.Example
{
    /// <summary>Represents a console application for demonstrating the <a href="http://github.com/DavorPenzar/magic-text"><em>MagicText</em></a> library.</summary>
    public static class Program
    {
        /// <summary>Represents a successful program exit code.</summary>
        public const int ExitSuccess = 0;

        /// <summary>Represents an unsuccessful program exit code.</summary>
        public const int ExitFailure = -1;

        private static IConfiguration configuration;

        /// <summary>Key/value application configuration properties.</summary>
        /// <value>The new configuration properties for the application.</value>
        /// <returns>The configuration properties for the application.</returns>
        /// <remarks>
        ///     <para>To configure the application, edit the <em>appsettings.json</em> file.</para>
        /// </remarks>
        public static IConfiguration Configuration
        {
            get => configuration;
            private set
            {
                configuration = value;
            }
        }

        /// <summary>Initialises static fields.</summary>
        static Program()
        {
            configuration = null!;
        }

        /// <summary>Initialises the application configuration properties.</summary>
        /// <param name="args">Command line arguments (excluding the program's command). If <c>null</c>, it is ignored.</param>
        /// <remarks>
        ///    <para>This method (re)sets the <see cref="Configuration" /> property.</para>
        /// </remarks>
        private static void InitialiseConfiguration(String[]? args = null)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(AppContext.BaseDirectory);
            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.AddJsonFile("appsettings.json");
            configurationBuilder.AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT") ?? "Production"}.json",
                optional: true
            );
            if (!(args is null))
            {
                configurationBuilder.AddCommandLine(args);
            }

            Configuration = configurationBuilder.Build();
        }

        /// <summary>Initialises the globally shared logger.</summary>
        /// <remarks>
        ///    <para>This method (re)sets the <see cref="Log.Logger" /> property.</para>
        /// </remarks>
        private static void InitialiseLogging()
        {
            SelfLog.Enable(Console.Error);

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
            if (!(Configuration is null))
            {
                loggerConfiguration.ReadFrom.Configuration(Configuration);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        /// <summary>Finalises the globally shared logger.</summary>
        /// <remarks>
        ///    <para>This method resets the <see cref="Log.Logger" /> propery by calling the <see cref="Log.CloseAndFlush()" /> method.</para>
        /// </remarks>
        private static void FinaliseLogging()
        {
            Log.CloseAndFlush();

            SelfLog.Disable();
        }

        /// <summary>Initialises the application configuration properties and the globally shared logger.</summary>
        /// <param name="args">Command line arguments (excluding the program's command). If <c>null</c>, it is ignored.</param>
        /// <remarks>
        ///    <para>This method simply calls the <see cref="InitialiseConfiguration(String[])" /> and <see cref="InitialiseLogging()" /> methods respectively.</para>
        /// </remarks>
        private static void InitialiseEnvironment(String[]? args = null)
        {
            InitialiseConfiguration(args);
            InitialiseLogging();
        }

        /// <summary>Finalises the globally shared logger.</summary>
        /// <remarks>
        ///    <para>This method simply calls the <see cref="FinaliseLogging()" /> method.</para>
        /// </remarks>
        private static void FinaliseEnvironment() =>
            FinaliseLogging();

        /// <summary>Represents the program's main procedure.</summary>
        /// <param name="args">Command line arguments (excluding the program's command).</param>
        /// <returns>A task that represents running the program. Its <see cref="Task{TResult}.Result" /> property is the resulting status code returned by the program (0 on success).</returns>
        /// <remarks>
        ///    <para>For more details about the program and what it does, read the rest of the documentation.</para>
        ///    <para>On success, the program returns <see cref="ExitSuccess" />; on failure, <see cref="ExitFailure" /> is returned.</para>
        /// </remarks>
        public static async Task<Int32> Main(String[]? args = null)
        {
            InitialiseEnvironment(args);

            using ILoggerProvider loggerProvider = new SerilogLoggerProvider(null, true);
            using ILoggerFactory loggerFactory = new SerilogLoggerFactory(null, true, null);

            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger(typeof(Program).FullName!);

            try
            {
                logger.LogDebug(
                    "The application has started. CLR version: {clr}, .NET framework: {dotnet}, time: {time:HH:mm:ss}",
                        Environment.Version,
                        Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName,
                        DateTime.Now
                );

                using HttpClientHandler clientHandler = new HttpClientHandler();
                using HttpClient client = new HttpClient(clientHandler, true)
                {
                    BaseAddress =
                        Configuration["Text:WebSource:BaseAddress"] is String webSourceBaseAddress &&
                            !String.IsNullOrEmpty(webSourceBaseAddress) ?
                                new Uri(webSourceBaseAddress) :
                                null
                };

                ITokeniser tokeniser = new RegexSplitTokeniser(
                    pattern:
                        Configuration["Tokeniser:Pattern"] is String tokeniserPattern &&
                            !String.IsNullOrEmpty(tokeniserPattern) ?
                                tokeniserPattern :
                                RegexSplitTokeniser.DefaultInclusivePattern,
                    options: Configuration.GetValue<RegexOptions>(
                        "Tokeniser:Options",
                        RegexTokeniser.DefaultOptions
                    )
                );

                Pen pen;
                {
                    String?[] tokens;

                    Stopwatch stopwatch = new Stopwatch();

                    using (
                        TextDownloader textDownloader = new TextDownloader(
                            logger: loggerFactory.CreateLogger<TextDownloader>(),
                            client: client,
                            tokeniser: tokeniser,
                            shatteringOptions: Configuration.GetValue<ShatteringOptions?>("ShatteringOptions"),
                            disposeMembers: false
                        )
                    )
                    {
                        tokens = await textDownloader.DownloadTextAsync(
                            uri:
                                Configuration["Text:WebSource:RequestUri"] is String webSourceRequestUri &&
                                    !String.IsNullOrEmpty(webSourceRequestUri) ?
                                        webSourceRequestUri :
                                        null,
                            encoding:
                                Configuration["Text:WebSource:Encoding"] is String encodingName &&
                                    !String.IsNullOrEmpty(encodingName) ?
                                        Encoding.GetEncoding(encodingName) :
                                        null
                        );
                    }

                    logger.LogDebug(
                        "Creating a pen from the downloaded text. Token count: {count:D}",
                            tokens.Length
                        );

                    stopwatch.Start();

                    pen = new Pen(
                        context: tokens,
                        comparisonType: Configuration.GetValue<StringComparison>(
                            "Pen:ComparisonType",
                            StringComparison.Ordinal
                        ),
                        sentinelToken:
                            Configuration["Pen:SentinelToke"] is String sentinelToken &&
                                !String.IsNullOrEmpty(sentinelToken) ?
                                    sentinelToken :
                                    null,
                        intern: Configuration.GetValue<Boolean>("Pen:Intern")
                    );

                    stopwatch.Stop();

                    logger.LogInformation(
                        "Pen successfully created from the downloaded text. Time elapsed: {duration:D} ms, token count: {count:D}",
                            stopwatch.ElapsedMilliseconds,
                            pen.Context.Count
                    );
                }

                String text = String.Join(
                    String.Empty,
                    pen.Render(
                        relevantTokens: Configuration.GetValue<Int32>("Text:RandomGenerator:RelevantTokens"),
                        random: Configuration.GetSection("Text:RandomGenerator:Seed").Exists() ?
                            new Random(Configuration.GetValue<Int32>("Text:RandomGenerator:Seed")) :
                            new Random(),
                        fromPosition: Configuration.GetValue<Nullable<Int32>>("Text:RandomGenerator:FromPosition")
                    ).Take(
                        Configuration.GetValue<Int32>(
                            "Text:RandomGenerator:MaxTokens",
                            Int32.MaxValue
                        )
                    )
                );
                logger.LogInformation(
                    "Generated text: {text}",
                        text.Length > 100 ?
                            JsonSerializer.Serialize(text.Substring(0, 99)) + "…" :
                            JsonSerializer.Serialize(text)
                );

                Console.WriteLine(text);
            }
            catch (Exception exception)
            {
                logger.LogCritical(
                    exception,
                    "An error has occured while running the program."
                );
            }
            finally
            {
                logger.LogDebug(
                    "The application is closing. Time: {time:HH:mm:ss}",
                        DateTime.Now
                );

                FinaliseEnvironment();
            }

            return ExitSuccess;
        }
    }
}
