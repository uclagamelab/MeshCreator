UCLA Game Lab
Mesh Creator

Create simple meshes from image textures. Add a MeshCreatorData component to an empty game object and select options. Use "Update Mesh" button in the inspector to generate mesh.

See the unity project for example, or import the package in MeshCreator.01 to an existing project.

This software doesn't have a specific license yet. Don't worry. Be happy.

jonathancecil@ucla.edu

**** Versions ****

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
