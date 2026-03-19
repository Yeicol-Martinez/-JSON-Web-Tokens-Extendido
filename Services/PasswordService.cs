using System.Security.Cryptography;
using System.Text;

namespace UsuariosAPI.Services
{
    public interface IPasswordService
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }

    public class PasswordService : IPasswordService
    {
        public string Hash(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash  = SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public bool Verify(string password, string hash) => Hash(password) == hash;
    }
}
