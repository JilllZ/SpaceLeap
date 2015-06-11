## SpaceLeap

### Description

SpaceLeap is a VR multiplayer cooperative game inspired
by Spaceteam. Downloading and running Spaceteam for iOS
or Android may be the easiest way to introduce the
mechanics behind SpaceLeap.

### Prerequisites

- Leap Motion Controller
- Rift DK2
- Oculus Runtime 0.5.0.1 or newer
- Windows or OS X

### Setup

Attach your Leap Motion Controller to the front of your
Oculus Rift via VR Developer Mount. Ensure that you have
enabled images via the Leap Motion Control Panel. This
can be verified via the Diagnostic Visualizer. Test your
Rift via the "Show Demo Scene" button in the Oculus
Configuration Utility.

Once ready, open up SpaceLeap.exe, SpaceLeap\_DirectToRift.exe, or
SpaceLeap.app, depending on your setup. You should see
green buttons in front of you for joining a game or
hosting. Use your hand to select the "Host Game" button.
As soon as a "Ready" button appears, press that to start
a single-player game.

If the buttons are not directly in front of you and it is
not convenient to turn around, press spacebar to re-center
the view. This can actually be used through the game.

### Gameplay

After a short delay where it says "Waiting for instructions",
you should see actual instructions, e.g. "Turn On Pineapple
Cutter". The object may be in the form of a button, a toggle
switch, lever, or numerical slider. Toggle switches are
configured such that up means On, down means Off. Levers
should be pulled to the right for On, left for Off. Buttons
and sliders should be fairly intuitive to press or slide.

When you have succesfully completed enough steps in the
allotted time, you'll enter "Air Space" and move on to
the next level.

### Multiplayer

After testing the app in a single-player environment, you
can work with a friend to start a multi-player game. Follow
instructions similar to above where one player should
select "Host Game". The other player should select "Join Game".
Technically you should be able to connect two players from
anywhere in the world, but it is recommended to start off
on the same subnet. On Windows, if you see a dialog requesting
network access for SpaceLeap.exe, grant this accordingly
for public and private networks.

For both players, when the Ready dialog appears, do not
press it immediately. Wait until both players show up,
then press "Ready". Note that, confusingly, both players may
show up as "Player 1".

You can actually add more than two players (one host and
multiple joiners), but we have not tested the upper limit.

### Troubleshooting

* When the Rift is not connected or the runtime is not
  installed, the app may have a frozen 2D screen on startup.
  This typically has a small flat appearance of the
  Host/Join Game buttons, and some fields for the host IP.
* When the Leap Motion service is not running or if images
  are not enabled, this may also result in the flat screen.
* Various bugs could cause any other sort of error at startup.
  You may be able to find more information in output\_log.txt
  in the SpaceLeap\_Data directory on Windows, or
  ~/Library/Logs/Unity/Player.log on OS X.
* If the Oculus Rift HDMI cable, tracker USB cable, or
  Leap Motion Controller is loose and disconnects during the
  game, this will cause problems.
* Multiple players can dramatically increase the chance of
  such unanticipated issues. This is not too different
  from playing Spaceteam across several devices with an
  unreliable Bluetooth or WiFi signal. Try out the single-player
  experience before aggressively scaling up.

### Building

If you wish to build from source, Unity 5.1 is required for
its revamped multiplayer API. You must also have Blender installed
and associated with \*.blend files.
