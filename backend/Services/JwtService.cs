using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Simon.Movilidad.Api.Services
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly int _expMinutes;

        public JwtService(IConfiguration config)
        {
            _secret = config["Jwt:Secret"] 
                      ?? throw new ArgumentNullException("Jwt:Secret");
            _expMinutes = int.Parse(config["Jwt:ExpirationMinutes"]!);
        }

        public string GenerateToken(int userId, string role)
        {
            // Header
            var header = new { alg = "HS256", typ = "JWT" };
            var headerJson = JsonSerializer.Serialize(header);
            var headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));

            // Payload
            var now = DateTimeOffset.UtcNow;
            var payload = new Dictionary<string, object>
            {
                ["sub"]  = userId.ToString(),
                ["role"] = role,
                ["iat"]  = now.ToUnixTimeSeconds(),
                ["exp"]  = now.AddMinutes(_expMinutes).ToUnixTimeSeconds()
            };
            var payloadJson = JsonSerializer.Serialize(payload);
            var payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

            // Signature
            var unsignedToken = $"{headerB64}.{payloadB64}";
            var signature   = ComputeSignature(unsignedToken);

            return $"{unsignedToken}.{signature}";
        }

        public bool ValidateToken(string token, out Dictionary<string,string>? claims)
        {
            claims = null;
            var parts = token.Split('.');
            if (parts.Length != 3) return false;

            var unsignedToken = $"{parts[0]}.{parts[1]}";
            var sigExpected   = ComputeSignature(unsignedToken);
            if (!ConstantTimeEquals(sigExpected, parts[2]))
                return false;

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson)
                       ?? new Dictionary<string, JsonElement>();

            if (dict.TryGetValue("exp", out var expEl) &&
                expEl.GetInt64() < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                return false;

            // Devuelvo las claims como string
            var result = new Dictionary<string,string>();
            foreach (var kv in dict)
                result[kv.Key] = kv.Value.ToString();
            claims = result;
            return true;
        }

        // Helpers

        private string ComputeSignature(string unsignedToken)
        {
            var key = Encoding.UTF8.GetBytes(_secret);
            using var sha = new HMACSHA256(key);
            var sig = sha.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken));
            return Base64UrlEncode(sig);
        }

        private static string Base64UrlEncode(byte[] bytes)
            => Convert.ToBase64String(bytes)
                      .TrimEnd('=')
                      .Replace('+','-')
                      .Replace('/','_');

        private static byte[] Base64UrlDecode(string input)
        {
            var s = input.Replace('-','+').Replace('_','/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }

        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
