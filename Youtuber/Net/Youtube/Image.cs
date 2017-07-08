using System;

namespace Youtuber.Net.Youtube
{
    public static class Image
    {
        public static Uri FromID(string videoID, ImageType type){
            string name = string.Empty;
            switch (type) {
                case ImageType._0:
                    name = "0";
                    break;
                case ImageType._1:
                    name = "1";
                    break;
                case ImageType._2:
                    name = "2";
                    break;
                case ImageType._3:
                    name = "3";
                    break;
                case ImageType.Default:
                    name = "default";
                    break;
                case ImageType.HighQualityDefault:
                    name = "hqdefault";
                    break;
                case ImageType.MediumQualityDefault:
                    name = "mqdefault";
                    break;
                case ImageType.StandardDefault:
                    name = "sddefault";
                    break;
                case ImageType.MaximumResolutionDefault:
                    name = "maxresdefault";
                    break;
            }
            return new Uri($"https://img.youtube.com/vi/{videoID}/{name}.jpg");
        }
    }

    public enum ImageType
    {
        _0,
        _1,
        _2,
        _3,
        Default,
        HighQualityDefault,
        MediumQualityDefault,
        StandardDefault,
        MaximumResolutionDefault
    }
}