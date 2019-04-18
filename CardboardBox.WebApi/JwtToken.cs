using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace CardboardBox.WebApi
{
    public class JwtToken
    {
        private List<Claim> claims = new List<Claim>();

        public string this[string key]
        {
            get => claims.Find(t => t.Type == key)?.Value;
            set
            {
                var claim = claims.Find(t => t.Type == key);

                if (claim != null)
                    claims.Remove(claim);

                claims.Add(new Claim(key, value));
            }
        }

        public string Email
        {
            get => this[ClaimTypes.Email];
            set => this[ClaimTypes.Email] = value;
        }

        public SymmetricSecurityKey Key { get; }

        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public int ExpireyMinutes { get; set; } = 3;
        public string SigningAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256;

        public JwtToken(SymmetricSecurityKey key)
        {
            this.Key = key;
        }
        public JwtToken(SymmetricSecurityKey key, string token)
        {
            this.Key = key;
            Read(token);
        }
        public JwtToken(string key)
        {
            this.Key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
        }
        public JwtToken(string key, string token)
        {
            this.Key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
            Read(token);
        }

        public JwtToken AddClaim(params Claim[] claims)
        {
            foreach (var claim in claims)
                this.claims.Add(claim);
            return this;
        }
        public JwtToken AddClaim(string key, string value)
        {
            return AddClaim(new Claim(key, value));
        }
        public JwtToken Expires(int minutes)
        {
            this.ExpireyMinutes = minutes;
            return this;
        }
        public JwtToken SetEmail(string email)
        {
            Email = email;
            return this;
        }
        public JwtToken SetIssuer(string issuer)
        {
            this.Issuer = issuer;
            return this;
        }
        public JwtToken SetAudience(string audience)
        {
            this.Audience = audience;
            return this;
        }

        public string Write()
        {
            this[JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString();

            var token = new JwtSecurityToken
            (
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(ExpireyMinutes),
                signingCredentials: new SigningCredentials(Key, SigningAlgorithm)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private void Read(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenSecure = handler.ReadJwtToken(token);

            var validations = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = Key,
                ValidateIssuerSigningKey = true
            };

            claims = handler.ValidateToken(token, validations, out SecurityToken ts).Claims.ToList();

            var t = (JwtSecurityToken)ts;
            Issuer = t.Issuer;
            Audience = t.Audiences.First();
            ExpireyMinutes = (t.ValidTo - DateTime.Now).Minutes;
            SigningAlgorithm = t.SignatureAlgorithm;
        }
    }
}
