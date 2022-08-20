using ChaseLabs.CLConfiguration;
using Octokit;
using Stripe;

namespace OpenDSM.Core.Handlers;

public class PaymentHandler
{
    #region Public Fields

    public static PaymentHandler Instance = Instance ??= new();

    #endregion Public Fields

    #region Private Fields

    private ConfigManager manager;

    #endregion Private Fields

    #region Private Constructors

    private PaymentHandler()
    {
        manager = new("payment", RootDirectory);

        StripeConfiguration.ApiKey = GetAPIKey();
        StripeConfiguration.MaxNetworkRetries = 2;
    }

    #endregion Private Constructors

    #region Private Methods

    private string GetAPIKey()
    {
        string key = manager.GetOrCreate("API_KEY", "").Value;

        if (string.IsNullOrWhiteSpace(key))
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Enter Stripe API Key >> ");
            Console.ResetColor();
            key = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(key))
                return GetAPIKey();
            manager.GetOrCreate("API_KEY", "").Value = key;
        }

        return key;
    }
    private string GetIdempotencyKey()
    {
        string IdempotencyKey = manager.GetOrCreate("Idempotency_Key", "").Value;

        if (string.IsNullOrWhiteSpace(IdempotencyKey))
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Enter Stripe Idempotency Key >> ");
            Console.ResetColor();
            IdempotencyKey = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(IdempotencyKey))
                return GetIdempotencyKey();
            manager.GetOrCreate("Idempotency_Key", "").Value = IdempotencyKey;
        }

        return IdempotencyKey;
    }
    private string GetStripeAccount()
    {
        string StripeAccount = manager.GetOrCreate("Stripe_Account_ID", "").Value;

        if (string.IsNullOrWhiteSpace(StripeAccount))
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Enter Stripe Account >> ");
            Console.ResetColor();
            StripeAccount = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(StripeAccount))
                return GetStripeAccount();
            manager.GetOrCreate("Stripe_Account_ID", "").Value = StripeAccount;
        }

        return StripeAccount;
    }

    #endregion Private Methods

}
