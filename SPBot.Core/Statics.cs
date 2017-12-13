using System;
using System.Collections.Generic;
using System.Text;

namespace SPBot.Core
{
    public class Statics
    {
        public static string HelpMessages()
        {
            string Commands = "";
            Commands = "+help (which i'm guessing you know) to get these commands." + Environment.NewLine;
            Commands += "+play <Youtube URL> to play the requested song." + Environment.NewLine;
            Commands += "+dab to make the bot dab." + Environment.NewLine;
            Commands += "+repeat to toggle repeat for the current song." + Environment.NewLine;
            Commands += "+skip skip to the next song." + Environment.NewLine;
            Commands += "+say <Message> to TTS in the current server." + Environment.NewLine;
            Commands += "+clear to empty the playlist." + Environment.NewLine;
            Commands += "And that's all I can do at the moment! ^_^";
            return Commands;
        }
    }
}
