using RealityCollective.Extensions;
using UnityEngine;

namespace CodeEffect.WebRTC.Audio
{
    /// <summary>
    /// Provides utilitiy and convenience functions for managing
    /// a WebRTC peers audio track / source.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PeerAudioSource : MonoBehaviour
    {
        [SerializeField, Tooltip("Should the audio source be configured for spatial audio?")]
        private bool isSpatial = false;

        /// <summary>
        /// The <see cref="UnityEngine.AudioSource"/> playing this peer's audio.
        /// </summary>
        public AudioSource AudioSource { get; private set; }

        /// <summary>
        /// Should the audio source be configured for spatial audio?
        /// </summary>
        public bool IsSpatial
        {
            get => isSpatial;
            set
            {
                if (isSpatial == value)
                {
                    return;
                }

                isSpatial = value;
                UpdateAudioSource();
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
            UpdateAudioSource();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (AudioSource.IsNotNull())
            {
                AudioSource.Stop();
                AudioSource = null;
            }
        }

        /// <summary>
        /// Updates the <see cref="audioSource"/> configuration according to the
        /// current <see cref="PeerAudioSource"/> settings.
        /// </summary>
        protected virtual void UpdateAudioSource()
        {
            AudioSource.spatialize = IsSpatial;
            AudioSource.spatializePostEffects = IsSpatial;
            AudioSource.spatialBlend = IsSpatial ? 1f : 0f;
        }
    }
}
