MeshCreatorDev is the internal development copy of the Mesh Creator for the game lab. Public releases are put on Github at https://github.com/uclagamelab/MeshCreator


BUGS:
- rigid body added when generate collider set to off
- when generate collider set to off, colliders should be removed
- when create edge and create backside unchecked, those subobjects should be removed
- changing to collider type none in inspector should remove colliders on update
FIXED 8/2/13 - Object.FindSceneObjectsOfType is obsolete warning
FIXED 8/2/13 - wizard texture box should be larger and square
FIXED 8/2/13 - small window size for wizard

TODO:
- set version number to 0.7 in all scripts

- full code cleanup and comments
- base materials should be dynamically generated in code
DONE 8/9/13 - AABB collider for top level
DONE 8/9/13 - change max box size to max number box colliders
DONE 8/2/13 - dropdown list for 2d vs 3d object creation in inspector
DONE 8/2/13 - dropdown list for collider selection in inspector
DONE 8/2/13 - threshold for pixel on slider in the inspector
DONE 8/2/13 - turn off rigidbody by default
DONE 8/2/13 - add trigger checkbox for colliders
DONE 8/2/13 - update version info in inspector
DONE 8/2/13 - dropdown list for collider selection in wizard
DONE 8/2/13 - dropdown list for 2d vs 3d object creation in wizard
DONE 8/2/13 - remove sizing in wizard and inspector
DONE - game lab logo and link - branding

Version 1.0 features
- circle packing for cylinder colliders
- deal with holes and islands
	- holes in mesh via poly2tri
- decimate mesh nicely - merging points, but clean
- more examples: ragdoll, multi-image characters, micro platformer?
- runtime support, would be fucking amazing
	- would need the thread library
- mesh asset cleanup tool
	- search through scene and rectify filenames in Meshes folder
- cleanup of mesh creator data, breaks backward compatibility
	- enums for mesh type, collider type
- add scale option back, under "advanced" tab
- output island and hole information

	
Unit Tests
- new png in wizard
- new png in inpspector
- new psd in wizard
- new psd in inspector
- update png in wizard
- update png in inpspector
- update psd in wizard
- update psd in inspector
- prefabs across scenes
- convert collider from none->AABB,none->mesh,none->

