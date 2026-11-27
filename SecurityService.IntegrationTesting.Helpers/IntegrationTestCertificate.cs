using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SecurityService.IntegrationTesting.Helpers;

public sealed class IntegrationTestCertificate : IDisposable
{
    private const string PasswordValue = "password";
    private const string ServerCertificateFileName = "integration-server.pfx";
    private const string RootCertificateFileName = "integration-root.crt";
    private const string ContainerCertificateDirectoryPath = "/app/test-certs";
    private const string RootSubjectName = "CN=SecurityServiceIntegrationTestRoot";
    private const string ServerSubjectName = "CN=localhost";
    private const string LocalhostName = "localhost";
    private const string LoopbackAddress = "127.0.0.1";

    private readonly string _certificateDirectory;
    private readonly X509Certificate2 _certificate;
    private readonly X509Certificate2 _rootCertificate;
    private bool _trustedRootInstalled;
    private bool _disposed;

    private IntegrationTestCertificate(string certificateDirectory, X509Certificate2 certificate, X509Certificate2 rootCertificate)
    {
        _certificateDirectory = certificateDirectory;
        _certificate = certificate;
        _rootCertificate = rootCertificate;
    }

    public string CertificateDirectory => _certificateDirectory;

    public string ContainerCertificateDirectory => ContainerCertificateDirectoryPath;

    public string Password => PasswordValue;

    public string RootCertificatePath => Path.Combine(_certificateDirectory, RootCertificateFileName);

    public string ServerCertificatePath => Path.Combine(_certificateDirectory, ServerCertificateFileName);

    public string ContainerRootCertificatePath => $"{ContainerCertificateDirectoryPath}/{RootCertificateFileName}";

    public static IntegrationTestCertificate Create()
    {
        string certificateDirectory = Path.Combine(Path.GetTempPath(), $"securityservice-cert-{Guid.NewGuid():N}");
        IntegrationTestCertificate? result = null;

        try
        {
            Directory.CreateDirectory(certificateDirectory);

            using RSA rootRsa = RSA.Create(2048);
            CertificateRequest rootRequest = new CertificateRequest(RootSubjectName, rootRsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
            rootRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(rootRequest.PublicKey, false));
            using X509Certificate2 rootCertificate = rootRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));

            using RSA serverRsa = RSA.Create(2048);
            CertificateRequest serverRequest = new CertificateRequest(ServerSubjectName, serverRsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            serverRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
            serverRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
            OidCollection enhancedKeyUsages = new OidCollection();
            enhancedKeyUsages.Add(new Oid("1.3.6.1.5.5.7.3.1"));
            serverRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(enhancedKeyUsages, false));
            SubjectAlternativeNameBuilder subjectAlternativeNames = new SubjectAlternativeNameBuilder();
            subjectAlternativeNames.AddDnsName(LocalhostName);
            subjectAlternativeNames.AddDnsName("identity-server");
            subjectAlternativeNames.AddIpAddress(IPAddress.Parse(LoopbackAddress));
            serverRequest.CertificateExtensions.Add(subjectAlternativeNames.Build());
            serverRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(serverRequest.PublicKey, false));

            byte[] serialNumber = RandomNumberGenerator.GetBytes(16);
            serialNumber[0] &= 0x7f;
            using X509Certificate2 publicCertificate = serverRequest.Create(rootCertificate, rootCertificate.NotBefore, rootCertificate.NotAfter, serialNumber);
            using X509Certificate2 certificate = publicCertificate.CopyWithPrivateKey(serverRsa);
            byte[] pfxBytes = certificate.Export(X509ContentType.Pfx, PasswordValue);
            byte[] certificateBytes = System.Text.Encoding.ASCII.GetBytes(rootCertificate.ExportCertificatePem());

            string serverCertificatePath = Path.Combine(certificateDirectory, ServerCertificateFileName);
            string rootCertificatePath = Path.Combine(certificateDirectory, RootCertificateFileName);
            File.WriteAllBytes(serverCertificatePath, pfxBytes);
            File.WriteAllBytes(rootCertificatePath, certificateBytes);

            result = new IntegrationTestCertificate(
                certificateDirectory,
                X509CertificateLoader.LoadPkcs12(pfxBytes, PasswordValue, X509KeyStorageFlags.EphemeralKeySet),
                X509CertificateLoader.LoadCertificate(certificateBytes));
            result.InstallTrustedRoot();
            return result;
        }
        catch
        {
            result?.Dispose();
            TryDeleteDirectory(certificateDirectory);
            throw;
        }
    }

    public void InstallTrustedRoot()
    {
        ThrowIfDisposed();

        using X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        store.Add(_rootCertificate);
        _trustedRootInstalled = true;
    }

    public string GetContainerCertificatePath()
    {
        return $"{ContainerCertificateDirectoryPath}/{ServerCertificateFileName}";
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            if (_trustedRootInstalled)
            {
                RemoveTrustedRoot();
            }
        }
        catch
        {
            // Cleanup must not mask the original test failure.
        }
        finally
        {
            TryDeleteDirectory(_certificateDirectory);
            _certificate.Dispose();
            _rootCertificate.Dispose();
            _disposed = true;
        }
    }

    private void RemoveTrustedRoot()
    {
        using X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);

        foreach (X509Certificate2 certificate in store.Certificates.Find(X509FindType.FindByThumbprint, _rootCertificate.Thumbprint, validOnly: false))
        {
            store.Remove(certificate);
        }

        _trustedRootInstalled = false;
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch
        {
            // Cleanup must not mask the original test failure.
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
