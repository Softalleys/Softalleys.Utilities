using System.Security.Cryptography.X509Certificates;

namespace Softalleys.Utilities.Extensions;

/// <summary>
///     Provides a method to retrieve an X509 certificate based on a given certificate identifier.
/// </summary>
public interface ICertificateProvider
{
    /// <summary>
    ///     Retrieves an X509 certificate based on the specified certificate identifier.
    /// </summary>
    /// <param name="certificateId">The identifier of the certificate to retrieve.</param>
    /// <returns>An X509Certificate2 object representing the certificate.</returns>
    X509Certificate2 GetCertificate(CertificateId certificateId);
}