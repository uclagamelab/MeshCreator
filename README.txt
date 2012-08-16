UCLA Game Lab
Mesh Creator

Create simple meshes from alpha image textures in Unity. Mesh Creator has editor scripts that use the alpha channel in textures to build a mesh and colliders in your scene.



There are two methods of mesh creation supported:
1. Simple Method. In GameObject menu, select Create Mesh Creator. In popup window, select the texture to use as a basis for mesh, flat or extruded for depth, and with colliders or without. Click the Make New Mesh button to create a new game object. Mesh can be updated in the inspector for more complicated changes.
2. Advanced Method. Create a new empty game object, attach the MeshCreatorData component found the scripts folder, and assign the desired texture to use as the basis for the mesh. Tweak the parameters to your heart's content. Click the Update Mesh button to create the object. I recommend using method #1 and updating after the object is created...



Images used with this tool need to have transparency and areas of highest opacity. PNGs work well. For those concerned about quality, Photoshop documents seem to work really well. I've included a couple Photoshop actions I use to speed up creation and updating of textures. See the OtherFiles directory.

Hint: use images with solid areas of opaque pixels. The simple triangulation algorithm used by these scripts doesn't like holes.



To use Mesh Creator in your project, import the Unity package MeshCreator.061.unitypackage with Assets->Import Package->Custom Package... The package will import the required assets in their correct locations. There are two folder locations you can not change: Editor and Meshes both have to stay in your folder hierarchy where they are imported.

The MeshCreatorOverview pdf included here gives a good introduction to some features of this tool.

For a sample scene with fully created objects, import the package MeshCreator.061.SampleScene.unitypackage

Or see a tutorial at(looks different, but idea is the same):
http://games.ucla.edu/resources/unity-mesh-creator/



For license information, see license.txt.

Jonathan Cecil
jonathancecil@arts.ucla.edu
http://jonathancecil.com
http://games.ucla.edu/



**** Versions ****

Version 0.61
August 16, 2012
Changes:
- Improve interface and layout in wizard window and mesh data inspector.
- Move Merge Clost Points option out of Experimental Tab into Mesh Options tab.
- Very minor code cleanup.
- Fixed bug in the included Photoshop actions.
Things to come:
- Same as before.
...plus, scaling lock for mesh sizes in editor.

Version 0.6
- Added "smoothing" feature called Merge Close Points in the experimental tab. Use it to combine points within a configurable distance. Nice for large textures.
- Mesh Creator Wizard looks nicer when you choose Game Object->Create Mesh Object. Thanks to Chris Reilly.
Things to come:
- Change default primitive collider to capsule, with option to change to change to box collider...but this could be a difficult circle packing problem.
- Support holes in mesh: with poly2tri library maybe?
- More code cleanup. It's still very difficult to read the mesh creation code chunks. I don't really know what I did.

Version 0.5
Changes:
- Added a GameObject menu item to create new object with simple combination of presets: flat vs depth, with colliders or without, and mesh size. 
- Meshes save less frequently. When a game object is copied and then updated, script checks other objects with the the Mesh Creator Data component.
- Advanced options are hidden in inspector window.
- Did a little code cleanup and slightly improved comments.
- Added version numbers to script files. Some display in the inspector.
- Cleaned up repo to not include Library folder and some other older assets.
Things to come:
- Add Game Lab logo and link to the wizard window.
- Change default primitive collider to capsule, with option to change to change to box collider...but this could be a difficult circle packing problem.
- Support holes in mesh: with poly2tri library maybe?
- More code cleanup. It's still very difficult to read the mesh creation code chunks. I don't really know what I did.

Version 0.4
August 27, 2011
Added box to add rigid body to object, should simplfy workflow a little. Included a few extra files in Library to help Unity recreate the project after unpacking the zip file; should be working again for PC and Mac.
Major Issues:
- not convinced saving all the meshes in the mesh directory is the best approach....waiting for inspiration, or help.
- code is still ugly

Version 0.3
June 25, 2011
Mesh Creator got a little simpler. Meshes are saved automagically to prevent duplicate mesh destruction. This pollutes the meshes folder, but is better than meshes disappearing. Also gone is the position offset section of the inspector interface.
Major Issues:
- this code is still really ugly


Version 0.2
June 18, 2011
Mesh Creator is fairly useable. Go for it.
Major Issues:
- Code is poorly commented and redundant in some areas. A cleanup is in order.
Minor Issues:
- If a edge mesh is created from an alpha image, sometimes the texture on that area is transparent. Creating a new texture for the edge mesh might be a way to solve this problem.

Version 0.1
May 23, 2011
Major Issues:
- The primitive fill algorithm is super slow, I'm sure there are some nice optimizations that will make life easier.
- Option for sprite type textures is needed, maybe just by adding a plane instead of creating a mesh.



Thanks to Chris Reilly for help with wizard interface.
Thanks to Aliah Darke for usability and interface testing.
