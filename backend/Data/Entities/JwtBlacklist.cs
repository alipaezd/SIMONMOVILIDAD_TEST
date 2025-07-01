using System;

namespace Simon.Movilidad.Api.Data.Entities
{
    public class JwtBlacklist
    {
        public int Id { get; set; }
        public string Jti { get; set; } = null!;
        public DateTime RevokedAt { get; set; }
    }
}
