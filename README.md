Streaming Map Demo
==================

<B>A Saab Dynamics demo of the Streaming Binary Data (SBD) map format</B>

This is a generic streaming API for 3D engines (Unity,Unreal...) that allows fast streaming updates of huge 3D datasets located in cloud or from disc.

This GIT repository will allow you to take a look at a Unity demo, written by the Open Saab Development community using the GizmoSDK code base from Saab Dynamics, Training & Simulation.

The open source and open documentation in here are to be considered as GPL code and can be used in your own projects.

The binaries are licensed by Saab Dynamics. If you are interested in using this for commersial use, please contact 

anders.moden@saabgroup.com




Setup
=====

Setup the Unity git repository in a folder and extract the file http://www.tooltech-software.com/Maps.zip to the "Maps" folder under the Unity project.

Or you can download another at https://www.dropbox.com/s/d22rhv4ly5nj09e/Maps.zip?dl=0 and uncompress to the "Maps" folder under the Unity project.

The demo now comes with an URL to an existing test map http:/www.gizmosdk.se/archives/maps/sweden/eksjo/processed/map.gzd

To run with 32 bits libs change the Assets/csc.rsp file to 

-define:NATIVE_32
-define:UNITY

and to run 64 bits libs change to 

-define:NATIVE_64
-define:UNITY



Running the demo
================

Press the "play" button in the Unity editor and use the keys 'a' and 'd' for left/right and the keys 'w' and 's' for forward/backward
Use the arrow keys to rotate the view. Remember to install a "Maps" folder from the url http://www.tooltech-software.com/Maps.zip
<b><u>Note Win64 only right now</u></b>
Good Luck !


Technology Info
===============

The demo is based on a 3D scenegraph written in native C++ that manages the logistics for loading/unloading data and manages LOD levels and transitions between LOD depending on what data is currently loaded. It uses up to 16 paralell threads to load data from multiple URL based datasources dynamically.
The demo uses a large double precision coordinate system and a ROI (Region Of Interest) subsystem that translates HUGE coordinates into local islands of single precision data. 

The system handles geocentric coordinate systems as well as flat UTM and other conic projections and provides a uniform WGS84 API to control all objects and queries.

The SceneGraph API also allows a fast intersector query to be performed to find ground features and clamp object to the ground.

The demo shows an example of SBD maps (Streaming Binary Data) that are quad or octree based spatial data in 3D. The format allows very large databases (entire globe) to be divided on multiple servers and that can have details down to (mm) in resolution.
