using CodeEffect.WebRTC.Audio;
using CodeEffect.WebRTC.Video;
using RealityCollective.Extensions;

namespace CodeEffect.WebRTC
{
    /// <summary>
    /// Represents a connected peer within the <see cref="IWebRTCConferenceService"/>.
    /// </summary>
    public class WebRTCClient
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connnectionId">The connection identifier of th client within the <see cref="IWebRTCConferenceService"/> conference.</param>
        public WebRTCClient(string connnectionId)
        {
            ConnectionId = connnectionId;
            IsAudioEnabled = true;
            IsVideoEnabled = true;
        }

        /// <summary>
        /// The peer's connection Id.
        /// </summary>
        public string ConnectionId { get; }

        private bool isAudioEnabled;
        /// <summary>
        /// Is the <see cref="Audio"/> of this client enabled?
        /// </summary>
        /// <remarks>Will always return <c>false</c>, if <see cref="Audio"/> is <c>null</c>.</remarks>
        public bool IsAudioEnabled
        {
            get => isAudioEnabled && Audio.IsNotNull();
            set => isAudioEnabled = value;
        }

        private bool isVideoEnabled;
        /// <summary>
        /// Is the <see cref="Video"/> of this client enabled?
        /// </summary>
        /// <remarks>Will always return <c>false</c>, if <see cref="Video"/> is <c>null</c>.</remarks>
        public bool IsVideoEnabled
        {
            get => isVideoEnabled && Video != null;
            set => isVideoEnabled = value;
        }

        /// <summary>
        /// The client's video texture.
        /// </summary>
        public PeerVideoSource Video { get; set; }

        /// <summary>
        /// The client's audio stream.
        /// </summary>
        public PeerAudioSource Audio { get; set; }
    }
}