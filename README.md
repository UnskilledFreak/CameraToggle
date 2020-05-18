# CPEM - CameraPlus External Mover

CPEM is an external, standalone tool allowing you to modify your cameras on the fly by reacting to specific commands.

This tool is standalone and depends only on three simple things:
 - Beat Saber Mod `CameraPlus` - written and modified by many great modders - thank you all!
 - some sort of chatbot allowing you to add custom commands and create files like Streamlabs Chatbot
 - .NET Core 3.1
 - text file with Beat Saber's install directory

## Why?

I wanted my viewers to be able to see me playing and dancing in different ways while beating some blocks.
Back in the days, CameraPlus dont have its own profile support, so i wrote this handy tool.

## How does it work?

I looked up to its code and saw its using FileSystemWatchers to react to changes of its settings. So I wrote an console prototype which implemented CPEM's core functionallity: loading CameraPlus settings files, modifing it and save it back and success. CameraPlus immediately updated it while i was playing. I then added the feature of reacting to commands written to a text file inside its own commad-folder, the file included the name of the command.

This is basically how it is working right now.

I added an GUI so i can port it with overlay apps like OVRToolkit or DailyOVR and interact with it myself.

Its code is terrible but works for me.

It _should_ run on Linux but not tested 

## Commands?

As of right now - 05-18-2020 - there are 6 hard coded commands:
- Back
- Front
- FPS
- FPS with smaller corner cameras
- toggle 360° - will enable/disable the 360° movement - you can use two settings for the camera positions, with and without 360°! - works only for Back and Front cameras right now)
- Restore - will load a backup of all cams and applys it to the current cams - the backup is loaded when CPEM is started

Not a command but added to the GUI:

Reload - this will, like the start, reload all cam settings and use it as backup instead of the first started ones. This is handy so if you change your camera settings ingame and you are happy with it, just hit the button and the `Restore` command will use your new settings instead of the old one

## Layouts
Inside its folder, theres a CameraPlus folder. Here you can find my very own layout i am useing for streaming on Twitch.tv.

There is currently no way of changing the layout, but you can change the camera positions and rotations!

See [Planed Features](#planed-features) for more details.

## See it in action
[CPEM Viewer reaction on Twitch.tv](https://www.twitch.tv/videos/555731945)
  
Yes im not a good player :D
  
Screenshots comming soon!

## Planed features
- add support for:
  - camera movement and rotation
  - deleting and adding new cameras
  - alternative profiling of different cam status
  - CameraPlus own profile system
  - ChatCore integration (?)
- make it costumizable instead of hard coded only for my purposes so everyone can use it with there own layouts and stuff

## Known issues

- it WILL break your camera settings if there is a new update of CameraPlus with new settings
- it MAY crashes your game when switching between cams to fast - I will add some delay to that later

