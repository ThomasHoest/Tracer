using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EQATEC.Tracer.Tools
{
  public class ImageCache
  {
    static Dictionary<string, BitmapImage> mImageCache = new Dictionary<string, BitmapImage>();

    public static Image GetImage(string path)
    {
      if (!mImageCache.ContainsKey(path))
        mImageCache.Add(path, UIUtils.GetPngBitmapImageFromRessource(path));

      //ImageDrawing imgDrawing = new ImageDrawing();
      //imgDrawing.ImageSource = mImageCache[path];
      //imgDrawing.Rect = new Rect(new Size(16, 16));
      //imgDrawing.Freeze();
      //DrawingImage imageControl = new DrawingImage(imgDrawing);
      Image img = new Image();
      img.Source = mImageCache[path];
      return img;
    }
  }
}