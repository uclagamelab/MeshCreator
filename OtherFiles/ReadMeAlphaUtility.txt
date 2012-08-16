The alpha utility actions provided on the Unity website are great, but I got sick of repeating the same steps over and over to create each texture. I made a new Photoshop action to do that for me.

Import the actions into Photoshop by choosing the Action window->menu->load actions. My actions require the use of Unity's downloadable actions described here:

http://unity3d.com/support/documentation/Manual/HOWTO-alphamaps.html

and downloaded from here:
http://unity3d.com/support/documentation/Images/manual/AlphaUtility.atn.zip

The CovertToUnityPSD action expects there to be one layer to your image, with transparency, and the layer should be named 'texture'. This action takes care of all the defringing and background layering and alpha channel addition.

There is a second script for updating an existing file: UpdateUnityPSD. It expects a file with the layer structure created by the first script. You can edit the texture layer, then run the action to update everything. Some crappiness can happen when you don't have a channel in your psd name 'Alpha 1'. If you don't, just delete all the channels and run the UpdateUnityPSD to fix it.