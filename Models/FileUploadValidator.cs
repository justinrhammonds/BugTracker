using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public static class FileUploadValidator
    {
        public static bool IsWebFriendlyFile(HttpPostedFileBase file)
        {
            if (file == null)
                return false;
            if (file.ContentLength > 2 * 1024 * 1024 || file.ContentLength < 1024)
                return false;
            try
            {
                using (var f = Image.FromStream(file.InputStream))
                {
                    return ImageFormat.Jpeg.Equals(f.RawFormat) ||
                            ImageFormat.Png.Equals(f.RawFormat) ||
                            ImageFormat.Gif.Equals(f.RawFormat);
                            
                }
            }
            catch
            {
                return false;
            }
        }

    }
}