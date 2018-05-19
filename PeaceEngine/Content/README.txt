Peace Engine Content Folder

This folder is where you should place content that should be bundled with the engine.
Normally this stuff would be in a MonoGame Content Builder project, however MonoGame 3.6 doesn't
allow multiple MGCB projects in one solution, possibly to prevent file conflicts.

So just use the Pipeline Tool to compile anything you need then add the raw .xnb files directly
to this folder and tell Visual Studio/MonoDevelop to "Copy if newer".