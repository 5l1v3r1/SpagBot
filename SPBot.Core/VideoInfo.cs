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

        public VideoInfo(string url, string title, Types Type)
        {
            DownloadUrl = url;
            Title = title;
            VideoType = Type;
        }


    }
}
