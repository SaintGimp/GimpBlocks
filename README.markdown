GimpBlocks
======================================================================

# License

Licensed under the [Ms-PL](http://www.microsoft.com/opensource/licenses.mspx#Ms-PL).

# What is it?

Experiments in building a Minecraft-esque game engine.  I have no particular plans to turn this into a real game - it's for my own amusement and education.  As with most of my personal projects, the emphasis is on interesting and expressive design patterns rather than the utmost performance. 

# How do I build and run it?

The GimpBlocks project is a Visual Studio 2015 solution with a dependency on [MonoGame](http://www.monogame.net).

* Install the DirectX Runtime from https://www.microsoft.com/en-us/download/details.aspx?id=35
* Install the most recent stable version of Monogame from http://www.monogame.net/downloads/
* Open the solution in Visual Studio
* Set GimpBlocks as the startup project
* Press F5 to build and run

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
