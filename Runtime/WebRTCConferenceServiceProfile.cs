using RealityCollective.ServiceFramework.Definitions;
using UnityEngine;

namespace CodeEffect.WebRTC
{
    /// <summary>
    /// Configuration profile for the <see cref="IWebRTCConferenceService"/>.
    /// </summary>
    public class WebRTCConferenceServiceProfile : BaseServiceProfile<IWebRTCConferenceServiceModule>
    {
        /// <summary>
        /// Should local video be enabled when joining a conference?
        /// </summary>
        [field: Header("Video"), SerializeField, Tooltip("Should local video be enabled when joining a conference?")]
        public bool LocalVideoEnabled { get; private set; } = true;

        /// <summary>
        /// Should local audio be enabled when joining a conference?
        /// </summary>
        [field: Header("Audio"), SerializeField, Tooltip("Should local audio be enabled when joining a conference?")]
        public bool LocalAudioEnabled { get; private set; } = true;

        /// <summary>
        /// Peer audio source prefab to spawn.
        /// </summary>
        [field: SerializeField, Tooltip("Peer audio source prefab to spawn.")]
        public GameObject PeerAudioSourcePrefab { get; private set; } = null;
    }
}