using System.Security.Cryptography;

namespace SpotifyApi.Services
{
    public interface IPasswordHasherService
    {
        string Hash(string password);
        bool Verify(string passwordHash, string passwordInput);
    }

    public class PasswordHasherService : IPasswordHasherService
    {
        private const int SaltSize = 128 / 8;
        private const int KeySize = 256 / 8;
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
        private const char Delimiter = ';';

        public string Hash(string password)
        {
            byte[]? salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[]? hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

            return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public bool Verify(string passwordHash, string passwordInput)
        {
            string[]? elements = passwordHash.Split(Delimiter);
            byte[]? salt = Convert.FromBase64String(elements[0]);
            byte[]? hash = Convert.FromBase64String(elements[1]);

            byte[]? hashInput = Rfc2898DeriveBytes.Pbkdf2(passwordInput, salt, Iterations, _hashAlgorithmName, KeySize);

            return CryptographicOperations.FixedTimeEquals(hash, hashInput);
        }
    }
}