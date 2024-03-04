using CodeEffect.WebRTC.Audio;
using CodeEffect.WebRTC.Signaling;
using CodeEffect.WebRTC.Video;
using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CodeEffect.WebRTC
{
    /// <summary>
    /// Abstract base implementation of <see cref="IWebRTCConferenceService"/>.
    /// </summary>
    public abstract class BaseWebRTCConferenceService<TClient> : BaseServiceWithConstructor, IWebRTCConferenceService where TClient : WebRTCClient
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name assigned to this service instance.</param>
        /// <param name="priority">The priority assigned to this service instance.</param>
        /// <param name="profile">The configuration profile for this service instance.</param>
        public BaseWebRTCConferenceService(string name, uint priority, WebRTCConferenceServiceProfile profile)
            : base(name, priority)
        {
            peerAudioSourcePrefab = profile.PeerAudioSourcePrefab;
            localAudioEnabled = profile.LocalAudioEnabled;
            localVideoEnabled = profile.LocalVideoEnabled;
        }

        private readonly GameObject peerAudioSourcePrefab;

        protected readonly Dictionary<string, TClient> clients = new();
        protected readonly Dictionary<string, PeerVideoSource> pendingVideoSources = new();
        protected readonly Dictionary<string, PeerAudioSource> pendingAudioSources = new();
        protected ISignalingClient signalingClient;

        public delegate void ClientAddedDelegate(TClient client);
        public delegate void ClientUpdatedDelegate(TClient client);
        public delegate void ClientRemovedDelegate(TClient client);

        /// <inheritdoc/>
        public bool IsConnecting { get; protected set; }

        private bool isConnected;
        /// <inheritdoc/>
        public bool IsConnected
        {
            get => isConnected;
            protected set
            {
                isConnected = value;
                if (isConnected)
                {
                    IsConnecting = false;
                }
            }
        }

        private bool localVideoEnabled;
        /// <inheritdoc/>
        public bool LocalVideoEnabled
        {
            get => localVideoEnabled;
            set
            {
                if (localVideoEnabled == value)
                {
                    return;
                }

                localVideoEnabled = value;
                UpdateLocalVideoTrack();
            }
        }

        private bool localAudioEnabled;
        /// <inheritdoc/>
        public bool LocalAudioEnabled
        {
            get => localAudioEnabled;
            set
            {
                if (localAudioEnabled == value)
                {
                    return;
                }

                localAudioEnabled = value;
                UpdateLocalAudioTrack();
            }
        }

        /// <summary>
        /// List of ICE servers used for the last connection attempt.
        /// </summary>
        protected IReadOnlyList<IceServerInfo> IceServers { get; set; }

        /// <summary>
        /// A client was added to the conference.
        /// </summary>
        public event ClientAddedDelegate ClientAdded;

        /// <summary>
        /// A client was updated.
        /// </summary>
        public event ClientUpdatedDelegate ClientUpdated;

        /// <summary>
        /// A client was removed from the conference.
        /// </summary>
        public event ClientRemovedDelegate ClientRemoved;

        /// <inheritdoc/>
        public override void Destroy()
        {
            base.Destroy();

            if (!Application.isPlaying)
            {
                return;
            }

            Disconnect();
        }

        /// <inheritdoc/>
        public abstract Task<bool> ConnectAsync(IReadOnlyList<IceServerInfo> iceServers);

        /// <inheritdoc/>
        public abstract Task<bool> ReconnectAsync();

        /// <inheritdoc/>
        public virtual void Disconnect()
        {
            foreach (var item in pendingAudioSources)
            {
                if (item.Value.IsNotNull())
                {
                    item.Value.Destroy();
                }
            }

            pendingAudioSources.Clear();

            foreach (var item in pendingVideoSources)
            {
                if (item.Value.Texture.IsNotNull())
                {
                    item.Value.Texture.Destroy();
                }
            }

            pendingVideoSources.Clear();

            clients.Clear();
            IsConnected = false;
        }

        #region Clients

        /// <summary>
        /// Creates a new <typeparamref name="TClient"/> for <paramref name="connectionId"/>.
        /// </summary>
        /// <param name="connectionId">The connection the client is created for.</param>
        /// <returns>The new <typeparamref name="TClient"/>.</returns>
        protected TClient CreateClient(string connectionId)
        {
            var client = (TClient)Activator.CreateInstance(typeof(TClient), connectionId);
            clients.Add(connectionId, client);

            OnClientAdded(client);
            ClientAdded?.Invoke(client);

            return client;
        }

        /// <summary>
        /// Raises <see cref="ClientUpdated"/> for <paramref name="connectionId"/>.
        /// </summary>
        /// <param name="connectionId">The client connection to raise the update for.</param>
        protected void UpdateClient(string connectionId)
        {
            if (clients.TryGetValue(connectionId, out var client))
            {
                OnClientUpdated(client);
                ClientUpdated?.Invoke(client);
            }
        }

        /// <summary>
        /// Removes the <typeparamref name="TClient"/> for <paramref name="connectionId"/>.
        /// </summary>
        /// <param name="connectionId">The connection to remove the client for.</param>
        protected void RemoveClient(string connectionId)
        {
            if (clients.TryGetValue(connectionId, out var client))
            {
                clients.Remove(connectionId);

                // Clean up pending audio sources for the connection.
                if (pendingAudioSources.TryGetValue(connectionId, out var peerAudioSource))
                {
                    pendingAudioSources.SafeRemoveDictionaryItem(connectionId);
                    peerAudioSource.gameObject.Destroy();
                }

                // Clean up pending video sources for the connection.
                if (pendingVideoSources.TryGetValue(connectionId, out var peerVideoSource))
                {
                    pendingVideoSources.SafeRemoveDictionaryItem(connectionId);
                    if (peerVideoSource.Texture.IsNotNull())
                    {
                        peerVideoSource.Texture.Destroy();
                    }
                }

                OnClientRemoved(client);
                ClientRemoved?.Invoke(client);
            }
        }

        /// <summary>
        /// The <paramref name="client"/> was added to the conference.
        /// </summary>
        /// <param name="client">The added <typeparamref name="TClient"/>.</param>
        protected virtual void OnClientAdded(TClient client) { }

        /// <summary>
        /// The <paramref name="client"/> was updated.
        /// </summary>
        /// <param name="client">The updated <typeparamref name="TClient"/>.</param>
        protected virtual void OnClientUpdated(TClient client) { }

        /// <summary>
        /// The <paramref name="client"/> was removed from the conference.
        /// </summary>
        /// <param name="client">The removed <typeparamref name="TClient"/>.</param>
        protected virtual void OnClientRemoved(TClient client) { }

        /// <summary>
        /// Attempts to find a <typeparamref name="TClient"/> connected to the session via
        /// the client's <paramref name="connectionId"/>.
        /// </summary>
        /// <param name="connectionId">The connection Id of the <paramref name="client"/>.</param>
        /// <param name="client">The <typeparamref name="TClient"/> related to <paramref name="connectionId"/>.</param>
        /// <returns><c>true</c> if client found.</returns>
        protected bool TryGetByConnectionId(string connectionId, out TClient client)
        {
            if (!string.IsNullOrWhiteSpace(connectionId) &&
                clients.TryGetValue(connectionId, out client))
            {
                return true;
            }

            client = null;
            return false;
        }

        #endregion Clients

        #region Signaling

        /// <summary>
        /// We have received a signaling message from <paramref name="connectionId"/>.
        /// </summary>
        /// <param name="connectionId">The connection the message comes from.</param>
        /// <param name="message">Content of the message.</param>
        protected virtual void OnMessageReceived(string connectionId, string message) { }

        #endregion Signaling

        #region Audio

        /// <summary>
        /// The <see cref="LocalAudioEnabled"/> state has changed.
        /// Update accordingly whether we should be sending audio to peers.
        /// </summary>
        protected abstract void UpdateLocalAudioTrack();

        /// <summary>
        /// We have received audio data from <paramref name="connectionId"/>.
        /// </summary>
        /// <param name="connectionId">The connection the audio comes from.</param>
        protected virtual void OnAudioReceived(string connectionId)
        {
            var clientExists = TryGetByConnectionId(connectionId, out var client);
            if (clientExists && client.Audio.IsNotNull())
            {
                // Client exists and already has an audio source, so we can
                // just update the track to play the latest stream.
                //client.Audio.SetTrack(audioStreamTrack);
                return;
            }

            // Create a audio source and configure it to play the peer's audio track.
            var peerAudioSource = UnityEngine.Object.Instantiate(peerAudioSourcePrefab, Vector3.zero, Quaternion.identity).GetComponent<PeerAudioSource>();
            //peerAudioSource.SetTrack(audioStreamTrack);

            // Update the peer's client information with the just created audio source,
            // if the client already exists.
            if (clientExists)
            {
                client.Audio = peerAudioSource;
                return;
            }

            // Add it to the audio sources registry until the client is ready to map it at a later time.
            pendingAudioSources.EnsureDictionaryItem(connectionId, peerAudioSource, true);
        }

        #endregion Audio

        #region Video

        /// <summary>
        /// The <see cref="LocalVideoEnabled"/> state has changed.
        /// Update accordingly whether we should be sending video to peers.
        /// </summary>
        protected abstract void UpdateLocalVideoTrack();

        /// <summary>
        /// We have received video data from <paramref name="connectionId"/>.
        /// </summary>
        /// <param name="connectionId">The connection the video comes from.</param>
        /// <param name="texture">The <see cref="Texture"/> received.</param>
        protected virtual void OnVideoReceived(string connectionId, Texture texture)
        {
            var clientExists = TryGetByConnectionId(connectionId, out var client);
            if (clientExists && client.Video != null)
            {
                // Client exists and already has a video source, so we can
                // just update the track to play the latest texture.
                client.Video.SetTrack(texture);
                return;
            }

            // Create a video source and configure it to play the peer's video track.
            var peerVideoSource = new PeerVideoSource();
            peerVideoSource.SetTrack(texture);

            // Update the peer's client information with the just created video source,
            // if the client already exists.
            if (clientExists)
            {
                client.Video = peerVideoSource;
                return;
            }

            // Add it to the video sources registry until the client is ready to map it at a later time.
            pendingVideoSources.EnsureDictionaryItem(connectionId, peerVideoSource, true);
        }

        #endregion Video
    }
}