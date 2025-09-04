using System.Security.Cryptography;
using System.Text;

namespace RestaurantAPI.Services
{
    public class RsaKeyService
    {
        private readonly RSA _rsa;

        public RsaKeyService(IConfiguration config)
        {
            _rsa = RSA.Create();
            var privateKey = Convert.FromBase64String(config["RsaKeys:PrivateKey"]);
            _rsa.ImportRSAPrivateKey(privateKey, out _);
        }

        public string GetPublicKey()
        {
            return Convert.ToBase64String(_rsa.ExportRSAPublicKey());
        }

        public string Decrypt(string base64Encrypted)
        {
            var encryptedBytes = Convert.FromBase64String(base64Encrypted);
            var decryptedBytes = _rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
