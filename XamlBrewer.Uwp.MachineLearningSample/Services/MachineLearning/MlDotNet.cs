using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Mvvm.Services
{
    public static class MlDotNet
    {
        /// <summary>
        /// Copies a file behind a logical Store App path to the local app folder, and returns its physical path.
        /// </summary>
        /// <param name="uwpPath">Store app path (think 'ms-appx:///something...')</param>
        /// <returns>A path to the copied file that non-UWP API's understand</returns>
        /// <remarks>Looks like a hack because it is one.</remarks>
        public static async Task<string> FilePath(string uwpPath)
        {
            var originalFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uwpPath));
            var storageFolder = ApplicationData.Current.LocalFolder;
            var localFile = await originalFile.CopyAsync(storageFolder, originalFile.Name, NameCollisionOption.ReplaceExisting);
            return localFile.Path;
        }
    }
}
