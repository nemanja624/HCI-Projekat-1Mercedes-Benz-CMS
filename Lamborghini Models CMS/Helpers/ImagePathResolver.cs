using System;
using System.IO;

namespace Mercedes_Models_CMS.Helpers
{
    public static class ImagePathResolver
    {
        private const string ImagesFolderRelativePath = "../../../Images";

        public static string? ResolveForDisplay(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            if (File.Exists(imagePath))
            {
                return imagePath;
            }

            string fileName = Path.GetFileName(imagePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return imagePath;
            }

            string relativeCandidate = Path.Combine(ImagesFolderRelativePath, fileName);
            string absoluteCandidate = Path.GetFullPath(relativeCandidate);

            return File.Exists(absoluteCandidate) ? relativeCandidate : imagePath;
        }

        public static string SaveImageToProject(string sourceImagePath, string modelName)
        {
            string imagesDirectory = Path.GetFullPath(ImagesFolderRelativePath);
            Directory.CreateDirectory(imagesDirectory);

            string extension = Path.GetExtension(sourceImagePath);
            string baseName = modelName;
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                baseName = baseName.Replace(invalid, '_');
            }

            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "model";
            }

            string fileName = $"{baseName}-{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            string destinationPath = Path.Combine(imagesDirectory, fileName);
            File.Copy(sourceImagePath, destinationPath, true);

            return Path.Combine(ImagesFolderRelativePath, fileName);
        }
    }
}
