Kii-2D-Platformer v0.3 (SDK v1.4)
=================================

A demo of using basic Kii functionality on a Unity 2D game

Note: if you just want to install Kii Game Cloud support with no demo
game you just need to get the barebones package available at:
https://www.assetstore.unity3d.com/#/content/15017

This project is also available on Github:
https://github.com/KiiPlatform/Kii2DPlatformer

Files
-----

There are 3 files in the Scripts folder that include Kii Cloud API calls:

1) AuthForm: used to sign-in/sign-up a user and initialize the backend
   and to send asynchronous user sign-in/sign-up requests to the backend
2) Score: used to send and receive player high scores from the backend
3) PlayerHealth: used to send Kii analytics events when the player dies
   
Build Settings Order
--------------------

Scenes/Auth.unity
Scenes/Level.unity

Create your own Game Cloud
--------------------------

1) Create an account at http://developer.kii.com
2) Create an application as explained in "Register an application"
following steps 1, 2, and 3 (disregard the other sections):
http://documentation.kii.com/en/starts/unity/
Choose Unity as platform for your app and the server location of
your backend.
3) Write down App Id and App Key assigned to your app as explained in
"Register an application" following step 4 (disregard the other 
sections):
http://documentation.kii.com/en/starts/unity/
4) Set keys from step 3 in your Unity project by choosing one of these
options:
  a) Go to "Kii Game Cloud" editor menu and setup your keys there
  b) Edit file Assets/Plugins/KiiConfig.txt and add your keys there
  c) Replace those keys in file KiiAutoInitialize.cs in the
  Kii.Initialize() method directly
Important: options a) and b) will only work in Editor mode. If you want
to initialize Kii when building for a specific platform you'll have to
use c). If you're bulding for Android make sure the Stripping level in
your project settings is set to "Disabled" and that the Internet setting
is set to "Require" (not "Auto").
5) If you want to use analytics (and saw an analytics related debug message)
in the game please follow these instructions:
In GameConfig.cs the analytics rule id is 0, but the ID must match 
the analytic rule you should create on developer.kii.com this way:
- Go to developer.kii.com, go to your app's console
- Click on Analytics on the left side bar, then lick on "Create a new rule"
- Name your rule AvgDeathTime or whichever name you like, select "Event Data"
- In the function dropdown select "Avg" and in the field enter the word "time"
- In the type combo select "float"
- In the dimensions fields enter "time", "time" and "float"
- Click on Save and activate the rule
- Once active copy the ID assigned to the rule and replace the 0 below
  with that

Want more info?
---------------

More demos: http://docs.kii.com/en/samples/
Game Cloud Tutorial: http://docs.kii.com/en/samples/Gamecloud-Unity/

Interested in Game Analytics?
-----------------------------

We also offer a dedicated Unity SDK for Game Analytics which you can
download here: http://developer.kii.com/#/sdks
More info: http://documentation.kii.com/en/guides/unity/managing-analytics