namespace CodeEffect.WebRTC.Signaling
{
    public interface ISignalingClient
    {
        /// <summary>
        /// Sends <paramref name="message"/> content to <paramref name="connectionId"/>.
        /// </summary>
        /// <param name="connectionId">The connection to message to.</param>
        /// <param name="message">The message content.</param>
        void SendMessage(string connectionId, string message);

        /// <summary>
        /// Sends <paramref name="message"/> content to <paramref name="connectionId"/>.
        /// </summary>
        /// <param name="connectionId">The connection to message to.</param>
        /// <param name="message">The message content.</param>
        void SendMessage(string connectionId, byte[] message);
    }
}
