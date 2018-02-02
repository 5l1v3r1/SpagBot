using System;
using System.Collections.Generic;
using System.Text;

namespace SPBot.Core
{
    public class VideoInfo
    {
        public string DownloadUrl = "";
        public string Title = "";
        public Types VideoType;
        public enum Types
        {
            Video,
            Livestream
        }

        private VideoInfo(string url, string title, Types Type)
        {
            DownloadUrl = url;
            Title = title;
            VideoType = Type;
        }

        public static VideoInfo CreateCustomVideo(string Url, string Title, Types Type)
        {
            return new VideoInfo(Url, Title, Type);
        }
    }
}
