namespace tophat
{
    public enum ConnectionScope
    {
        /// <summary>
        /// One connection is opened on every request for a unit of work
        /// </summary>
        AlwaysNew,
        /// <summary>
        /// One connection is opened for a single HTTP request
        /// </summary>
        ByRequest,
        /// <summary>
        /// One connection is opened per thread
        /// </summary>
        ByThread,
        /// <summary>
        /// One connection is opened per HTTP session
        /// </summary>
        BySession,
        /// <summary>
        /// One connection is opened for the life of the registration
        /// </summary>
        KeepAlive
    }
}