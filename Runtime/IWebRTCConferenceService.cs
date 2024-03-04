using RealityCollective.ServiceFramework.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeEffect.WebRTC
{
    /// <summary>
    /// A WebRTC conferencing manager service.
    /// </summary>
    public interface IWebRTCConferenceService : IService
    {
        /// <summary>
        /// Are we currently connecting to a conference?
        /// </summary>
        bool IsConnecting { get; }

        /// <summary>
        /// Are we currently connected to a conference?
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Should the local peer be streaming a video track to remote peers?
        /// </summary>
        bool LocalVideoEnabled { get; set; }

        /// <summary>
        /// Should the local peer be streaming an audio track to remote peers?
        /// </summary>
        bool LocalAudioEnabled { get; set; }

        /// <summary>
        /// Connects / reconnects to the configured WebRTC conference.
        /// </summary>
        /// <param name="iceServers">ICE servers to use.</param>
        /// <returns><c>true</c>, if successfully connected to the signaling server.</returns>
        Task<bool> ConnectAsync(IReadOnlyList<IceServerInfo> iceServers);

        /// <summary>
        /// Reconnects to the conference.
        /// </summary>
        /// <returns><c>true</c>, if successfully connected to the signaling server.</returns>
        Task<bool> ReconnectAsync();

        /// <summary>
        /// Disconnects from the conference.
        /// </summary>
        void Disconnect();
    }
}
