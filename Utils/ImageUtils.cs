using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ek24.Utils;

public static class ImageUtilities
{
    public static BitmapImage LoadImage(Assembly assembly, string name)
    {
        var img = new BitmapImage();
        try
        {
            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains(name));
            var stream = assembly.GetManifestResourceStream(resourceName);
            img.BeginInit();
            img.StreamSource = stream;
            img.EndInit();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return img;
    }
}
