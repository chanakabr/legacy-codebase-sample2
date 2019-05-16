using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ImageManager
{
    public enum ImageType
    {
        THUMB,
        FULL,
        SIZE
    }

    public enum ResizeStatus
    {
        PENDING,
        SUCCESS,
        FAILED
    }

    public class ImageObj
    {
        public ImageType eImageType;
        public ResizeSettings oResizeSettings;
        public string sExt;
        public string sBaseName;
        public ResizeStatus eResizeStatus;

        public ImageObj() : this(string.Empty, ImageType.THUMB, 0, 0, string.Empty) { }

        public ImageObj(string baseName, ImageType eType, int width, int height, string ext)
        {
            eResizeStatus = ResizeStatus.PENDING;
            sBaseName = baseName;
            eImageType = eType;

            if (eType == ImageType.FULL)
            {
                oResizeSettings = new ResizeSettings();
                oResizeSettings.Quality = 100;
            }
            else
            {
                oResizeSettings = new ResizeSettings(width, height, FitMode.Crop);
                oResizeSettings.Quality = 80;
            }

            sExt = ext.Replace(".", string.Empty);
        }

        public override string ToString()
        {
            string name = string.Empty;

            switch (eImageType)
            {
                case ImageType.THUMB:
                    name = "tn";
                    break;
                case ImageType.FULL:
                    name = "full";
                    break;
                case ImageType.SIZE:
                    name = string.Format("{0}X{1}", oResizeSettings.Width, oResizeSettings.Height);
                    break;

                default:
                    break;
            }

            return string.Format("{0}_{1}.{2}", sBaseName, name, sExt);
        }
    }

    public class ResizeSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public FitMode FitMode { get; set; }
        public int Quality { get; set; }

        public ResizeSettings()
        {
        }

        public ResizeSettings(int width, int height, FitMode fitMode, int? quality = null)
        {
            Width = width;
            Height = height;
            FitMode = fitMode;
            Quality = quality ?? 100;
        }

    }

    public enum FitMode
    {
        Crop = 0,
        Resize = 1,
    }
}
