using System.Security.Cryptography;

namespace eVR.Reader.Services
{
    /// <summary>
    /// Class used to generate random byte arrays.
    /// </summary>
    public class RandomGenerator
        : IRandomGenerator
    {
        #region Interface - IRandomGenerator

        /// <summary>
        /// Generate a random byte array
        /// </summary>
        /// <param name="length">The number of bytes in the array</param>
        /// <returns></returns>
        public Task<byte[]> GenerateRandom(int length)
        {
            return Task.Run(() =>
            {
                var g = RandomNumberGenerator.Create();

                byte[] result = new byte[length];
                g.GetBytes(result);

                return result;
            });
        }

        #endregion
    }
}
