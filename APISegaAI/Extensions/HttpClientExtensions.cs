using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

namespace APISegaAI.Extensions
{
    public static class HttpClientExtensions
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration config)
        {
            // Настройка LlmClient
            services.AddHttpClient("LlmClient", (sp, client) =>
            {
                var baseUrl = config["LlmSettings:BaseUrl"] ?? 
                    throw new ArgumentNullException(nameof(config), "LlmSettings:BaseUrl configuration is missing");
                var apiKey = config["LlmSettings:ApiKey"];
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.Timeout = TimeSpan.FromSeconds(int.Parse(config["LlmSettings:TimeoutSeconds"] ?? "300"));
            })
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var certPath = config["LlmSettings:CertificatePath"] ?? 
                    throw new ArgumentNullException(nameof(config), "LlmSettings:CertificatePath configuration is missing");
                var cert = new X509Certificate2(certPath);
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(cert);
                handler.ServerCertificateCustomValidationCallback = (request, certChain, chain, sslPolicyErrors) =>
                {
                    if (certChain == null)
                        return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;

                    var chainContext = new X509Chain();
                    chainContext.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    chainContext.ChainPolicy.ExtraStore.Add(cert);
                    return chainContext.Build(certChain) && sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
                };
                return handler;
            })
            .AddPolicyHandler(PollyExtensions.GetRetryPolicy());

            // для docker
            // services.AddHttpClient("LlmClient", (sp, client) =>
            // {
            //     var config = sp.GetRequiredService<IConfiguration>();
            //     var baseUrl = config["LlmSettings:BaseUrl"] ?? throw new ArgumentNullException(nameof(config), "LlmSettings:BaseUrl configuration is missing");
            //     var apiKey = config["LlmSettings:ApiKey"];
            //     client.BaseAddress = new Uri(baseUrl);
            //     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            //     client.Timeout = TimeSpan.FromSeconds(int.Parse(config["LlmSettings:TimeoutSeconds"] ?? "300"));
            // })
            // .ConfigurePrimaryHttpMessageHandler(sp =>
            // {
            //     var config = sp.GetRequiredService<IConfiguration>();
            //     var certPath = Path.Combine(AppContext.BaseDirectory, config["LlmSettings:CertificatePath"] ?? throw new ArgumentNullException(nameof(config), "LlmSettings:CertificatePath configuration is missing"));
            //     var logger = sp.GetRequiredService<ILogger<Program>>();
            //     logger.LogInformation("Loading certificate from: {CertPath}", certPath);
            //     if (!File.Exists(certPath))
            //         throw new FileNotFoundException($"Certificate file not found: {certPath}");
            //     var cert = new X509Certificate2(certPath);
            //     logger.LogInformation("Certificate loaded: {CertSubject}", cert.Subject);
            //     var handler = new HttpClientHandler();
            //     handler.ClientCertificates.Add(cert);
            //     handler.ServerCertificateCustomValidationCallback = (request, certChain, chain, sslPolicyErrors) =>
            //     {
            //         logger.LogInformation("SSL Policy Errors: {SslPolicyErrors}", sslPolicyErrors);
            //         if (certChain == null)
            //         {
            //             logger.LogError("Certificate chain is null");
            //             return false;
            //         }
            //         var chainContext = new X509Chain();
            //         chainContext.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            //         chainContext.ChainPolicy.ExtraStore.Add(cert);
            //         chainContext.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            //         var chainBuilt = chainContext.Build(certChain);
            //         logger.LogInformation("Chain built: {ChainBuilt}", chainBuilt);
            //         if (!chainBuilt)
            //         {
            //             foreach (var status in chainContext.ChainStatus)
            //                 logger.LogWarning("Chain status: {Status} - {StatusInformation}", status.Status, status.StatusInformation);
            //         }
            //         return chainBuilt;
            //     };
            //     return handler;
            // })
            // .AddPolicyHandler(PollyExtensions.GetRetryPolicy());

            // Настройка ZephyrClient
            services.AddHttpClient("ZephyrClient", (sp, client) =>
            {
                var baseUrl = config["ZephyrSettings:BaseUrl"] ?? 
                    throw new ArgumentNullException(nameof(config), "ZephyrSettings:BaseUrl configuration is missing");
                var apiKey = config["ZephyrSettings:ApiKey"];
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            })
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var certPath = config["ZephyrSettings:CertificatePath"] ?? 
                    throw new ArgumentNullException(nameof(config), "ZephyrSettings:CertificatePath configuration is missing");
                var cert = new X509Certificate2(certPath);
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(cert);
                handler.ServerCertificateCustomValidationCallback = (request, certChain, chain, sslPolicyErrors) =>
                {
                    if (certChain == null)
                        return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
                        
                    var chainContext = new X509Chain();
                    chainContext.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    chainContext.ChainPolicy.ExtraStore.Add(cert);
                    return chainContext.Build(certChain) && sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
                };
                return handler;
            })
            .AddPolicyHandler(PollyExtensions.GetRetryPolicy());

            return services;
        }
    }
}