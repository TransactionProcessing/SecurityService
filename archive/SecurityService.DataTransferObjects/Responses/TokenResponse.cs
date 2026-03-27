using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.DataTransferObjects.Responses
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class TokenResponse
    {
        /// <summary>
        /// The access token
        /// </summary>
        /// <value>The access token.</value>
        public String AccessToken { get; private set; }

        /// <summary>
        /// Gets the expires.
        /// </summary>
        /// <value>The expires.</value>
        public DateTimeOffset Expires { get; private set; }

        /// <summary>
        /// The expires
        /// </summary>
        /// <value>The expires in.</value>
        public Int64 ExpiresIn { get; private set; }

        /// <summary>
        /// Gets the issued.
        /// </summary>
        /// <value>The issued.</value>
        public DateTimeOffset Issued { get; private set; }

        /// <summary>
        /// The refresh token
        /// </summary>
        /// <value>The refresh token.</value>
        public String RefreshToken { get; private set; }

        public static TokenResponse Create(String token)
        {
            dynamic auth = JsonConvert.DeserializeObject(token);

            Int64 expiresIn = auth["expires_in"].Value;
            String accessToken = auth["access_token"].Value;

            DateTimeOffset issued = DateTimeOffset.Now;
            DateTimeOffset expires = DateTimeOffset.Now.AddSeconds(expiresIn);

            String refreshToken = null;
            //For client credentials, the refresh_token will not be present
            if (auth["refresh_token"] != null)
            {
                refreshToken = auth["refresh_token"].Value;
            }

            return TokenResponse.Create(accessToken, refreshToken, expiresIn, issued, expires);
        }

        public static TokenResponse Create(String accessToken,
                                           String refreshToken,
                                           Int64 expiresIn,
                                           DateTimeOffset issued = default(DateTimeOffset),
                                           DateTimeOffset expires = default(DateTimeOffset))
        {
            return new TokenResponse(accessToken,refreshToken,expiresIn,issued, expires);
        }

        private TokenResponse(String accessToken,
                              String refreshToken,
                              Int64 expiresIn,
                              DateTimeOffset issued = default(DateTimeOffset),
                              DateTimeOffset expires = default(DateTimeOffset))
        {
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
            this.ExpiresIn = expiresIn;
            this.Issued = issued;
            this.Expires = expires;
        }
    }
}
