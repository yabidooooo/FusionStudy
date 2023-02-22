Readme - Fusion Dragon Hunter VR Demo
Copyright: Exit Games GmbH  Email: developer@photonengine.com


# Overview

    This readme will help you get started with the Fusion Dragon Hunter VR Demo.

    Documentation and background info:
    https://doc.photonengine.com/en-us/fusion/current/samples/fusion-dragonhunters-vr


# Supported Platforms

    Editor
    Standalone
    VR, Oculus Quest 2 (other VR platforms work but are not explicitly tested)


# Photon Cloud Account

    The Fusion Dragon Hunter VR Demo uses Fusion, Photon Voice and the Photon Cloud.
    A free account for the Photon Cloud is needed (to use the online matchmaking and fallback relay).

    The "Photon Fusion Hub" window should pop up when you open the demo in the Editor for the first time.
    This will guide you to create the needed Fusion AppId.

    In short:
    - Sign in or register on: https://dashboard.photonengine.com
    - Create an app of type Fusion and one of type Photon Voice
    - Copy and paste both AppIds into the PhotonAppSettings scriptable object


# Community and Help

    Discord: https://discord.gg/pTnq7KcN6D
    Forum: https://forum.photonengine.com


# Hello-Fusion-VR

## Starting Scene
Load the starting scene ("VR_Start") to launch the game. This scene will start Fusion in Singleplayer-mode. This way all the interactions work without any special non-networked code. Select one of the buttons to start as Host or Client and load the proper Game scene.


## The Player
Hand.cs: Updates Pose according to input. Handles grabbing / dropping of interactables.

Interactable.cs: A grabbable object. Handles velocity tracking towards hand while grabbed.

HighlightCollector.cs: Attached to hand. Uses OnTriggerEnter / Leave to collect Highlightable objects.

Highlightable.cs: Component being searched for by HighlightCollector. Also enables / disabled visual highlight.

SpawnLocalPlayer.cs: Gives user the option to spawn VR or PC rig ( usefull for multiplayer testing on one machine )

## The Local Rigs
PlayerInputHandler.cs: Collects the input and sends it on to Fusion.
LocalController.cs: writes the user input into the InputData according to Actions assigned

PCInput.cs: A Debug PC input script to test multiplayer features more easily. This does not handle all features a Controller / VR player could and only has one hand.

## Input
The input struct ( found in InputData.cs ) is seperated into the main struct ( InputData ) and the indiviual hands ( InputDataController ).
Position and Rotation are saved as is in local space.
Buttons are sorted into two categories: "Actions" and "States"

* "Actions" are One-time actions such as, button down events, or Teleport.
    To avoid loosing input in the case of a packetloss we switch the bits whenever the action is performed and then check uppon receiving if the bit has changed.
    After preprocessing the Actions the bit is 1 for the tick when the action was performed.
    The previous Actions need to be saved as [Networked] for prediction and resimulation to work.
* "States" are contiuous actions such as Teleport Mode is activated. The bit is 1 as long as the button is held.


# Assets CC0
https://freesound.org/people/bruno.auzet/sounds/527435/
https://freesound.org/people/cyoung510/sounds/521242/
https://freesound.org/people/Saltbearer/sounds/545742/
https://freesound.org/people/worthahep88/sounds/319217/

https://freesound.org/people/Framing_Noise/sounds/256911/

https://freesound.org/people/999999990/sounds/320345/
https://freesound.org/people/cubic.archon/sounds/44192/


# Setup

Consider this done as you got this sample as "full project".
Else you would have to:

### Allow 'unsafe' Code
1. Edit -> Project Settings -> Player -> Other Settings
    => enable "Allow 'unsafe' Code"

### Add Package Dependencies
1. Add the following Packages in the Packagemanager:
    * Input System
    * OpenXR Plugin

### Setup OpenXR
1. Project Settings-> XR Plug-in Managment->
    * Select OpenXR
    -> Click on the Red icon next to it and let it fix all issues
2. Project Settings-> XR Plug-in Managment->OpenXR->Features
    * Select the controllers you want to support ( tested with HTC Vive Controller and Valve Index Controller, Oculus Quest 1)

### Create an AppId
1. Go to https://dashboard.photonengine.com/
2. Select "Create A New App".
3. Select Photon Type Fusion, and enter the name, etc. and create.
4. Copy the created AppId.
5. Select Assets/Photon/Fusion/Resources/PhotonAppSettings.asset in the project.
6. Paste the AppId into "App Id Realtime".
7. Repeat this for a Photon Voice app.

