using Application.Abstractions.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;

namespace Infrastructure.Email;

public sealed class MailgunEmailSender(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<MailgunEmailSender> logger) : IEmailSender
{
    private const string DefaultBaseUrl = "https://api.eu.mailgun.net";

    public async Task SendEmailAsync(string receiver, string subject, string text, string html)
    {
        var apiKey = configuration["MAILGUN_API_KEY"]
            ?? throw new InvalidOperationException("No Mailgun API key provided.");
        var domain = configuration["MAILGUN_DOMAIN"]
            ?? throw new InvalidOperationException("MAILGUN_DOMAIN is not configured");
        var from = configuration["MAILGUN_FROM"]
            ?? throw new InvalidOperationException("MAILGUN_FROM is not configured");
        var baseUrl = configuration["MAILGUN_BASE_URL"] ?? DefaultBaseUrl;

        using var client = new RestClient(httpClient, new RestClientOptions(baseUrl)
        {
            Authenticator = new HttpBasicAuthenticator("api", apiKey)
        });

        var request = new RestRequest($"/v3/{domain}/messages", Method.Post)
        {
            AlwaysMultipartFormData = true
        };

        request.AddParameter("from", from);
        request.AddParameter("to", receiver);
        request.AddParameter("subject", subject);
        request.AddParameter("text", text);
        request.AddParameter("html", html);

        var response = await client.ExecuteAsync(request);
        if (!response.IsSuccessful)
        {
            logger.LogError(response.ErrorException, "Mailgun error: {StatusCode} {ErrorMessage}",
                response.StatusCode, response.ErrorMessage);
            throw new InvalidOperationException("Mailgun error.");
        }
    }
}
