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
- AABB collider for top level
- change max box size to max number box colliders
- output island and hole information
- full code cleanup and comments
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

