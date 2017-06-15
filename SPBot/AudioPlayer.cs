using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using YoutubeExtractor;
using Discord.Audio;
using System.Diagnostics;

namespace SPBot
{
    public class AudioPlayer
    {
        private Process FFMPEGProcess;
        public AudioOutStream DiscordOutStream;
        public List<Object> Videos;

        public delegate void SendMessage(string Message, Discord.IMessageChannel MessageChannel);
        public event SendMessage SendMessage_Raised;

        public Discord.IMessageChannel MessageClient;

        private IAudioClient AudioClient;
        private bool IsRepeated = false;

        public AudioPlayer()
        {
            Videos = new List<Object>();
            FFMPEGProcess = null;
        }

        public string DoPlay(string VideoUrl)
        {
            string retval = "";
            Object VideoObject = null;
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
                        IEnumerable<VideoInfo> values;
                        try
                        {
                            values = DownloadUrlResolver.GetDownloadUrls(VideoUrl, false);
                            VideoInfo Video = values.FirstOrDefault(video => video.VideoType == VideoType.Mp4);
                            if (Video != null && Video.RequiresDecryption)
                            {
                                DownloadUrlResolver.DecryptDownloadUrl(Video);
                            }
                            VideoObject = Video;
                        }
                        catch(YoutubeParseException)
                        {
                            string args = @"YouLiveLinker.py " + VideoUrl;
                            ProcessStartInfo PSI = new ProcessStartInfo()
                            {
                                FileName = @"python.exe",
                                Arguments = args
                            };
                            Process Proc = Process.Start(PSI);
                            Proc.WaitForExit();
                            string Link = System.IO.File.ReadAllText("file.txt");
                            if (Link.Contains("m3u8") == false)
                            {
                                retval = "No valid URI found :(";
                            }
                            else
                            {
                                VideoObject = Link;
                            }
                            System.IO.File.Delete("file.txt");
                        }
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

        public void EngageOutStream(IAudioClient audioClient)
        {
            AudioClient = audioClient;
            DiscordOutStream = audioClient.CreatePCMStream(AudioApplication.Mixed, 1920);
        }

        public Object GetNext()
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
                Object NewVid = Videos.FirstOrDefault();
                Videos.Remove(NewVid);
                if(NewVid is VideoInfo)
                {
                    VideoInfo CastedObject = (VideoInfo)NewVid;
                    await PlayVideo(CastedObject);
                }
                else
                {
                    string CastedObject = (string)NewVid;
                    await PlayURL(CastedObject);
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
                Object VidInfo = GetNext();
                if(IsRepeated == false)
                {
                    if (VidInfo is VideoInfo)
                    {
                        VideoInfo CastedObject = (VideoInfo)VidInfo;
                        SendMessage_Raised("Now Playing On SpagBot: " + CastedObject.Title, MessageClient);
                    }
                    else
                    {
                        string CastedObject = (string)VidInfo;
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
                Object VidInfo = GetNext();
                if (VidInfo is VideoInfo)
                {
                    VideoInfo CastedObject = (VideoInfo)VidInfo;
                    SendMessage_Raised("Now Playing On SpagBot: " + CastedObject.Title, MessageClient);
                }
                else
                {
                    string CastedObject = (string)VidInfo;
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
