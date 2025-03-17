namespace Softalleys.Utilities.ObjectValues.Password;

/// <summary>
///     Specifies the hash algorithms supported for hashing operations.
/// </summary>
public enum HashAlgorithm
{
    /// <summary>
    ///     Represents the SHA-256 hash algorithm.
    ///     SHA-256 (Secure Hash Algorithm 256-bit) is a cryptographic hash function
    ///     that produces a 256-bit hash value, widely used for data integrity verification.
    /// </summary>
    Sha256,

    /// <summary>
    ///     Represents the SHA-512 hash algorithm.
    ///     SHA-512 (Secure Hash Algorithm 512-bit) is a cryptographic hash function
    ///     that produces a 512-bit hash value. It is used in various security applications
    ///     and protocols, including TLS and SSL, PGP, SSH, and IPsec.
    /// </summary>
    Sha512
}