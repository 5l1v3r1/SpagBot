using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using YoutubeExtractor;
using Beautiplayer;
using System.Windows.Forms;
using Discord.Audio;
using Discord;
using System.Diagnostics;

namespace SPBot
{
    public class AudioPlayer
    {
        private Process FFMPEGProcess;
        public AudioOutStream DiscordOutStream;
        public List<VideoInfo> Videos;

        public delegate void SendMessage(string Message);
        public event SendMessage SendMessage_Raised;

        private IAudioClient AudioClient;

        public AudioPlayer()
        {
            Videos = new List<VideoInfo>();
            FFMPEGProcess = null;
        }

        public string DoPlay(string VideoUrl)
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
                        var values = DownloadUrlResolver.GetDownloadUrls(VideoUrl, false);
                        VideoInfo Video = values.FirstOrDefault(video => video.VideoType == VideoType.Mp4);
                        if (Video != null && Video.RequiresDecryption)
                        {
                            DownloadUrlResolver.DecryptDownloadUrl(Video);
                        }
                        VideoObject = Video;

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

        public void EngageOutStream(IAudioClient audioClient)
        {
            AudioClient = audioClient;
            DiscordOutStream = audioClient.CreatePCMStream(AudioApplication.Mixed, 1920);
        }

        public async Task<VideoInfo> GetNext()
        {
            return Videos.FirstOrDefault();
        }

        public async Task PlayNext(bool KillTrack = false)
        {
            if(KillTrack)
            {
                FFMPEGProcess.Kill();
                FFMPEGProcess = null;
            }
            if (Videos.Count > 0)
            {
                VideoInfo NewVid = Videos.FirstOrDefault();
                Videos.Remove(NewVid);
                await PlayVideo(NewVid);
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
                Process.GetProcessesByName("ffmpeg").ToList().ForEach(x => x.Kill());
            }
            catch(Exception)
            {
                
            }
            if (Videos.Count == 0)
            {
                DiscordOutStream = null;
                await AudioClient.StopAsync();
            }
            else
            {
                VideoInfo VidInfo = await GetNext();
                SendMessage_Raised("Now Playing On SpagBot: " + VidInfo.Title);
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
