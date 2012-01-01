UCLA Game Lab
Mesh Creator

Create simple meshes from alpha image textures in Unity. Mesh Creator has editor scripts that use the alpha channel in textures to build a mesh and colliders in your scene.



There are two methods of mesh creation supported:
1. Simple Method. In GameObject menu, select Create Mesh Creator. In popup window, select the texture to use as a basis for mesh, flat or extruded for depth, and with colliders or without. Click the Make New Mesh button to create a new game object. Mesh can be updated in the inspector for more complicated changes.
2. Advanced Method. Create a new empty game object, attach the MeshCreatorData component found the scripts folder, and assign the desired texture to use as the basis for the mesh. Click the Update Mesh button to create the object.



To use Mesh Creator in your project, import the Unity package MeshCreator.05 with Assets->Import Package->Custom Package... The package will import the required assets in their correct locations. There are two folder locations you can not change: Editor and Meshes both have to stay in your folder hierarchy where they are imported.

For a sample scene with fully created objects, import the package MeshCreator.05.sample.unity.

Or see a tutorial at(looks different, but idea is the same):
http://games.ucla.edu/resources/unity-mesh-creator/

Hint: use images with solid areas of opaque pixels. The simple triangulation algorithm doesn't like holes.

This software doesn't have a specific license yet. I'm open to suggestions. Don't worry. Be happy. 

jonathancecil@arts.ucla.edu



**** Versions ****

Version 0.5

Changes:
- Added a GameObject menu item to create new object with simple combination of presets: flat vs depth, with colliders or without, and mesh size. 
- Meshes save less frequently. When a game object is copied and then updated, script checks other objects with the the Mesh Creator Data component.
- Advanced options are hidden in inspector window.
- Did a little code cleanup and slightly improved comments.
- Added version numbers to script files. They display in the inspector.
- Cleaned up repo to not include Library folder.

Things to come:
- Add Game Lab logo and link to inspector window.
- Change default primitive collider to capsule, with option to change to change to box collider. This could be a difficult circle packing problem.
- Support holes in mesh with poly2tri library.
- More code cleanup.

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
