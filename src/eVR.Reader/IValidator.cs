namespace eVR.Reader
{
    /// <summary>
    /// Interface for the classes used for the validation of the card.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// The name of the validator
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Perform the validation using the state
        /// </summary>
        /// <param name="state">The data read from the card</param>
        /// <returns>An indication whether the validation was successful</returns>
        Task<bool> Validate(eVRCardState state);
    }
}
