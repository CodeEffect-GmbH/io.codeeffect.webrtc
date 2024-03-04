using System.Collections.Generic;

namespace CodeEffect.WebRTC
{
    /// <summary>
    /// Contains information to connect via ICE.
    /// </summary>
    public readonly struct IceServerInfo
    {
        /// <summary>
        /// Array to set URLs of your STUN/TURN servers
        /// </summary>
        public string[] Urls { get; }

        /// <summary>
        /// Optional: Specifies the username to use when authenticating with the ICE server.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Optional: Specifies the credential to use when authenticating with the ICE server.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="urls"><see cref="Urls"/>.</param>
        /// <param name="username"><see cref="Username"/>.</param>
        /// <param name="password"><see cref="Password"/>.</param>
        public IceServerInfo(List<string> urls, string username = null, string password = null)
        {
            Urls = urls.ToArray();
            Username = username ?? string.Empty;
            Password = password ?? string.Empty;
        }
    }
}
