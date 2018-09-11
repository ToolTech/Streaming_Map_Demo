
Streaming Map Demo
==================

<B>A Saab Dynamics demo of the Streaming Binary Data (SBD) map format</B>

This GIT repository will allow you to take a look at a Unity demo, written by the Open Saab Development community using the GizmoSDK code base from Saab Dynamics, Training & Simulation.

The open source and open documentation in here are to be considered as GPL code and can be used in your own projects.

The binaries are licensed by Saab Dynamics. If you are interested in using this for commersial use, please contact 

anders.moden@saabgroup.com




Setup
=====

Setup the Unity git repository in a folder and extract the file http://www.tooltech-software.com/Maps.zip to the "Maps" folder under the Unity project.


Running the demo
================

Press the "play" button in the Unity editor and use the keys 'a' and 'd' for left/right and the keys 'w' and 's' for forward/backward
Use the arrow keys to rotate the view. Remember to install a "Maps" folder from the url http://www.tooltech-software.com/Maps.zip
Good Luck !


Technology Info
===============

The demo is based on a 3D scenegraph written in native C++ that manages the logistics for loading/unloading data and manages LOD levels and transitions between LOD depending on what data is currently loaded.
The demo uses a large double precision coordinate system and a ROI (Region Of Interest) subsystem that translates HUGE coordinates into local islands of single precision data. 
