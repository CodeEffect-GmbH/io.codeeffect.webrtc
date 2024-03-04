using UnityEngine;

namespace CodeEffect.WebRTC.Video
{
    /// <summary>
    /// Delegate subscribing to video source property changes.
    /// </summary>
    public delegate void OnPeerVideoSourceChanged();

    /// <summary>
    /// Provides utilitiy and convenience functions for managing
    /// a WebRTC peers video track / source.
    /// </summary>
    public class PeerVideoSource
    {
        /// <summary>
        /// The peer's video <see cref="UnityEngine.Texture"/>.
        /// </summary>
        public Texture Texture { get; private set; }

        /// <summary>
        /// The <see cref="Texture"/> width in pixels.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The <see cref="Texture"/> height in pixels.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The <see cref="Texture"/> aspect ratio.
        /// </summary>
        public float AspectRatio { get; private set; }

        /// <summary>
        /// Is this video in portrait mode?
        /// </summary>
        public bool IsPortrait { get; private set; }

        /// <summary>
        /// This <see cref="PeerVideoSource"/>'s <see cref="Texture"/> has changed.
        /// </summary>
        public event OnPeerVideoSourceChanged SourceChanged;

        /// <summary>
        /// Sets the video <paramref name="texture"/> on this <see cref="PeerVideoSource"/>.
        /// </summary>
        /// <param name="texture">The peer's video <see cref="UnityEngine.Texture"/>.</param>
        public void SetTrack(Texture texture)
        {
            Texture = texture;
            Width = Texture.width;
            Height = Texture.height;
            IsPortrait = Height > Width;
            AspectRatio = Width / (float)Height;

            SourceChanged?.Invoke();
        }
    }
}
