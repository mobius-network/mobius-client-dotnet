# mobius-client-dotnet

# Mobius DApp Store C# .NET SDK

The Mobius DApp Store .Net SDK makes it easy to integrate Mobius DApp Store MOBI payments. The Mobius .NET SDK is a library that works with any .NET Standard or .NET Core API or application. This also means that any Unity Project running on version 2018.1 or later can now utilize this SDK for in-app cryptocurrency payments with 0 fees. The SDK has a target framework of netstandard2.0 making it compatible with a wide variety of .NET projects.

## Installation
Install the nuget package into an existing .NET project.

```sh
$ dotnet add package mobius-client-dotnet
```

## Production Server Setup

Your production server must use HTTPS and set the below header on the `/auth` endpoint:

`Access-Control-Allow-Origin: *`

## Authentication

### Explanation

When a user opens an app through the DApp Store it tells the app what Mobius account it should use for payment.

The application needs to ensure that the user actually owns the secret key to the Mobius account and that this isn't a replay attack from a user who captured a previous request and is replaying it.

This authentication is accomplished through the following process:

* When the user opens an app in the DApp Store it requests a challenge from the application.
* The challenge is a payment transaction of 1 XLM from and to the application account. It is never sent to the network - it is just used for authentication.
* The application generates the challenge transaction on request, signs it with its own private key, and sends it to user.
* The user receives the challenge transaction and verifies it is signed by the application's secret key by checking it against the application's published public key (that it receives through the DApp Store). Then the user signs the transaction with its own private key and sends it back to application along with its public key.
* Application checks that challenge transaction is now signed by itself and the public key that was passed in. Time bounds are also checked to make sure this isn't a replay attack. If everything passes the server replies with a token the application can pass in to "login" with the specified public key and use it for payment (it would have previously given the app access to the public key by adding the app's public key as a signer).

Note: the challenge transaction also has time bounds to restrict the time window when it can be used.

**Basic Authentication Console Demo**

```bash
$ git clone https://github.com/mobius-network/mobius-client-dotnet.git

$ cd mobius-client-dotnet

$ dotnet run --project Mobius.Console
```

### Sample Server Implementation

Using .NET Core

```csharp
namespace DotNetCore.API.Controllers
{
    [Route("auth")]
    [EnableCors("All")]
    public class AuthController : ControllerBase
    {
        private IConfiguration Configuration;
        private string APP_KEY {get; set;}
        private string APP_DOMAIN {get; set;}

        public AuthController(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.APP_KEY = Configuration.GetValue("APP_KEY", "string");
            this.APP_DOMAIN = Configuration.GetValue("APP_DOMAIN", "string");
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            return new Mobius.Library.Auth.Challenge().Call(this.APP_KEY);
        }

        [HttpPost]
        public ActionResult<string> Post(
            [FromForm] TokenRequest request = null,
            [FromQuery] string xdr = null,
            [FromQuery] string public_key = null
        )
        {
            if (request.Xdr == null && xdr == null)
                return BadRequest("xdr cannot be null");

            if (request.PublicKey == null && public_key == null)
                return BadRequest("public_key cannot be null");

            xdr = xdr != null ? xdr : request.Xdr;
            public_key = public_key != null ? public_key : request.PublicKey;

            try
            {
                var token = new Auth.Token(this.APP_KEY, xdr, public_key);
                token.Validate();

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.APP_KEY));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                Claim[] claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, public_key),
                    new Claim("public_key", public_key ),
                    new Claim(JwtRegisteredClaimNames.Jti, token.Hash())
                };

                var timebounds = token.TimeBounds();

                JwtSecurityToken payload = new JwtSecurityToken(
                    issuer: this.APP_DOMAIN,
                    audience: this.APP_DOMAIN,
                    claims: claims,
                    expires: DateTimeOffset.FromUnixTimeSeconds(timebounds.MaxTime).UtcDateTime,
                    signingCredentials: creds);

                string signedToken = new JwtSecurityTokenHandler().WriteToken(payload);

                return Ok(signedToken);
            }
            catch (Exception ex)
            {
                return StatusCode(401, ex.Message);
            }
        }
    }
}
```

## Payment

### Explanation

After the user completes the authentication process they have a token. They now pass it to the application to "login" which tells the application which Mobius account to withdraw MOBI from (the user public key) when a payment is needed. For a web application the token is generally passed in via a `token` request parameter. Upon opening the website/loading the application it checks that the token is valid (within time bounds etc) and the account in the token has added the app as a signer so it can withdraw MOBI from it.

### Sample Payment Implementation

Using .NET Core

```csharp
namespace DotNetCore.API.Controllers
{
    [Route("api")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ApiController : Controller
    {
        private IConfiguration Configuration;
        private string APP_KEY {get; set;}
        public ApiController(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.APP_KEY = Configuration.GetValue("APP_KEY", "string");
        }

        [HttpPost("charge")]
        public async Task<IActionResult> Charge([FromForm] PaymentRequest request)
        {
            if (request.Amount <= 0) return BadRequest("Invalid Amount");

            try
            {
                string userPublicKey = User.Claims.FirstOrDefault().Value;
                App dapp = await new AppBuilder().Build(this.APP_KEY, userPublicKey);

                var response = await dapp.Charge(request.Amount, request.TargetAddress);

                return Ok(new {
                    status = "Ok",
                    tx_hash = response.Hash,
                    balance = dapp.UserBalance()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
```


## Development

```bash
# Clone this repo
$ git clone https://github.com/mobius-network/mobius-client-dotnet.git

# Build project to ./package/lib
$ dotnet build

# Run tests
$ dotnet test Mobius.Test
```

## Full API Demo

Veiw a full .NET Core API demo along with a flappy bird frontend at [https://docs.mobius.network/docs/dotnet-core-api](https://docs.mobius.network/docs/dotnet-core-api)


## Contributing

Bug reports and pull requests are welcome on GitHub at https://github.com/mobius-network/mobius-client-dotnet. This project is intended to be a safe, welcoming space for collaboration, and contributors are expected to adhere to the [Contributor Covenant](http://contributor-covenant.org) code of conduct.

## License

The package is available as open source under the terms of the [MIT License](https://opensource.org/licenses/MIT).

## Code of Conduct

Everyone interacting in the Mobius::Client project’s codebases, issue trackers, chat rooms and mailing lists is expected to follow the [code of conduct](https://github.com/mobius-network/mobius-client-dotnet/blob/master/CODE_OF_CONDUCT.md).