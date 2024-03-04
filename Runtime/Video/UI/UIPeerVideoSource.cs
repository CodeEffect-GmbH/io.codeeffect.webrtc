using UnityEngine;
using UnityEngine.UI;

namespace CodeEffect.WebRTC.Video.UI
{
    /// <summary>
    /// A Unity UI based UI component for displaying / rendering the <see cref="WebRTCClient.Video"/> track.
    /// </summary>
    public class UIPeerVideoSource : MonoBehaviour
    {
        [SerializeField]
        private RawImage videoImage = null;

        [SerializeField]
        private AspectRatioFitter aspectFitter = null;

        [SerializeField]
        private AspectRatioFitter.AspectMode aspectMode = AspectRatioFitter.AspectMode.FitInParent;

        private WebRTCClient client;
        /// <summary>
        /// The <see cref="WebRTCClient"/> we are displaying the <see cref="PeerVideoSource"/> for.
        /// </summary>
        public WebRTCClient Client
        {
            get => client;
            set
            {
                if (string.Equals(client?.ConnectionId, value?.ConnectionId))
                {
                    // Already displaying the client.
                    return;
                }

                // If we previously had a client assigned, we must clean up
                // event registrations.
                if (client != null &&
                    client.Video != null)
                {
                    client.Video.SourceChanged -= Video_SourceChanged;
                }

                client = value;

                // If video is available on this client, register for source changes.
                if (client?.Video != null)
                {
                    client.Video.SourceChanged += Video_SourceChanged;
                    Video_SourceChanged();
                }
            }
        }

        private void Video_SourceChanged()
        {
            videoImage.texture = Client.Video.Texture;
            aspectFitter.aspectMode = aspectMode;
            aspectFitter.aspectRatio = Client.Video.AspectRatio;
        }
    }
}