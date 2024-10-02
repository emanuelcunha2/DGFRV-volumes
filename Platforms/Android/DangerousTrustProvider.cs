using Java.IO;
using Java.Security;
using Java.Security.Cert;
using Javax.Net.Ssl;

namespace CheckAllocationApp.Platforms.Android
{

    internal class DangerousTrustProvider : Provider
    {
        private const string TRUST_PROVIDER_ALG = "DangerousTrustAlgorithm";
        private const string TRUST_PROVIDER_ID = "DangerousTrustProvider";

        public DangerousTrustProvider() : base(TRUST_PROVIDER_ID, 1, "A custom provider that trusts all certificates")
        {
            try
            {
                string key = "TrustManagerFactory." + DangerousTrustManagerFactory.GetAlgorithm();
                string val = Java.Lang.Class.FromType(typeof(DangerousTrustManagerFactory)).Name;
                Put(key, val);
            }
            catch (Exception ex)
            {
                // Ensure any exception is logged for debugging purposes.
                System.Console.WriteLine("Failed to initialize DangerousTrustProvider: " + ex.Message);
            }
        }

        public static void Register()
        {
            try
            {
                Provider registered = Security.GetProvider(TRUST_PROVIDER_ID);
                if (registered == null)
                {
                    Security.InsertProviderAt(new DangerousTrustProvider(), 1);
                    Security.SetProperty("ssl.TrustManagerFactory.algorithm", TRUST_PROVIDER_ALG);
                }
            }
            catch (Exception ex)
            {
                // Log any errors during registration to help with debugging.
                System.Console.WriteLine("Failed to register DangerousTrustProvider: " + ex.Message);
            }
        }

        public class DangerousTrustManager : Java.Lang.Object, IX509TrustManager
        {
            public void CheckClientTrusted(X509Certificate[] chain, string authType)
            {
                // Trust all clients
            }

            public void CheckServerTrusted(X509Certificate[] chain, string authType)
            {
                // Trust all servers
            }

            public X509Certificate[] GetAcceptedIssuers()
            {
                return new X509Certificate[0];
            }
        }

        public class DangerousTrustManagerFactory : TrustManagerFactorySpi
        { 
            protected override void EngineInit(IManagerFactoryParameters mgrparams)
            {
                // Initialize trust manager factory
            }

            protected override ITrustManager[] EngineGetTrustManagers()
            {
                return new ITrustManager[] { new DangerousTrustManager() };
            }

            public static string GetAlgorithm()
            {
                return TRUST_PROVIDER_ALG;
            }

            protected override void EngineInit(KeyStore ks)
            {
                // Initialize trust manager factory
            }
        }
    }
}
