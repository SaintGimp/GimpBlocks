GimpBlocks
======================================================================

# License

Licensed under the [Ms-PL](http://www.microsoft.com/opensource/licenses.mspx#Ms-PL).

# What is it?

Experiments in building a Minecraft-esque game engine.  I have no particular plans to turn this into a real game - it's for my own amusement and education.  As with most of my personal projects, the emphasis is on interesting and expressive design patterns rather than the utmost performance. 

# How do I build and run it?

The GimpBlocks project is a Visual Studio 2012 solution dependencies on a couple of NuGet packages (automatically downloaded when you build) and [MonoGame](http://monogame.codeplex.com/).

* Download and run the MonoGame installer, choose to install OpenAL as well as the core files. (TODO: move all MonoGame dependencies into the repo.)
* Load the solution in Visual Studio.
* Build and run.

The content files are pre-compiled with the XNA content pipeline.  Right now, if you want to add more content files you'll have to do the following:
* Load GimpBlocksContent.sln in Visual Studio 2010 with XNA 4.0 installed.
* Build the solution.
* Copy the contents of \GimpBlocksContent\DummyGame\bin\x86\Debug\Content to \GimpBlocks\Content.
(This will hopefully go away when MonoGame implements its own content pipeline.)

If you run into any problems, let me know.

# What are the controls?

* W/A/S/D for left/right/forward/backward
* E/C for up/down
* -/= to decrease/increase camera speed
* ,/. to decrease/increase camera zoom level
* ESC to exit mouselook, left-click to re-enter mouselook
* U to toggle state updates (useful for inspecting the world from different angles.)
* F to toggle wireframe rendering

# A caution about branches
Until further notice, the only branch that's safe to base work on is master.  Any other branches in this repo may have their history rewritten at any time without warning.
