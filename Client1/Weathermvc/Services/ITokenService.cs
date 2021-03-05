using System;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Weathermvc.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GetToken(string scope);
    }
}
