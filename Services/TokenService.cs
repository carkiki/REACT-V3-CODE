using System;

using System.Security.Cryptography;

using System.Text;



namespace ReactCRM.Services

{

    /// <summary>

    /// Servicio para generar tokens seguros para links de subida de clientes

    /// </summary>

    public static class TokenService

    {

        private const int TOKEN_LENGTH = 32; // 256 bits



        /// <summary>

        /// Genera un token único y seguro usando GUID v4 + random bytes

        /// </summary>

        public static string GenerateSecureToken()

        {

            // Combinar GUID con bytes aleatorios para máxima seguridad

            var guidBytes = Guid.NewGuid().ToByteArray();

            var randomBytes = new byte[TOKEN_LENGTH];



            using (var rng = RandomNumberGenerator.Create())

            {

                rng.GetBytes(randomBytes);

            }



            // Combinar y crear hash

            var combined = new byte[guidBytes.Length + randomBytes.Length];

            Buffer.BlockCopy(guidBytes, 0, combined, 0, guidBytes.Length);

            Buffer.BlockCopy(randomBytes, 0, combined, guidBytes.Length, randomBytes.Length);



            using (var sha256 = SHA256.Create())

            {

                var hash = sha256.ComputeHash(combined);

                return Convert.ToBase64String(hash)

                    .Replace("+", "-")

                    .Replace("/", "_")

                    .Replace("=", "")

                    .Substring(0, 43); // URL-safe base64, 43 chars = 256 bits

            }

        }



        /// <summary>

        /// Genera un token HMAC-SHA256 basado en clientId y secret

        /// </summary>

        public static string GenerateHmacToken(int clientId, string secretKey)

        {

            var data = $"{clientId}:{DateTime.UtcNow.Ticks}:{Guid.NewGuid()}";

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            var dataBytes = Encoding.UTF8.GetBytes(data);



            using (var hmac = new HMACSHA256(keyBytes))

            {

                var hash = hmac.ComputeHash(dataBytes);

                return Convert.ToBase64String(hash)

                    .Replace("+", "-")

                    .Replace("/", "_")

                    .Replace("=", "");

            }

        }



        /// <summary>

        /// Valida que un token tenga el formato correcto

        /// </summary>

        public static bool IsValidTokenFormat(string token)

        {

            if (string.IsNullOrWhiteSpace(token))

                return false;



            // Token debe tener entre 40-50 caracteres y solo caracteres URL-safe

            if (token.Length < 40 || token.Length > 50)

                return false;



            foreach (char c in token)

            {

                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')

                    return false;

            }



            return true;

        }



        /// <summary>
        /// Genera fecha de expiración (por defecto 30 días)
        /// </summary>
        public static DateTime GenerateExpirationDate(int days = 1)
        {
            return DateTime.UtcNow.AddDays(days);
        }
        /// <summary>
        /// Verifica si un token ha expirado
        /// </summary>
        public static bool IsTokenExpired(DateTime? expirationDate)
        {
            if (!expirationDate.HasValue)
                return false; // Sin expiración = nunca expira
            return DateTime.UtcNow > expirationDate.Value;
        }
    }

}