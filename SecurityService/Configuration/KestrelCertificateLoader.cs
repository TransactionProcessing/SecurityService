using System.Security.Cryptography.X509Certificates;
using SecurityService.BusinessLogic;

namespace SecurityService.Configuration;

public static class KestrelCertificateLoader
{
    public static X509Certificate2? LoadCertificate(KestrelOptions? options, string contentRootPath)
    {
        if (options is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(options.Path) == false)
        {
            var configuredCertificatePath = ResolveCertificatePath(options.Path, contentRootPath);
            return LoadCertificateFromPath(configuredCertificatePath, options.Password);
        }

        if (options.AutoDiscoverFromContentRoot == false)
        {
            return null;
        }

        var searchPattern = string.IsNullOrWhiteSpace(options.SearchPattern) ? "*.pfx" : options.SearchPattern;
        var discoveredCertificatePath = Directory.EnumerateFiles(contentRootPath, searchPattern, SearchOption.TopDirectoryOnly)
                                            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                                            .FirstOrDefault();

        if (discoveredCertificatePath is null)
        {
            throw new FileNotFoundException($"No certificate matching '{searchPattern}' was found in '{contentRootPath}'.");
        }

        return LoadCertificateFromPath(discoveredCertificatePath, options.Password);
    }

    private static X509Certificate2 LoadCertificateFromPath(string certificatePath, string? password)
    {
        if (File.Exists(certificatePath) == false)
        {
            throw new FileNotFoundException($"The configured Kestrel certificate '{certificatePath}' was not found.", certificatePath);
        }

        return X509CertificateLoader.LoadPkcs12FromFile(certificatePath, password);
    }

    private static string ResolveCertificatePath(string certificatePath, string contentRootPath)
    {
        if (System.IO.Path.IsPathRooted(certificatePath))
        {
            return certificatePath;
        }

        return System.IO.Path.Combine(contentRootPath, certificatePath);
    }
}
