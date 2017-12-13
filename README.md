# SpagBot [![Build status](https://ci.appveyor.com/api/projects/status/psnod85ca34bdvxr?svg=true)](https://ci.appveyor.com/project/JackRyder/spagbot)
A discord bot written in C# with the love and help of Discord.NET


## What is SpagBot?
Spagbot is a discord bot capable of playing music from YouTube, as well as serving other requests (more to come).

SpagBot currently has 2 versions, a .NET Framework build, which will only run well on Windows, but it also has a version which is build using the .NET Core Framework, which can run well on Mac OSX, Linux and Windows.

## What's the difference?

I have made more progress on the .NET core project as opposed to the .NET framework version, So usage of that is recommended! :)

## What's the Python file?

The Python file is my way of making sure I don't need libraries in .NET that work for both .NET Framework and .NET Core, which is good because the library that the main one uses is currently .NET Framework only, and has a few copies that don't replicate and work correctly with my current codebase. The .NET core application now works, although it currently only supports YouTube videos and livestreams at the moment :)

## Can I use your bot in my Discord?

You sure can! When I can get the url required for you to add it to your room :)

[Link to invite to your discord!](https://discordapp.com/oauth2/authorize?&client_id=299586375752089600&scope=bot&permissions=0)
