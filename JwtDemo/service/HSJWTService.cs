using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtDemo.config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JwtDemo.service;

public class HSJWTService : IJWTService
{
    private IConfiguration _configuration;

    public HSJWTService(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public string GetToken(string name)
    {
        var _JWTTokenOptions=_configuration.GetSection("JWTTokenOptions").Get<JWTTokenOptions>();
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.UniqueName, "admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, "email@124.com"),
            
            new Claim("Name", name),
            new Claim("id", "11"),
            new Claim(ClaimTypes.Name, "ZhangLei"),
            new Claim("Age", "35"),
            new Claim("Mobile", "18664876666"),
            new Claim(ClaimTypes.Role,"Admin"),
            new Claim("Role", "Assistant"),//这个不能默认角色授权，动态角色授权
            new Claim("Sex", "1")//各种信息拼装
        };
        /*
         在 ASP.NET Core 中，您可以使用 SymmetricSecurityKey 类来表示对称加密算法的密钥。
        对称加密算法使用相同的密钥用于加密和解密数据，因此该密钥必须保密并且仅在通信双方之间共享。
        在您提供的代码中，this._JWTTokenOptions.SecurityKey 是用于创建 JWT 签名凭据的密钥字符串。
        Encoding.UTF8.GetBytes 方法将该字符串转换为字节数组，然后使用该字节数组创建一个 SymmetricSecurityKey 对象。
        在 JWT 身份验证中，该密钥用于加密和解密 JWT 数据。
         */
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JWTTokenOptions.SecurityKey));
        /*
         在 ASP.NET Core 中，您可以使用 SigningCredentials 类创建用于 JWT（JSON Web Token）身份验证的签名凭据对象。
        JWT 是一种开放标准，用于在各种应用程序和服务之间安全地传输信息。JWT 由三部分组成：头部、载荷和签名。
        在您提供的代码中，key 是一个 SymmetricSecurityKey 对象，用于存储加密和解密 JWT 数据的密钥值。
        SecurityAlgorithms.HmacSha256 表示使用 HMAC-SHA256 算法进行签名。SigningCredentials 类将这些参数组合在一起，
        创建用于 JWT 签名的签名凭据对象。
         */
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        /**
         * Claims (Payload)
            Claims 部分包含了一些跟这个 token 有关的重要信息。 JWT 标准规定了一些字段，下面节选一些字段:
            iss: The issuer of the token，token 是给谁的
            sub: The subject of the token，token 主题
            exp: Expiration Time。 token 过期时间，Unix 时间戳格式
            iat: Issued At。 token 创建时间， Unix 时间戳格式
            jti: JWT ID。针对当前 token 的唯一标识
            除了规定的字段外，可以包含其他任何 JSON 兼容的字段。
         * */
        var token = new JwtSecurityToken(
            issuer: _JWTTokenOptions.Isuser,
            audience: _JWTTokenOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddSeconds(60 * 10), //10分钟有效期
            notBefore: DateTime.Now, //立即生效  DateTime.Now.AddMilliseconds(30),//30s后有效
            signingCredentials: creds);
        string returnToken = new JwtSecurityTokenHandler().WriteToken(token);
        return returnToken;
    }
}