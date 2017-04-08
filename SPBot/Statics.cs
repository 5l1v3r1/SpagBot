using System;
using System.Collections.Generic;
using System.Text;

namespace SPBot
{
    public class Statics
    {
        public static string HelpMessages()
        {
            string Commands = "";
            Commands = "!help (which i'm guessing you know) to get these commands." + Environment.NewLine;
            Commands += "And that's all I can do at the moment! ^_^";
            return Commands;
        }
    }
}
