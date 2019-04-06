Human Robot Interaction Version 2.31 09/07/2017

Bachelor Thesis of Hasret Gümgümcü and Daniel Laumer
ETH Zurich and Disney Research Spring 2017

Description
——————
This software was made for a bachelors thesis at ETH Zurich in collaboration with Disney Research Zurich. The goal was to use a Depth Camera, 
the Intel RealSense SR 300, to track hand movements and establish a safety zone around the camera which issues a warning whenever the hand
enters the zone. The idea was to eventually use the camera for a robot in order to be able to interact with humans, detect their movements and
also to ensure safe interaction with the use of this safety zone. 

For more information or the report of the thesis feel free to contact us.

Requirements
———————
- Computer with Windows 10  (macOS or Linux are not supported)
- Intel RealSense SR300 camera
- USB 3.0 Port


Installation Guide
————————
Note that the installation order matters!
1) Make sure you have the „Human Robot Interaction“ software or download it from the GIT depository (contact us for access permission)
2) Install Unity Game Engine (the free version, called „Personal“ is sufficient) at https://store.unity.com/download?ref=personal
3) Open Unity, create a new account online and sign in (you will need to participate in a small survey)
4) Plug in the Intel RealSense Camera and wait if the automatic device installer starts. Else go to Settings —> Devices —> Connected devices —> Add device
5) Install the driver (depth camera manager) for the Intel RealSense SR 300 at https://downloadcenter.intel.com/download/25044/Intel-RealSense-Depth-Camera-Manager?product=92329
6) Create an account at Intel: https://software.intel.com/registration/?lang=en-us
7) Install the Intel RealSense SDK at http://registrationcenter-download.intel.com/akdlm/irc_nas/9078/intel_rs_sdk_offline_package_10.0.26.0396.exe
8) (This step is just needed if you want to edit the code): Install Microsoft Visual Studios Community at https://www.visualstudio.com/de/downloads/?rr=https%3A%2F%2Fwww.google.ch%2F  (choose the two modules „Universal Windows Platform development“ and „-NET desktop development“)


General Usage Notes
——————————
Once you installed all the required softwares, go the the Folder with the „Human Robot Interaction“ Software (called HumanRobotInteractionU) —> Assets —> Visualization
This should start the Unity project. After that click on the play button and it should work :)

If you want to edit the code, go to HumanRobotInteractionU —> Assets —> HumanRobotInteraction —> HumanRobotInteractionVS.csproj
This should start the Visual Studios Project

====================================================

Contact Information
—————————
Daniel Laumer
ETH Zurich
daniel.laumer@gmail.com

Hasret Gümgümcü
ETH Zurich
hasret.g46@gmail.com