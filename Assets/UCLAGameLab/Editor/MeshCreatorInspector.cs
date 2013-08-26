/****************************************************************************
Copyright (c) 2013, Jonathan Cecil and UCLA Game Lab
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/***
* MeshCreatorInspector
*	modifies the inspector to show controls for the Mesh Creator Data component.
*	This script needs to be in the Editor folder of your project along
*	with the SimpleSurfaceEdge.cs, MeshCreator.cs, and the Triangulator.cs script.
*   
***/
[CustomEditor(typeof(MeshCreatorData))]
public class MeshCreatorInspector :  Editor {
	
	private MeshCreatorData mcd;
    private MeshCreatorUndoManager mcud;
	private const float versionNumber = 0.7f;
	private bool showColliderInfo = false;
	private bool showMeshInfo = false;
	private bool showMaterialInfo = false;
	private bool showExperimentalInfo = false;
    private bool showToolInfo = false;

    // enums for mesh and collider type
    private ObjectColliderType colliderType;
    private ObjectMeshType meshType;

	/***
	* OnEnable
	* 	set the MeshCreator when component is added to the object
	***/
	private void OnEnable()
    {
		mcd = target as MeshCreatorData;
		if (mcd == null) {
			Debug.LogError("MeshCreatorInspector::OnEnable(): couldn't find a MeshCreatorData.cs component. Is the file in your project?");
		}
        mcud = new MeshCreatorUndoManager(mcd, "Mesh Creator");
    }
	 
	/***
	* OnInspectorGUI
	*	this does the main display of information in the inspector.
	***/
	public override void OnInspectorGUI() {
        mcud.CheckUndo();

       

		EditorGUIUtility.LookLikeInspector();
		
		// TODO: inspector layout should be redesigned so that it's easier to 
		// see the texture and material information
		if (mcd != null) {

            // determine if we're looking at a scene object or a prefab object
            bool isPrefab = PrefabUtility.GetPrefabType(mcd.gameObject) == PrefabType.Prefab;

            // below is GUI code for normal scene view
            if (!isPrefab)
            {
                EditorGUILayout.LabelField("UCLA Game Lab Mesh Creator");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mesh Creation Outline", "");
                mcd.outlineTexture =
                    EditorGUILayout.ObjectField("Mesh Outline Texture", mcd.outlineTexture, typeof(Texture2D), true) as Texture2D;
                mcd.pixelTransparencyThreshold = EditorGUILayout.Slider("  Pixel Threshold", mcd.pixelTransparencyThreshold, 1.0f, 255.0f);

                EditorGUILayout.Space();
                // what type of object being created, 2d or 3d?
                if (mcd.uvWrapMesh == true)
                {
                    meshType = ObjectMeshType.Full3D;
                }
                else
                {
                    meshType = ObjectMeshType.Flat2D;
                }

                meshType = (ObjectMeshType)EditorGUILayout.EnumPopup("Mesh Type", meshType);
                if (meshType == ObjectMeshType.Full3D)
                {
                    mcd.uvWrapMesh = true;
                }
                else
                {
                    mcd.uvWrapMesh = false;
                }

                //with colliders?
                if (mcd.generateCollider == false)
                {
                    colliderType = ObjectColliderType.None;
                }
                else if (mcd.usePrimitiveCollider == false && mcd.useAABBCollider == false)
                {
                    colliderType = ObjectColliderType.Mesh;
                }
                else if (mcd.usePrimitiveCollider == false && mcd.useAABBCollider == true)
                {
                    colliderType = ObjectColliderType.BoundingBox;
                }
                else
                {
                    colliderType = ObjectColliderType.Boxes;
                }

                colliderType = (ObjectColliderType)EditorGUILayout.EnumPopup("Collider Type", colliderType);
                if (colliderType == ObjectColliderType.None)
                {
                    mcd.generateCollider = false;
                }
                else if (colliderType == ObjectColliderType.Mesh)
                {
                    mcd.generateCollider = true;
                    mcd.usePrimitiveCollider = false;
                    mcd.useAABBCollider = false;
                }
                else if (colliderType == ObjectColliderType.BoundingBox)
                {
                    mcd.generateCollider = true;
                    mcd.usePrimitiveCollider = false;
                    mcd.useAABBCollider = true;
                }
                else // ObjectColliderType.Boxes
                {
                    mcd.generateCollider = true;
                    mcd.usePrimitiveCollider = true;
                    mcd.useAABBCollider = false;
                }

                EditorGUILayout.Space();

                if (mcd.uvWrapMesh) EditorGUILayout.TextArea("A 3d mesh will be created.");
                else
                {
                    if (mcd.createEdges && mcd.createBacksidePlane) EditorGUILayout.TextArea("Flat front and back planes will be created, with a mesh side edge.");
                    else if (mcd.createEdges) EditorGUILayout.TextArea("A flat front plane will be created, with a mesh side edge.");
                    else if (mcd.createBacksidePlane) EditorGUILayout.TextArea("Flat front and back planes will be created.");
                    else EditorGUILayout.TextArea("A flat front plane will be created.");
                }

                EditorGUILayout.Space();
                showMeshInfo = EditorGUILayout.Foldout(showMeshInfo, "Mesh Creation");
                if (showMeshInfo)
                {
                    EditorGUILayout.LabelField("  Mesh id number", mcd.idNumber);
                    if (!mcd.uvWrapMesh)
                    {
                        mcd.createEdges = EditorGUILayout.Toggle("  Create full mesh for edge?", mcd.createEdges);
                        mcd.createBacksidePlane = EditorGUILayout.Toggle("  Create backside plane?", mcd.createBacksidePlane);
                    }
                }

                EditorGUILayout.Space();
                showMaterialInfo = EditorGUILayout.Foldout(showMaterialInfo, "Mesh Materials");
                if (showMaterialInfo)
                {
                    mcd.useAutoGeneratedMaterial = EditorGUILayout.Toggle("  Auto Generate Material?", mcd.useAutoGeneratedMaterial);
                    if (!mcd.useAutoGeneratedMaterial) mcd.frontMaterial =
                        EditorGUILayout.ObjectField("    Use Other Material", mcd.frontMaterial, typeof(Material), true) as Material;
                }

                EditorGUILayout.Space();
                showColliderInfo = EditorGUILayout.Foldout(showColliderInfo, "Collider Creation");
                if (showColliderInfo)
                {
                    if (mcd.generateCollider && mcd.usePrimitiveCollider) mcd.maxNumberBoxes = EditorGUILayout.IntField("  Max Number Boxes", mcd.maxNumberBoxes);
                    if (mcd.generateCollider)
                    {
                        mcd.usePhysicMaterial = EditorGUILayout.Toggle("  Use Physics Material?", mcd.usePhysicMaterial);
                        if (mcd.usePhysicMaterial) mcd.physicMaterial =
                            EditorGUILayout.ObjectField("    Physical Material", mcd.physicMaterial, typeof(PhysicMaterial), true) as PhysicMaterial;
                        mcd.setTriggers = EditorGUILayout.Toggle("  Set Collider Triggers?", mcd.setTriggers);
                    }
                }

                EditorGUILayout.Space();
                showExperimentalInfo = EditorGUILayout.Foldout(showExperimentalInfo, "Advanced");
                if (showExperimentalInfo)
                {
                    EditorGUILayout.LabelField("  Mesh Scale", "");
                    mcd.meshWidth = EditorGUILayout.FloatField("    Width", mcd.meshWidth);
                    mcd.meshHeight = EditorGUILayout.FloatField("    Height", mcd.meshHeight);
                    mcd.meshDepth = EditorGUILayout.FloatField("    Depth", mcd.meshDepth);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("  Edge Smoothing", "");
                    mcd.mergeClosePoints = EditorGUILayout.Toggle("    Merge Close Vertices", mcd.mergeClosePoints);
                    //mcd.mergePercent = EditorGUILayout.FloatField( "Merge Percent Points", mcd.mergePercent);
                    if (mcd.mergeClosePoints) mcd.mergeDistance = EditorGUILayout.FloatField("      Merge Distance (px)", mcd.mergeDistance);
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("  Pivot Position", "");
                    mcd.pivotHeightOffset = EditorGUILayout.FloatField("    Pivot Height Offset", mcd.pivotHeightOffset);
                    mcd.pivotWidthOffset = EditorGUILayout.FloatField("    Pivot Width Offset", mcd.pivotWidthOffset);
                    mcd.pivotDepthOffset = EditorGUILayout.FloatField("    Pivot Depth Offset", mcd.pivotDepthOffset);
                }

                EditorGUILayout.Space();
                if (GUILayout.Button("Update Mesh", GUILayout.MaxWidth(100)))
                {
                    // set entire scene for undo, object only won't work cause we're adding and removing components
                    Undo.RegisterSceneUndo("Update Mesh Creator Object");

                    // do some simple parameter checking here so we don't get into trouble
                    if (mcd.maxNumberBoxes < 1)
                    {
                        Debug.LogWarning("Mesh Creator: minimum number of boxes should be one or more. Setting to 1 and continuing.");
                    }
                    else
                    {
                        MeshCreator.UpdateMesh(mcd.gameObject);
                    }
                }
                showToolInfo = EditorGUILayout.Foldout(showToolInfo, "Mesh Creator Info");
                if (showToolInfo)
                {

                    EditorGUILayout.LabelField("  Mesh Creator Data", "version " + MeshCreatorData.versionNumber.ToString());
                    EditorGUILayout.LabelField("  Mesh Creator Editor", "version " + versionNumber.ToString());
                    EditorGUILayout.LabelField("  Mesh Creator", "version " + MeshCreator.versionNumber.ToString());

                }
            } // end normal scene GUI code
            else // begin prefab inspector GUI code
            {
                EditorGUILayout.LabelField("UCLA Game Lab Mesh Creator");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mesh Creator must be used in a scene.");
                EditorGUILayout.LabelField("To manipulate Mesh Creator Data and update this prefab,");
                EditorGUILayout.LabelField("pull it into a scene, update Mesh Creator, and apply your");
                EditorGUILayout.LabelField("changes to the prefab.");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mesh Creation Outline", "");
                EditorGUILayout.ObjectField("Mesh Outline Texture", mcd.outlineTexture, typeof(Texture2D), true);
                EditorGUILayout.LabelField("  Pixel Threshold", mcd.pixelTransparencyThreshold.ToString() );

                EditorGUILayout.Space();
                // what type of object being created, 2d or 3d?
                if (mcd.uvWrapMesh == true)
                {
                    meshType = ObjectMeshType.Full3D;
                }
                else
                {
                    meshType = ObjectMeshType.Flat2D;
                }

                EditorGUILayout.LabelField("Mesh Type", meshType.ToString());
                

                //with colliders?
                if (mcd.generateCollider == false)
                {
                    colliderType = ObjectColliderType.None;
                }
                else if (mcd.usePrimitiveCollider == false && mcd.useAABBCollider == false)
                {
                    colliderType = ObjectColliderType.Mesh;
                }
                else if (mcd.usePrimitiveCollider == false && mcd.useAABBCollider == true)
                {
                    colliderType = ObjectColliderType.BoundingBox;
                }
                else
                {
                    colliderType = ObjectColliderType.Boxes;
                }

                EditorGUILayout.LabelField("Collider Type", colliderType.ToString());
                

                EditorGUILayout.Space();

                if (mcd.uvWrapMesh) EditorGUILayout.TextArea("A 3d mesh will be created.");
                else
                {
                    if (mcd.createEdges && mcd.createBacksidePlane) EditorGUILayout.TextArea("Flat front and back planes will be created, with a mesh side edge.");
                    else if (mcd.createEdges) EditorGUILayout.TextArea("A flat front plane will be created, with a mesh side edge.");
                    else if (mcd.createBacksidePlane) EditorGUILayout.TextArea("Flat front and back planes will be created.");
                    else EditorGUILayout.TextArea("A flat front plane will be created.");
                }

                EditorGUILayout.Space();
                showMeshInfo = EditorGUILayout.Foldout(showMeshInfo, "Mesh Creation");
                if (showMeshInfo)
                {
                    EditorGUILayout.LabelField("  Mesh id number", mcd.idNumber);
                    if (!mcd.uvWrapMesh)
                    {
                        EditorGUILayout.Toggle("  Create full mesh for edge?", mcd.createEdges);
                        EditorGUILayout.Toggle("  Create backside plane?", mcd.createBacksidePlane);
                    }
                }

                EditorGUILayout.Space();
                showMaterialInfo = EditorGUILayout.Foldout(showMaterialInfo, "Mesh Materials");
                if (showMaterialInfo)
                {
                    EditorGUILayout.Toggle("  Auto Generate Material?", mcd.useAutoGeneratedMaterial);
                    if (!mcd.useAutoGeneratedMaterial)
                        EditorGUILayout.ObjectField("    Use Other Material", mcd.frontMaterial, typeof(Material), true);


                }

                EditorGUILayout.Space();
                showColliderInfo = EditorGUILayout.Foldout(showColliderInfo, "Collider Creation");
                if (showColliderInfo)
                {
                    if (mcd.generateCollider && mcd.usePrimitiveCollider) EditorGUILayout.LabelField("  Max Number Boxes", mcd.maxNumberBoxes.ToString());
                    if (mcd.generateCollider)
                    {
                        EditorGUILayout.Toggle("  Use Physics Material?", mcd.usePhysicMaterial);
                        if (mcd.usePhysicMaterial) 
                            EditorGUILayout.ObjectField("    Physical Material", mcd.physicMaterial, typeof(PhysicMaterial), true);
                        EditorGUILayout.Toggle("  Set Collider Triggers?", mcd.setTriggers);
                    }
                }

                EditorGUILayout.Space();
                showExperimentalInfo = EditorGUILayout.Foldout(showExperimentalInfo, "Advanced");
                if (showExperimentalInfo)
                {
                    EditorGUILayout.LabelField("  Mesh Scale", "");
                    EditorGUILayout.LabelField("    Width", mcd.meshWidth.ToString());
                    EditorGUILayout.LabelField("    Height", mcd.meshHeight.ToString());
                    EditorGUILayout.LabelField("    Depth", mcd.meshDepth.ToString());
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("  Edge Smoothing", "");
                    EditorGUILayout.Toggle("    Merge Close Vertices", mcd.mergeClosePoints);
                    if (mcd.mergeClosePoints) EditorGUILayout.LabelField("      Merge Distance (px)", mcd.mergeDistance.ToString());
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("  Pivot Position", "");
                    EditorGUILayout.LabelField("    Pivot Height Offset", mcd.pivotHeightOffset.ToString());
                    EditorGUILayout.LabelField("    Pivot Width Offset", mcd.pivotWidthOffset.ToString());
                    EditorGUILayout.LabelField("    Pivot Depth Offset", mcd.pivotDepthOffset.ToString());
                }

                showToolInfo = EditorGUILayout.Foldout(showToolInfo, "Mesh Creator Info");
                if (showToolInfo)
                {

                    EditorGUILayout.LabelField("  Mesh Creator Data", "version " + MeshCreatorData.versionNumber.ToString());
                    EditorGUILayout.LabelField("  Mesh Creator Editor", "version " + versionNumber.ToString());
                    EditorGUILayout.LabelField("  Mesh Creator", "version " + MeshCreator.versionNumber.ToString());

                }
            } // end prefab inspector GUI code
          
		}
		else {
			Debug.LogError("MeshCreatorInspector::OnInspectorGUI(): couldn't find a MeshCreatorData component. Something has gone horribly wrong, try reloading your scene.");
		}
        mcud.CheckDirty();
	}
	
}

