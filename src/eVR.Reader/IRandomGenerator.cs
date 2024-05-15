namespace eVR.Reader
{
    /// <summary>
    /// Interface for a random byte array generator
    /// </summary>
    public interface IRandomGenerator
    {
        /// <summary>
        /// Generate a random byte array
        /// </summary>
        /// <param name="length">The number of bytes in the array</param>
        /// <returns></returns>
        Task<byte[]> GenerateRandom(int length);
    }
}
