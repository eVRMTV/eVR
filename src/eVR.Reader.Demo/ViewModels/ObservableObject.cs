using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace eVR.Reader.Demo.ViewModels
{
    /// <summary>
    /// Base implementation of interface INotifyPropertyChanged
    /// </summary>
    public class ObservableObject
        : INotifyPropertyChanged
    {
        #region Interface - INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Protected Methods

        /// <summary>
        /// Raise the event PropertyChanged
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Set the value of a property and raise the PropertyChanged event when necessary
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="field">The field that will be updated</param>
        /// <param name="newValue">The new value of the property</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>an indication whether the property was changed</returns>
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }
            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

    }
}
