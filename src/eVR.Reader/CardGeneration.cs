namespace eVR.Reader
{
    /// <summary>
    /// The generation of a card.
    /// </summary>
    public enum CardGeneration
    {
        /// <summary>
        /// First generation, using RSA Active Authentication
        /// </summary>
        Generation1,

        /// <summary>
        /// Second generation, using RSA Active Authentication
        /// </summary>
        Generation2,

        /// <summary>
        /// Third generation, using Elliptic Curve Active Authentication
        /// </summary>
        Generation3,
    }
}
