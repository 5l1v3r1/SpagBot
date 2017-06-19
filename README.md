# SpagBot [![Build status](https://ci.appveyor.com/api/projects/status/psnod85ca34bdvxr?svg=true)](https://ci.appveyor.com/project/JackRyder/spagbot)
A discord bot written in C# with the love and help of Discord.NET


## What is SpagBot?
Spagbot is a discord bot capable of playing music from YouTube, as well as serving other requests (more to come).

SpagBot currently has 2 versions, a .NET Framework build, which will only run well on Windows, but it also has a version which is build using the .NET Core Framework, which can run well on Mac OSX, Linux and Windows.

## What's the difference?

SpagBot at the moment is only fully functional as the .NET Framework version, as the .NET core version is currently a little bit buggy and needs to be worked on, but it'll get there eventually!

## What's the Python file?

The Python file is my way of making sure I don't need libraries in .NET that work for both .NET Framework and .NET Core, which is good because the library that the main one uses is currently .NET Framework only, and has a few copies that don't replicate and work correctly with my current codebase. Once the .NET Core build is working this file will be used to serve media requests from places such as SoundCloud, YouTube and other media sources.

## Can I use your bot in my Discord?

You sure can! When I can get the url required for you to add it to your room :)
