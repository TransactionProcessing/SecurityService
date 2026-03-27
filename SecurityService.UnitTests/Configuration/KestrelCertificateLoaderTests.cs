using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SecurityService.BusinessLogic;
using SecurityService.Configuration;
using Shouldly;

namespace SecurityService.UnitTests.Configuration;


public class KestrelCertificateLoaderTests : IDisposable
{
    private readonly string _temporaryDirectory;

    public KestrelCertificateLoaderTests()
    {
        this._temporaryDirectory = Path.Combine(Path.GetTempPath(), $"newsecurity-kestrel-{Guid.NewGuid():N}");
        Directory.CreateDirectory(this._temporaryDirectory);
    }

    [Fact]
    public void LoadCertificate_NoConfiguration_ReturnsNull()
    {
        var certificate = KestrelCertificateLoader.LoadCertificate(new KestrelOptions(), this._temporaryDirectory);

        certificate.ShouldBeNull();
    }

    [Fact]
    public void LoadCertificate_RelativePath_LoadsCertificate()
    {
        const string password = "password";
        var certificatePath = this.CreateCertificateFile("securityservice.pfx", password);

        using var certificate = KestrelCertificateLoader.LoadCertificate(new KestrelOptions
        {
            Path = Path.GetFileName(certificatePath),
            Password = password
        }, this._temporaryDirectory);

        certificate.ShouldNotBeNull();
        certificate.Subject.ShouldContain("CN=localhost");
    }

    [Fact]
    public void LoadCertificate_AutoDiscover_LoadsFirstMatchingCertificate()
    {
        const string password = "password";
        this.CreateCertificateFile("securityservice.pfx", password);

        using var certificate = KestrelCertificateLoader.LoadCertificate(new KestrelOptions
        {
            AutoDiscoverFromContentRoot = true,
            Password = password
        }, this._temporaryDirectory);

        certificate.ShouldNotBeNull();
        certificate.HasPrivateKey.ShouldBeTrue();
    }

    [Fact]
    public void LoadCertificate_MissingConfiguredFile_Throws()
    {
        var exception = Should.Throw<FileNotFoundException>(() => KestrelCertificateLoader.LoadCertificate(new KestrelOptions
        {
            Path = "missing.pfx",
            Password = "password"
        }, this._temporaryDirectory));

        exception.FileName.ShouldNotBeNull();
        exception.FileName.ShouldContain("missing.pfx");
    }

    public void Dispose()
    {
        if (Directory.Exists(this._temporaryDirectory))
        {
            Directory.Delete(this._temporaryDirectory, recursive: true);
        }
    }

    private string CreateCertificateFile(string fileName, string password)
    {
        using RSA rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
        var certificateBytes = certificate.Export(X509ContentType.Pfx, password);
        var certificatePath = Path.Combine(this._temporaryDirectory, fileName);
        File.WriteAllBytes(certificatePath, certificateBytes);
        return certificatePath;
    }
}
