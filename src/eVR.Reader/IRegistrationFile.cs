using System.Text;

namespace eVR.Reader
{
    /// <summary>
    /// Interface for the Elementary Files that contain the eVR registration data: 
    /// 
    /// - EF Registration_A
    /// - EF Registration_B
    /// - EF Registration_C
    /// </summary>
    public interface IRegistrationFile
        : IElementaryFile
    {
        /// <summary>
        /// The encoding used in this Elementary File with eVR registration data
        /// </summary>
        Encoding? CharacterSetEncoding { get; }
    }
}
