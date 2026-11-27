using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SecurityService.IntegrationTesting.Helpers;

public sealed class IntegrationTestRootCertificateTrust : IDisposable
{
    private readonly X509Certificate2 _certificate;
    private bool _disposed;

    private IntegrationTestRootCertificateTrust(X509Certificate2 certificate)
    {
        _certificate = certificate;
    }

    public static IntegrationTestRootCertificateTrust Install(string certificatePath)
    {
        X509Certificate2 certificate = X509CertificateLoader.LoadCertificate(File.ReadAllBytes(certificatePath));

        try
        {
            using X509Store store = new(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            return new IntegrationTestRootCertificateTrust(certificate);
        }
        catch
        {
            certificate.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            using X509Store store = new(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            foreach (X509Certificate2 certificate in store.Certificates.Find(
                         X509FindType.FindByThumbprint,
                         _certificate.Thumbprint,
                         validOnly: false))
            {
                store.Remove(certificate);
            }
        }
        finally
        {
            _certificate.Dispose();
            _disposed = true;
        }
    }
}
