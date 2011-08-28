UCLA Game Lab
Mesh Creator

Create simple meshes from image textures. Add a MeshCreatorData component to an empty game object and select options. Use "Update Mesh" button in the inspector to generate mesh. The Editor scripts also need to be included in your project.

For best looking results, uncheck the "Use UV mapped mesh" box.

See the unity project for example, or import the package MeshCreator.04 in MeshCreator.Package to an existing project.

Really basic instructions:
Make an empty game object and put the MeshCreatorData script on it in the inspector.
Assign a texture with transparency to the "Mesh Outline Texture" area of the script.
Press the "Update Mesh" button to create a mesh around the opaque portions of your image.

or see a tutorial at:
http://games.ucla.edu/resources/unity-mesh-creator/

Hint: use images with solid areas of opaque pixels. The simple triangulation algorithm doesn't like holes.

This software doesn't have a specific license yet. I'm open to suggestions. Don't worry. Be happy. 

jonathancecil@ucla.edu

**** Versions ****

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
