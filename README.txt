MeshCreatorDev is the internal development copy of the Mesh Creator for the game lab. Public releases are put on Github at https://github.com/uclagamelab/MeshCreator


BUGS:
FIXED 8/23/13 - triangulation is always run, even if no mesh is required.
- calling undo on wizard created object does not remove new mesh from hierarchy.
FIXED 8/23/13 - update mass error when using Flat2d and mesh collider. do collider first, then
the render mesh.
- switching between 2d and 3d mesh in inspector does not change opacity of the material. materials should be present for opaque and transparent. should switch out between the two.
FIXED 8/22/13 - updating a prefab in project causes tons of errors. - RESULT - made inspector read only for prefabs.
FIXED 8/12/13 - collider mesh not being saved when front plane is used.
FIXED 8/12/13 - generative meshes not seen by unity as meshes(change extension to .asset)
FIXED 8/9/13 - rigid body added when generate collider set to off
FIXED 8/9/13 - when generate collider set to off, colliders should be removed
FIXED 8/9/13 - when create edge and create backside unchecked, those subobjects should be removed
FIXED 8/9/13 - changing to collider type none in inspector should remove colliders on update
FIXED 8/2/13 - Object.FindSceneObjectsOfType is obsolete warning
FIXED 8/2/13 - wizard texture box should be larger and square
FIXED 8/2/13 - small window size for wizard

Version 0.7 TODO:
- asset store documentation
- rewrite warnings to explain solutions.
- great images for front page
- move BSD license to header of each script file
DONE 8/21/13 - add undo
DONE 8/9/13 - set version number to 0.7 in all scripts
DONE 8/12/13 - add scale option back, under "advanced" tab
DONE 8/9/13 - Bounding Box collider for top level
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
- store inspector GUI variables in MeshCreatorData
- replace Mesh Creator Info section with a simpler message:
	- all scripts version XX
	- scripts are wrong, versions XX and XX present.
- separate out the adobe actions from the repository
- user should be able to configure location of saved meshes and materials.
	- use http://docs.unity3d.com/Documentation/ScriptReference/PreferenceItem.html
	- add mesh and material locations
- circle packing for cylinder colliders
- deal with holes and islands
	- holes in mesh via poly2tri
- decimate mesh nicely - merging points improvement, but clean
- more examples: ragdoll, multi-image characters, micro platformer?, skeletal?
- runtime support, would be fucking amazing
	- would need the thread library
- mesh asset cleanup tool
	- search through scene and rectify filenames in Meshes folder
- cleanup of mesh creator data, breaks backward compatibility
	- enums for mesh type, collider type
- output island and hole information
- full code cleanup and comments
- base materials should be dynamically generated in code

	
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
- convert collider from none->BB,none->mesh,none->boxes
- convert collider from boxes->BB,boxes->none,boxes->mesh
- convert collider from BB->boxes,BB->none,BB->mesh
- convert collider from mesh->none,mesh->BB,mesh->boxes
- import non-square image

