using CommonWithSL.Enums;
using CommonWithSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL.Converters.Gallery
{
    public class GalleryItemConverterFactory
    {
        public static IGalleryItemConverter CreateConverter(GalleryItemType galleryItemType)
        {
            IGalleryItemConverter converter = null;
            switch (galleryItemType)
            {
                case GalleryItemType.MediaObject:
                    {
                        converter = new GalleryMediaObjectConverter();
                        break;
                    }
                case GalleryItemType.EPGChannelProgramObject:
                    {
                        converter = new GalleryEPGChannelProgramObjectConverter();
                        break;
                    }
            }
            return converter;
        }
    }
}
