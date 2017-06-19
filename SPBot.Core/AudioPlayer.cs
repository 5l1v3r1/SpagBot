using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Discord.Audio;
using System.Diagnostics;

namespace SPBot.Core
{
    public class AudioPlayer
    {
        private Process FFMPEGProcess;
        public AudioOutStream DiscordOutStream;
        public List<VideoInfo> Videos;

        public delegate void SendMessage(string Message, Discord.IMessageChannel MessageChannel);
        public event SendMessage SendMessage_Raised;

        public Discord.IMessageChannel MessageClient;

        private IAudioClient AudioClient;
        private bool IsRepeated = false;

        public AudioPlayer()
        {
            Videos = new List<VideoInfo>();
            FFMPEGProcess = null;
        }

        public async Task<string> DoPlayAsync(string VideoUrl)
        {
            string retval = "";
            VideoInfo VideoObject = null;
            if (VideoUrl.Trim() != "")
            {
                string RegexDomain = @"^(?:https?:\/\/)?(?:[^@\/\n]+@)?(?:www\.)?([^:\/\n]+)";
                string Domain = "";
                Regex Regex = new Regex(RegexDomain);
                Match Match = Regex.Match(VideoUrl);
                if (Match.Groups.Count > 0)
                {
                    Domain = Match.Groups[0].Value.ToLower();
                    if (Domain.Contains("youtube") || Domain.Contains("youtu.be"))
                    {
                        VideoObject = await GetVideoViaTCPAsync(VideoUrl);
                    }
                    else if (Domain.Contains("soundcloud"))
                    {
                        //gotta use the API or something..
                    }
                    else
                    {
                        retval = "I'm not quite sure how to play from " + Domain + ", sorry.";
                    }
                    if (VideoObject != null)
                    {
                        //play something for me :)
                        if (FFMPEGProcess == null || FFMPEGProcess.HasExited)
                        {
                            retval = "MOVE";
                        }
                        else
                        {
                            retval = "QUEUE";
                        }
                        Videos.Add(VideoObject);
                    }
                    else
                    {
                        retval = "Song download failed :(";
                    }
                }
                else
                {
                    retval = "No valid URI found :(";
                }

            }
            else
            {
                retval = "You want me to play nothing? You can use +skip for that..";
            }
            return retval;
        }

        private async Task<VideoInfo> GetVideoViaTCPAsync(string domain)
        {
            using (System.Net.Sockets.TcpClient TCP = new System.Net.Sockets.TcpClient())
            {
                await TCP.ConnectAsync(System.Net.IPAddress.Parse("127.0.0.1"), 1212);
                var stream = TCP.GetStream();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(domain);
                stream.Write(data, 0, data.Length);
                string response = "";
                do
                {
                    System.Threading.Thread.Sleep(200);
                } while (stream.CanRead == false);
                using (System.IO.StreamReader SR = new System.IO.StreamReader(stream))
                {
                    response = SR.ReadToEnd();
                }
                VideoInfo VideoInfo = null;
                Newtonsoft.Json.Linq.JToken Vals = Newtonsoft.Json.Linq.JToken.Parse(response);
                if(Vals["errorset"] != null)
                {
                    SendMessage_Raised("Failed to get track: " + Vals.SelectToken("errorset").ToString(), MessageClient);
                }
                else
                {
                    string url = Vals.SelectToken("url").ToString();
                    string title = Vals.SelectToken("title").ToString();
                    VideoInfo.Types type = VideoInfo.Types.Video;
                    switch (Vals.SelectToken("type").ToString())
                    {
                        case "video":
                            type = VideoInfo.Types.Video;
                            break;
                        case "livestream":
                            type = VideoInfo.Types.Livestream;
                            break;

                    }
                    VideoInfo = new VideoInfo(url, title, type);
                }
                return VideoInfo;

            }
        }

        public void EngageOutStream(IAudioClient audioClient)
        {
            AudioClient = audioClient;
            DiscordOutStream = audioClient.CreatePCMStream(AudioApplication.Mixed, 1920);
        }

        public VideoInfo GetNext()
        {
            return Videos.FirstOrDefault();
        }

        public async Task PlayNext(bool KillTrack = false)
        {
            if(KillTrack && FFMPEGProcess != null)
            {
                FFMPEGProcess.Kill();
                FFMPEGProcess = null;
            }
            if (Videos.Count > 0)
            {
                VideoInfo NewVid = Videos.FirstOrDefault();
                Videos.Remove(NewVid);
                if(NewVid.VideoType == VideoInfo.Types.Video)
                {
                    await PlayVideo(NewVid);
                }
                else
                {
                    string LivestreamURL = NewVid.DownloadUrl;
                    await PlayURL(LivestreamURL);
                }
            }
        }

        public void ClearQueue()
        {
            Videos.Clear();
            DiscordOutStream = null;
        }

        public async Task PlayVideo(VideoInfo Video)
        {
            var ffmpeg = CreateStream(Video.DownloadUrl);
            FFMPEGProcess = ffmpeg;
            var output = ffmpeg.StandardOutput.BaseStream;
            try
            {
                
                await output.CopyToAsync(DiscordOutStream);
                await DiscordOutStream.FlushAsync();
                FFMPEGProcess = null;
            }
            catch(Exception)
            {
                
            }
            if(IsRepeated)
            {
                Videos.Insert(0, Video);
            }
            if (Videos.Count == 0)
            {
                DiscordOutStream = null;
                await AudioClient.StopAsync();
            }
            else
            {
                VideoInfo VidInfo = GetNext();
                if(IsRepeated == false)
                {
                    if (VidInfo.VideoType == VideoInfo.Types.Video)
                    {
                        SendMessage_Raised("Now Playing On SpagBot: " + VidInfo.Title, MessageClient);
                    }
                    else
                    {
                        SendMessage_Raised("Playing Livestream on Spagbot!", MessageClient);
                    }
                }
                await PlayNext();
            }
        }

        public bool ToggleRepeat()
        {
            IsRepeated = !IsRepeated;
            return IsRepeated;
        }

        public async Task PlayURL(string url)
        {
            var ffmpeg = CreateStream(url);
            FFMPEGProcess = ffmpeg;
            var output = ffmpeg.StandardOutput.BaseStream;
            try
            {

                await output.CopyToAsync(DiscordOutStream);
                await DiscordOutStream.FlushAsync();
                FFMPEGProcess = null;
            }
            catch (Exception)
            {

            }
            if (Videos.Count == 0)
            {
                DiscordOutStream = null;
                await AudioClient.StopAsync();
            }
            else
            {
                VideoInfo VidInfo = GetNext();
                if (VidInfo.VideoType == VideoInfo.Types.Video)
                {
                    VideoInfo CastedObject = VidInfo;
                    SendMessage_Raised("Now Playing On SpagBot: " + CastedObject.Title, MessageClient);
                }
                else
                {
                    SendMessage_Raised("Playing Livestream on Spagbot!", MessageClient);
                }
                await PlayNext();
            }
        }

        private Process CreateStream(string path)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {path} -ac 2 -f s16le -af volume=0.5 -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            return Process.Start(ffmpeg);
        }

    }
}
