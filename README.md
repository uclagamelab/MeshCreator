# UCLA Game Lab
# Mesh Creator

Create meshes from alpha image textures in Unity. Mesh Creator has editor scripts that use the alpha channel in textures to build a mesh and colliders in your scene. This allows simple creation of art assets without the need for external 3d software.

Major releases are available on the Unity Asset store here: xxxxxx. This is our developement repository, use at your own peril. Major release packages are kept here in the MeshCreator.Packages directory. Those packages may not be compatible with files in the rest of this repo. Also, don't miss the AlphaUtilityAdditions Photoshop action in the OtherFiles directory.

The MeshCreatorOverview pdf included here gives a brief overview of some features of this tool.

Or see a tutorial at:
http://games.ucla.edu/resources/unity-mesh-creator/

For license information, see license.txt.

Jonathan Cecil
jonathancecil@arts.ucla.edu
http://jonathancecil.com
http://games.ucla.edu/



**** Versions ****

Version 0.7
August 16, 2013
Changes:
- Streamlined wizard and inspector layouts for easier usablity.
- Added pixel opacity threshold for edge smoothing.
- Changed from max box size(confusing) to maximum number of box colliders.
- Added Bounding Box box collider option.
- Remove rigidbody adding features.
- Added ability to set triggers on all created box colliders.
- Saved meshes now visible as Unity assets.
- Compatiblity with Unity 4.2
- Many bug fixes.
Things to come:
- Circle packing for cylinder colliders.
- Deal properly with alpha holes in images.
- Cleaner edge creation and smoothing.
- Automatic old mesh asset cleanup tool.
- Runtime mesh creation. :)

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
