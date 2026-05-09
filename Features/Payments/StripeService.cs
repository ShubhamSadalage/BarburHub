using Stripe;

namespace BarberHub.Web.Features.Payments;

public class StripeSettings
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string Currency { get; set; } = "inr";
}

public interface IStripeService
{
    Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string orderNumber, string receiptEmail);
    Event ConstructWebhookEvent(string json, string signatureHeader);
}

public class StripeService : IStripeService
{
    private readonly StripeSettings _settings;

    public StripeService(IConfiguration configuration)
    {
        _settings = configuration.GetSection("Stripe").Get<StripeSettings>() ?? new StripeSettings();
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string orderNumber, string receiptEmail)
    {
        // Stripe amounts are in the smallest unit (paise for INR, cents for USD).
        var amountInSmallestUnit = (long)(amount * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInSmallestUnit,
            Currency = _settings.Currency,
            ReceiptEmail = receiptEmail,
            Metadata = new Dictionary<string, string> { ["OrderNumber"] = orderNumber },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
        };

        var service = new PaymentIntentService();
        return await service.CreateAsync(options);
    }

    public Event ConstructWebhookEvent(string json, string signatureHeader)
    {
        return EventUtility.ConstructEvent(json, signatureHeader, _settings.WebhookSecret);
    }
}
