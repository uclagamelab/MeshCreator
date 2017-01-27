/****************************************************************************
Copyright (c) 2017, Jonathan Cecil and UCLA Game Lab
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

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MeshCreator : UnityEngine.Object {
	public static float versionNumber = 0.8f;
	
    public static void UpdateMesh(GameObject gameObject, bool updatingExistingGameObject = true)
	{
        if (updatingExistingGameObject)
        {
            Undo.SetCurrentGroupName("Update Mesh");
        }

		MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;
		
		// unity should prevent this from happening to the inspector, but just in case.....
		if (mcd == null) {
			Debug.LogError("MeshCreator Error: selected object does not have a MeshCreatorData component. Select an object with a MeshCreatorData component to update.");
			return;
		}
			
		// add a TextureImporter object here to check whether texture is readable
		// set it to readable if necessary
		if (mcd.outlineTexture == null) {
			Debug.LogError("MeshCreator: no texture found. Make sure to have a texture selected before updating mesh.");
			return;
		}
		
		// if this is a new save, generate a unique idNumber
		if ( mcd.idNumber == "" )
		{
			mcd.idNumber = MeshCreator.GenerateId();
			// Debug.Log(mcd.gameObject.name + "MeshCreator: set new mesh id number to " + mcd.idNumber);
		}
		
		// check the id number, if it is used in another scene object
        // generate a new id number
		while (MeshCreator.IdExistsInScene(mcd))
		{
			mcd.idNumber = MeshCreator.GenerateId(); 
		}
		
		// check for scene folder
        string[] sceneNames = SceneManager.GetActiveScene().path.Split('/');//EditorApplication.currentScene.Split('/');
		if (sceneNames.Length == 1 && sceneNames[0] == "")
		{
			Debug.LogError("MeshCreator Error: please save the scene before creating a mesh.");
			DestroyImmediate(mcd.gameObject);
			return;
		}
		string sceneName = sceneNames[sceneNames.Length-1];
		string folderName = sceneName.Substring(0, sceneName.Length - 6);
		string folderPath = "Assets/UCLAGameLab/Meshes/" + folderName; // TODO: this should be a preference

        if (!Directory.Exists("Assets/UCLAGameLab/Meshes"))
        {
            if (!Directory.Exists("Assets/UCLAGameLab"))
            {
                Debug.LogError("MeshCreator: UCLAGameLab folder is missing from your project, please reinstall Mesh Creator.");
                return;
            }
            AssetDatabase.CreateFolder("Assets/UCLAGameLab", "Meshes");
            Debug.Log("MeshCreator: making new Meshes folder at Assets/Meshes");
        }

		if (!Directory.Exists(folderPath))
		{
			Debug.Log("MeshCreator: making new folder in Meshes folder at " + folderPath);
			AssetDatabase.CreateFolder("Assets/UCLAGameLab/Meshes", folderName );
		}
		
		string saveName = folderName + "/" + mcd.gameObject.name + "." + mcd.idNumber ;
			
		// stash the rotation value, set back to identity, then switch back later
		Quaternion oldRotation = mcd.gameObject.transform.rotation;
		mcd.gameObject.transform.rotation = Quaternion.identity;
				
		// stash the scale value, set back to one, then switch back later
		Vector3 oldScale = mcd.gameObject.transform.localScale;
		mcd.gameObject.transform.localScale = Vector3.one;
		
		// transform the object if needed to account for the new pivot
		if (mcd.pivotHeightOffset != mcd.lastPivotOffset.x || mcd.pivotWidthOffset != mcd.lastPivotOffset.y || mcd.pivotWidthOffset != mcd.lastPivotOffset.z ) {
			mcd.gameObject.transform.localPosition -= mcd.lastPivotOffset;
			mcd.lastPivotOffset = new Vector3(mcd.pivotWidthOffset, mcd.pivotHeightOffset, mcd.pivotDepthOffset);
			mcd.gameObject.transform.localPosition += mcd.lastPivotOffset;
		}

        // 
        // start mesh renderer setup section
        //
		
        // mesh for rendering the object
        // will either be flat or full mesh
		Mesh msh = new Mesh();

        // collider for mesh, if used
		Mesh collidermesh = new Mesh();
		if (mcd.uvWrapMesh) {
			// Set up game object with mesh;
			AssignMesh(gameObject, ref msh);
			collidermesh = msh;
		}
		else {
			AssignPlaneMesh(gameObject, ref msh);
            // if needed, create the 3d mesh collider
            if (mcd.generateCollider && !mcd.usePrimitiveCollider && !mcd.useAABBCollider)
    			AssignMesh(gameObject, ref collidermesh);
		}
			
		MeshRenderer mr = (MeshRenderer) mcd.gameObject.GetComponent("MeshRenderer");
		if (mr == null) {
			//Debug.Log("MeshCreator Warning: no mesh renderer found on update object, adding one.");
			mcd.gameObject.AddComponent(typeof(MeshRenderer));
		}
			
		// update the front material via renderer
		Material meshmat;
        string materialNameLocation = "Assets/UCLAGameLab/Materials/"+mcd.outlineTexture.name+".material.mat";
        string transparentMaterialNameLocation = "Assets/UCLAGameLab/Materials/"+mcd.outlineTexture.name+".trans.material.mat";

        string baseMaterialNameLocation = "Assets/UCLAGameLab/Materials/baseMaterial.mat";
        string transparentBaseMaterialNameLocation = "Assets/UCLAGameLab/Materials/baseTransparentMaterial.mat";

		if (mcd.useAutoGeneratedMaterial) {
			// if using uvWrapMesh, use regular material
			if (mcd.uvWrapMesh) {
				meshmat = (Material) AssetDatabase.LoadAssetAtPath(materialNameLocation, typeof(Material));
				if (meshmat == null) {
					meshmat = CopyTexture(baseMaterialNameLocation, materialNameLocation, mcd.outlineTexture);
				}
				mcd.gameObject.GetComponent<Renderer>().sharedMaterial = meshmat;
			}
			else { // use a transparent material
				meshmat = (Material) AssetDatabase.LoadAssetAtPath(transparentMaterialNameLocation, typeof(Material));
				if (meshmat == null) {
					meshmat = CopyTexture(transparentBaseMaterialNameLocation, transparentMaterialNameLocation, mcd.outlineTexture); 
				}
				mcd.gameObject.GetComponent<Renderer>().sharedMaterial = meshmat;
			}
		}
		else {
			mcd.gameObject.GetComponent<Renderer>().sharedMaterial = mcd.frontMaterial;
		}
			
		MeshFilter mf = (MeshFilter) mcd.gameObject.GetComponent("MeshFilter");
		if (mf == null) {
			//Debug.LogWarning("MeshCreator Warning: no mesh filter found on update object, adding one.");
			mf= mcd.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		}
        
		mf.sharedMesh = msh;

        // save the main mesh
		string meshName = "Assets/UCLAGameLab/Meshes/" + saveName + ".asset";

        AssetDatabase.CreateAsset(msh, meshName);

  
        	

		// make the side edges
		if (!mcd.uvWrapMesh && mcd.createEdges) 
        {
			Mesh edgemesh = new Mesh();
			MeshCreator.AssignEdgeMesh(gameObject, ref edgemesh);
				
			// remove the old backside mesh game object
			string edgeName = mcd.gameObject.name + ".edge";
			ArrayList destroyObject = new ArrayList();
			foreach (Transform child in mcd.gameObject.transform) {
				if (child.name == edgeName) {
					MeshFilter emf = (MeshFilter) child.gameObject.GetComponent("MeshFilter");
					if (emf != null) {
						Mesh ems = (Mesh) emf.sharedMesh;
						if (ems != null) {
							//DestroyImmediate(ems, true);
						}
					}
					destroyObject.Add(child);
				}
			}
				
			while (destroyObject.Count > 0) {
				Transform child = (Transform) destroyObject[0];
				destroyObject.Remove(child);
                Undo.DestroyObjectImmediate(child.gameObject);
			}
				
			// create a new game object to attach the backside plane
			GameObject edgeObject = new GameObject();
			edgeObject.transform.parent = mcd.gameObject.transform;
			edgeObject.transform.localPosition = Vector3.zero;
			edgeObject.transform.rotation = Quaternion.identity;
			edgeObject.name = edgeName;
			MeshFilter edgemf = (MeshFilter) edgeObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
			edgemf.sharedMesh = edgemesh;

			// save the mesh in the Assets folder
			string edgeMeshName = "Assets/UCLAGameLab/Meshes/" + saveName + ".Edge" + ".asset";
			AssetDatabase.CreateAsset(edgemesh, edgeMeshName);
				
			MeshRenderer edgemr = edgeObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

            // for side meshes use the opaque material
            Material edgematerial = (Material)AssetDatabase.LoadAssetAtPath(materialNameLocation, typeof(Material));
            if (edgematerial == null)
            {
                edgematerial = CopyTexture(baseMaterialNameLocation, materialNameLocation, mcd.outlineTexture);
            }
			edgemr.GetComponent<Renderer>().sharedMaterial = edgematerial;
		}
		else // destroy the old edge objects because they're not needed
        {
			string edgeName = mcd.gameObject.name + ".edge";
			ArrayList destroyObject = new ArrayList();
			foreach (Transform child in mcd.gameObject.transform) {
				if (child.name == edgeName) {
					destroyObject.Add(child);
					MeshFilter emf = (MeshFilter) child.gameObject.GetComponent("MeshFilter");
					if (emf != null) {
						Mesh ems = (Mesh) emf.sharedMesh;
						if (ems != null) {
							//DestroyImmediate(ems, true);
						}
					}
				}
			}
			while (destroyObject.Count > 0) {
				Transform child = (Transform) destroyObject[0];
				destroyObject.Remove(child);
                Undo.DestroyObjectImmediate(child.gameObject);
			}
		}
		
        // make the backside plane
		if (!mcd.uvWrapMesh && mcd.createBacksidePlane) {
			Mesh backmesh = new Mesh();
			AssignPlaneMeshBackside(gameObject, ref backmesh);
				
			// remove the old backside mesh game object
			string backsideName = mcd.gameObject.name + ".backside";
			ArrayList destroyObject = new ArrayList();
			foreach (Transform child in mcd.gameObject.transform) {
				if (child.name == backsideName) {
					destroyObject.Add(child);
					MeshFilter emf = (MeshFilter) child.gameObject.GetComponent("MeshFilter");
					if (emf != null) {
						Mesh ems = (Mesh) emf.sharedMesh;
						if (ems != null) {
							//DestroyImmediate(ems, true);
						}
					}
				}
			}
			
			while (destroyObject.Count > 0) {
				Transform child = (Transform) destroyObject[0];
				destroyObject.Remove(child);
                Undo.DestroyObjectImmediate(child.gameObject);
			}
				
			// create a new game object to attach the backside plane
			GameObject backsideObject = new GameObject();
			backsideObject.transform.parent = mcd.gameObject.transform;
			backsideObject.transform.localPosition = Vector3.zero;
			backsideObject.transform.rotation = Quaternion.identity;
			backsideObject.name = backsideName;
			MeshFilter backmf = (MeshFilter) backsideObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
			backmf.sharedMesh = backmesh;
			// save the mesh in the Assets folder
			string backMeshName = "Assets/UCLAGameLab/Meshes/" + saveName + ".Back" + ".asset";
			AssetDatabase.CreateAsset(backmesh, backMeshName);
				
			MeshRenderer backmr = backsideObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

            // for backside plane, use the transparent material
            Material backmaterial = (Material)AssetDatabase.LoadAssetAtPath(transparentMaterialNameLocation, typeof(Material));
            if (backmaterial == null)
            {
                backmaterial = CopyTexture(transparentBaseMaterialNameLocation, transparentMaterialNameLocation, mcd.outlineTexture);
            }
			backmr.GetComponent<Renderer>().sharedMaterial = backmaterial;
		}
        else // remove the old backside mesh game object because it's not needed
        {
			string backsideName = mcd.gameObject.name + ".backside";
			ArrayList destroyObject = new ArrayList();
			foreach (Transform child in mcd.gameObject.transform) {
				if (child.name == backsideName) {
					destroyObject.Add(child);
					// get rid of the old mesh from the assets
					MeshFilter emf = (MeshFilter) child.gameObject.GetComponent("MeshFilter");
					if (emf != null) {
						Mesh ems = (Mesh) emf.sharedMesh;
						if (ems != null) {
							//DestroyImmediate(ems, true);
						}
					}
				}
			}
				
			while (destroyObject.Count > 0) {
				Transform child = (Transform) destroyObject[0];
				destroyObject.Remove(child);
                Undo.DestroyObjectImmediate(child.gameObject);
			}
		}
        // end mesh renderer setup section

        //
        // start collider setup section
		//

		// generate a mesh collider
		if (mcd.generateCollider && !mcd.usePrimitiveCollider && !mcd.useAABBCollider) {
			
			// remove the old compound collider before assigning new
			string compoundColliderName = mcd.gameObject.name + "CompoundColliders";
			foreach(Transform child in mcd.gameObject.transform) {
				if (child.name == compoundColliderName) {
                    
                    Undo.DestroyObjectImmediate(child.gameObject);
				}
			}
			
            // if the current mesh on the mesh renderer is flat
            // and the object has a rigidbody, unity will give an
            // error trying to update the mass.
            // the fix is to stash the current mesh, switch to the
            // full 3d version, and switch back
            if ( !mcd.uvWrapMesh )
            {
                mf.mesh = collidermesh;
            }
			Collider col = mcd.gameObject.GetComponent<Collider>();
            if (col != null)
            {
                Undo.DestroyObjectImmediate(col);
            }

            Undo.AddComponent<MeshCollider>(mcd.gameObject);
			
			MeshCollider mcol = mcd.gameObject.GetComponent("MeshCollider") as MeshCollider;
			if (mcol == null) 
            {
				Debug.LogWarning("MeshCreator: found a non-Mesh collider on object to update. If you really want a new collider generated, remove the old one and update the object with MeshCreator again.");
			}
			else 
            {
				mcol.sharedMesh = collidermesh;
                // save the collider mesh if necessary
                if (!mcd.uvWrapMesh) // if uvWrapMesh, then mesh already saved
                {
                    string colliderMeshName = "Assets/UCLAGameLab/Meshes/" + saveName + ".collider.asset";
                    AssetDatabase.CreateAsset(collidermesh, colliderMeshName);
                }
			}

            // switch mesh filter back if the flat one was
            // swapped out previously
            if (!mcd.uvWrapMesh)
            {
                mf.mesh = msh;
            }

			if (mcd.usePhysicMaterial) 
            {
				mcol.material = mcd.physicMaterial;
			}

            // set triggers for the mesh collider?
            if (mcd.setTriggers)
            {
                mcol.isTrigger = true;
            }
            else
            {
                mcol.isTrigger = false;
            }
		} // end generate mesh collider

        // generate box colliders
		else if (mcd.generateCollider && mcd.usePrimitiveCollider && !mcd.useAABBCollider) {
			// remove the old collider if necessary
			Collider col = mcd.gameObject.GetComponent<Collider>();
			if (col != null) {
                if (col.GetType() == typeof(MeshCollider))
                {
                    //Debug.LogWarning("Mesh Creator: found a collider on game object " + gameObject.name +", please remove it.");
                    MeshCollider mshcol = mcd.gameObject.GetComponent("MeshCollider") as MeshCollider;
                    if (mshcol != null)
                    {
                        //Debug.LogWarning("Mesh Creator: found a mesh collider on game object " + gameObject.name + ", destroying it's mesh.");
                        mshcol.sharedMesh = null;
                    }
                }

                Undo.DestroyObjectImmediate(col);

			}
				
			// all compound colliders are stored in a gameObject 
			string compoundColliderName = mcd.gameObject.name + "CompoundColliders";

            GameObject compoundColliderObject = null;
			
			// find old compound colliders and remove
			foreach (Transform child in mcd.gameObject.transform) {
				if (child.name == compoundColliderName) {
					
                    //DestroyImmediate(go);

					
                    compoundColliderObject = child.gameObject;
					ArrayList removeChildren = new ArrayList();
					foreach (Transform childchild in child) {
						removeChildren.Add(childchild);
					}
					
                    foreach (Transform childchild in removeChildren) 
                    {
                        Undo.DestroyObjectImmediate(childchild.gameObject);
					}
				}
			}


            if (compoundColliderObject == null)
            {
                compoundColliderObject = new GameObject();
            }
            Undo.RegisterFullObjectHierarchyUndo(compoundColliderObject, Undo.GetCurrentGroupName());

				
            compoundColliderObject.name = compoundColliderName;
            compoundColliderObject.transform.parent = mcd.gameObject.transform;
            compoundColliderObject.transform.localPosition = Vector3.zero;
            compoundColliderObject.transform.rotation = Quaternion.identity;
			ArrayList boxColliderCoordinates = GetBoxColliderCoordinates(gameObject);
			int count = 0;
			int imageHeight = mcd.outlineTexture.height;
			int imageWidth = mcd.outlineTexture.width;
			foreach (Vector4 bcc in boxColliderCoordinates) {
				Vector4 bc = bcc;
				
				// if using a uvWrapMesh, subtract half a pixel from each side 
				if (mcd.uvWrapMesh && Math.Abs(bc.x - bc.z) > 1.0f && Math.Abs(bc.y - bc.w) > 1.0f) {
					bc.x += 0.5f;
					bc.y += 0.5f;
					bc.z -= 0.5f;
					bc.w -= 0.5f;
				}
				else if (mcd.uvWrapMesh) { // if here, height or width is only one
					continue;
				}
				
				{
					count++;

                    GameObject colgo = new GameObject();
                    Undo.RegisterCreatedObjectUndo(colgo, "Update Mesh");

					colgo.name = compoundColliderName+"."+count;
                    colgo.transform.parent = compoundColliderObject.transform;
					colgo.transform.localPosition = Vector3.zero;
					
                    BoxCollider bxcol = Undo.AddComponent<BoxCollider>(colgo);

					float vertX = 1.0f - (bc.x/imageWidth) ; // get X point and normalize
					float vertY = bc.y/imageHeight ; // get Y point and normalize
					float vert2X = 1.0f - (bc.z/imageWidth);
					float vert2Y = bc.w/imageHeight;
					vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
					vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);
					
					vert2X = (vert2X * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
					vert2Y = (vert2Y * mcd.meshHeight) - (mcd.meshHeight / 2.0f);
					
					bxcol.center = new Vector3(vertX - ((vertX-vert2X)/2.0f)-mcd.pivotWidthOffset, vertY - ((vertY-vert2Y)/2.0f)-mcd.pivotHeightOffset, - mcd.pivotDepthOffset);

					bxcol.size = new Vector3(Math.Abs(vertX-vert2X), Math.Abs(vertY-vert2Y), mcd.meshDepth);
					
                    // use physics material
                    if (mcd.usePhysicMaterial) {
						bxcol.material = mcd.physicMaterial;
					}

                    // set trigger for this box collider?
                    if (mcd.setTriggers)
                    {
                        bxcol.isTrigger = true;
                    }
				}
			}
        } // end generate box colliders

        // generate AABB collider
        else if (mcd.generateCollider && !mcd.usePrimitiveCollider && mcd.useAABBCollider)
        {
            // remove the old collider if necessary
            Collider col = mcd.gameObject.GetComponent<Collider>();
            if (col != null)
            {
                Undo.DestroyObjectImmediate(col);
            }
                
            Undo.AddComponent<BoxCollider>(mcd.gameObject);

            // remove the old compound collider before assigning new
            string compoundColliderName = mcd.gameObject.name + "CompoundColliders";
            foreach (Transform child in mcd.gameObject.transform)
            {
                if (child.name == compoundColliderName)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }

            BoxCollider bxcol = mcd.gameObject.GetComponent("BoxCollider") as BoxCollider;

            Vector4 extents = GetTransparencyExtents(mcd.gameObject);
            int imageHeight = mcd.outlineTexture.height;
            int imageWidth = mcd.outlineTexture.width;

            float vertX = 1.0f - (extents.x / imageWidth); // get X point and normalize
            float vertY = extents.y / imageHeight; // get Y point and normalize
            float vert2X = 1.0f - (extents.z / imageWidth);
            float vert2Y = extents.w / imageHeight;
            vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
            vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);

            vert2X = (vert2X * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
            vert2Y = (vert2Y * mcd.meshHeight) - (mcd.meshHeight / 2.0f);

            bxcol.center = new Vector3(vertX - ((vertX - vert2X) / 2.0f) - mcd.pivotWidthOffset, vertY - ((vertY - vert2Y) / 2.0f) - mcd.pivotHeightOffset, -mcd.pivotDepthOffset);

            bxcol.size = new Vector3(Math.Abs(vertX - vert2X), Math.Abs(vertY - vert2Y), mcd.meshDepth);

            // use physics material
            if (mcd.usePhysicMaterial)
            {
                bxcol.material = mcd.physicMaterial;
            }

            // set trigger for this box collider?
            if (mcd.setTriggers)
            {
                bxcol.isTrigger = true;
            }

        } // end generate AABB collider
        else
        {
            // remove the old collider if necessary
            Collider col = mcd.gameObject.GetComponent<Collider>();
            if (col != null)
            {
                Undo.DestroyObjectImmediate(col);
            }

            // remove the old compound collider before assigning new
            string compoundColliderName = mcd.gameObject.name + "CompoundColliders";
            foreach (Transform child in mcd.gameObject.transform)
            {
                if (child.name == compoundColliderName)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
        }

        // end collider section
			
		mcd.gameObject.transform.rotation = oldRotation;
		mcd.gameObject.transform.localScale = oldScale;
	
        //Prevents a harmless exception in cases where components deleted during a MeshUpdate() call
        EditorGUIUtility.ExitGUI();
    }

    // Vec4 returned is box coordinates
    // upperleft.x,upperleft.y, lowerright.x,lowerright.y
    // pixels in Unity are left to right, from bottom to top
    static Vector4 GetTransparencyExtents(GameObject gameObject)
    {
        MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;
        Vector4 extents = new Vector4();

        string path = AssetDatabase.GetAssetPath(mcd.outlineTexture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        textureImporter.isReadable = true;
        AssetDatabase.ImportAsset(path);

        Color[] pixels = mcd.outlineTexture.GetPixels();	// get the pixels to build the mesh from
        float pixelThreshold = mcd.pixelTransparencyThreshold / 255.0f;
        int imageHeight = mcd.outlineTexture.height;
        int imageWidth = mcd.outlineTexture.width;

        // set the extents to max mins
        extents.z = imageWidth - 1;
        extents.w = imageHeight - 1;
        extents.x = 0;
        extents.y = 0;

        for (int I = 0; I < imageWidth; I++)
        {
            for (int j = 0; j < imageHeight; j++)
            {
                if (pixels[I + (imageWidth * j)].a >= pixelThreshold)
                {
                    if (I < extents.z) extents.z = I;
                    if (I > extents.x) extents.x = I;
                    if (j < extents.w) extents.w = j;
                    if (j > extents.y) extents.y = j;
                }
            }
        }

        return extents;
    }
	
	static ArrayList GetBoxColliderCoordinates(GameObject gameObject) {
		MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;
		
		ArrayList boxCoordinates = new ArrayList();
		string path = AssetDatabase.GetAssetPath(mcd.outlineTexture);
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		textureImporter.isReadable = true;
		AssetDatabase.ImportAsset(path);

		Color[] pixels = mcd.outlineTexture.GetPixels();	// get the pixels to build the mesh from
			
		// possibly do some size checking
		// TODO: check for a square power of two
		int imageHeight = mcd.outlineTexture.height;
		int imageWidth = mcd.outlineTexture.width;
		
		if ( ((float)imageWidth)/((float)imageHeight) != mcd.meshWidth/mcd.meshHeight) {
			Debug.LogWarning("Mesh Creator: selected meshWidth and meshHeight is not the same proportion as source image width and height. Results may be distorted.");
		}
		
		// copy the pixels so they can be modified
		Color[] pix = new Color[pixels.Length];
		for (int i = 0; i < pixels.Length; i++) {
			Color pixel = pixels[i];
			pix[i] = new Color(pixel.r, pixel.g, pixel.b, pixel.a);
		}
		
		Vector4 boxCoord = GetLargestBox(ref pix, imageWidth, imageHeight, mcd.pixelTransparencyThreshold/255.0f);
        boxCoordinates.Add(boxCoord);
		while(boxCoordinates.Count < mcd.maxNumberBoxes)
        {
			boxCoord = GetLargestBox(ref pix, imageWidth, imageHeight, mcd.pixelTransparencyThreshold/255.0f);
            boxCoordinates.Add(boxCoord);
		}
		return boxCoordinates;
	}
	
		
	// based on algorithm from http://e-maxx.ru/algo/maximum_zero_submatrix
	static Vector4 GetLargestBox(ref Color[] pixs, int imageWidth, int imageHeight, float threshold) {
		Vector4 largestBox = new Vector4(-1.0f,-1.0f,-1.0f,-1.0f);
		int n = imageHeight;
		int m = imageWidth; 
		
		List< List<int> > a = new List< List<int> > ( n ) ;
		for (int i = 0; i < n; i++) {
			a.Add(new List<int>(m));
			for (int j = 0; j < m; j++) {
				a[i].Add(0);
			}
		}
		
		for  ( int I = 0 ; I < n ; I++ ) {
			for  ( int j = 0 ; j < m ;  j++ ) {
				if (pixs[j + (imageWidth * I )].a < threshold) a[ I ][ j ] = 1; // check if alpha is less than threshold
			}
		}
		 
		int ans =  0 ;
		List < int > d  = new List < int > ( m );
		List < int > d1 = new List <int> ( m );
		List <int >  d2 = new List<int>( m ) ;
		for (int i = 0; i < m; ++i) {
			d.Add(-1);
			d1.Add(-1);
			d2.Add(-1);
		}
		
		Stack < int > st = new Stack<int>(); 
		for (int i=0; i<n; ++i) {
			for (int j=0; j<m; ++j) if (a[i][j] == 1) d[j] = i;
			while (st.Count > 0) st.Pop(); // empty the stack
			for (int j=0; j<m; ++j) {
				while (st.Count > 0 && d[st.Peek()] <= d[j])  st.Pop();
				d1[j] = st.Count == 0 ? -1 : st.Peek();
				st.Push(j);
			}
			while (st.Count > 0) st.Pop();
			for (int j=m-1; j>=0; --j) {
				while (st.Count>0 && d[st.Peek()] <= d[j])  st.Pop();
				d2[j] = st.Count == 0 ? m : st.Peek();
				st.Push (j);
			}
			for (int j=0; j<m; ++j) {
				int oldLarge = ans;
				ans = Math.Max (ans, (i - d[j]) * (d2[j] - d1[j] - 1));
				if (oldLarge != ans) {
					largestBox[2] = d2[j];
					largestBox[3] = i+1; 
					largestBox[0] = d1[j] +1;
					largestBox[1] = d[j]+1;
				}
			}
		} 
		
		// remove inside pixels from the box area
			if (largestBox.x != -1.0f) {
				for (int i = (int)largestBox.x ; i < (int)largestBox.z; i++) {
					for (int j = (int)largestBox.y ; j < (int)largestBox.w; j++) {
						pixs[i + (j *imageWidth)].a = 0.0f;
					}
				}
				// delete all pixels if this is width 1 or height 1
				if ( ((int)Math.Abs(largestBox.x-largestBox.z) == 1) || ((int)Math.Abs(largestBox.y-largestBox.w) == 1) ){
					for (int i = (int)largestBox.x; i <= (int)largestBox.z; i++) {
						for (int j = (int)largestBox.y; j <= (int)largestBox.w; j++) {
							pixs[i + (j *imageWidth)].a = 0.0f;
						}
					}
				}
			}
			else {
                Debug.Log("Mesh Creator: yikes, got a negative box inside pixel map array. Try resaving the image. Please create a new issue at https://github.com/uclagamelab/MeshCreator/issues.");
			}
		
		return largestBox;
	}
	

	
	/*
	*	AssignMesh() does calculation of a uv mapped mesh from the raster image.
	*/ 
	public static void AssignMesh(GameObject gameObject, ref Mesh msh) {
		MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;
		string path = AssetDatabase.GetAssetPath(mcd.outlineTexture);
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		textureImporter.isReadable = true;
		AssetDatabase.ImportAsset(path);
		
		Color[] pixels = mcd.outlineTexture.GetPixels();	// get the pixels to build the mesh from
		
		// possibly do some size checking
		int imageHeight = mcd.outlineTexture.height;
		int imageWidth = mcd.outlineTexture.width;
		if ( ((float)imageWidth)/((float)imageHeight) != mcd.meshWidth/mcd.meshHeight) {
			//Debug.LogWarning("Mesh Creator Inspector Warning: selected meshWidth and meshHeight is not the same proportion as source image width and height. Results may be distorted.");
			//Debug.LogWarning("    You may want to resize your image to be square, it can be easier that way.");
		}
		
		// make a surface object to create and store data from image
		MC_SimpleSurfaceEdge mcs = new MC_SimpleSurfaceEdge(pixels,  imageWidth, imageHeight, mcd.pixelTransparencyThreshold/255.0f);
		
		if ( mcd.mergeClosePoints ) mcs.MergeClosePoints(mcd.mergeDistance);
		
		// Create the mesh
		
		if (!mcs.ContainsIslands()) {
			// need a list of ordered 2d points
			Vector2 [] vertices2D = mcs.GetOutsideEdgeVertices();
    
			// Use the triangulator to get indices for creating triangles
			Triangulator tr = new Triangulator(vertices2D);

			int[] indices = tr.Triangulate(); // these will be reversed for the back side
			Vector2[] uvs = new Vector2[vertices2D.Length * 4];
			// Create the Vector3 vertices
			Vector3[] vertices = new Vector3[vertices2D.Length * 4];
		
			float halfDepth = -mcd.meshDepth/2.0f;
			float halfVerticalPixel = 0.5f/imageHeight;
			float halfHorizontalPixel = 0.5f/imageWidth;
			for (int i=0; i<vertices2D.Length; i++) {
				float vertX = 1.0f - (vertices2D[i].x/imageWidth) - halfHorizontalPixel; // get X point and normalize
				float vertY = vertices2D[i].y/imageHeight + halfVerticalPixel; // get Y point and normalize
				vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
				vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);
				
				vertices[i] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth  - mcd.pivotDepthOffset);
				
				vertices[i + vertices2D.Length] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, halfDepth-mcd.pivotDepthOffset);
				
				vertices[i+(vertices2D.Length*2)] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth -mcd.pivotDepthOffset); // vertex for side
				
				vertices[i +(vertices2D.Length*3)] = new Vector3(vertX-mcd.pivotWidthOffset, vertY-mcd.pivotHeightOffset, halfDepth -mcd.pivotDepthOffset);

				uvs[i] = mcs.GetUVForIndex(i);
				uvs[i+vertices2D.Length] = uvs[i];
				uvs[i+(vertices2D.Length*2)] = uvs[i];
				uvs[i+(vertices2D.Length*3)] = uvs[i];
			}
		
			// make the back side triangle indices
			// double the indices for front and back, 6 times the number of edges on front
			int[] allIndices = new int[(indices.Length*2) + ( (vertices2D.Length ) * 6)];
		
			// copy over the front and back index data
			for (int i = 0; i < indices.Length; i++) {
				allIndices[i] = indices[i]; // front side uses normal indices returned from the algorithm
				allIndices[(indices.Length*2) - i -1] = indices[i] + vertices2D.Length; // backside reverses the order
			}
		
			// create the side triangle indices
			// for each edge, create a new set of two triangles
			// edges are just two points from the original set
			for (int i = 0; i < vertices2D.Length - 1; i++) {
				allIndices[(indices.Length*2) + (6 * i)] = (vertices2D.Length *2) + i + 1;
				allIndices[(indices.Length*2) + (6 * i) + 1] = (vertices2D.Length *2) +i ;
				allIndices[(indices.Length*2) + (6 * i) + 2] = (vertices2D.Length *2) + i + 1 + vertices2D.Length;
				allIndices[(indices.Length*2) + (6 * i) + 3] = (vertices2D.Length *2) + i + 1 + vertices2D.Length;
				allIndices[(indices.Length*2) + (6 * i) + 4] = (vertices2D.Length *2) + i ;
				allIndices[(indices.Length*2) + (6 * i) + 5] = (vertices2D.Length *2) + i + vertices2D.Length;
			}
		
			// wrap around for the last face
			allIndices[allIndices.Length-6] = (vertices2D.Length *2) + 0;
			allIndices[allIndices.Length-5] = (vertices2D.Length *2) +vertices2D.Length-1;
			allIndices[allIndices.Length-4] = (vertices2D.Length *2) +vertices2D.Length;
			allIndices[allIndices.Length-3] = (vertices2D.Length *2) +vertices2D.Length;
			allIndices[allIndices.Length-2] = (vertices2D.Length *2) +vertices2D.Length-1;
			allIndices[allIndices.Length-1] = (vertices2D.Length *2) + (vertices2D.Length*2) - 1;
	
		
			msh.vertices = vertices;
			msh.triangles = allIndices;
			msh.uv = uvs;
			msh.RecalculateNormals();
			msh.RecalculateBounds();
			msh.name = mcd.outlineTexture.name + ".asset";
			
			// this will get the pivot drawing in the correct place
			Bounds oldBounds = msh.bounds;
			msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
		}
		else { // there be islands here, so treat mesh creation slightly differently
			ArrayList allVertexLoops = mcs.GetAllEdgeVertices();
			
			ArrayList completeVertices = new ArrayList();
			ArrayList completeIndices = new ArrayList();
			ArrayList completeUVs = new ArrayList();
			int verticesOffset = 0;
			int indicesOffset = 0;
			int uvOffset = 0;
			int loopCount = 0;
			foreach (Vector2[] vertices2D in allVertexLoops) {
				// TODO: this needs to check if the current list is inside another shape
				// Use the triangulator to get indices for creating triangles
				Triangulator tr = new Triangulator(vertices2D);
				int[] indices = tr.Triangulate(); // these will be reversed for the back side
				Vector2[] uvs = new Vector2[vertices2D.Length * 4];
				// Create the Vector3 vertices
				Vector3[] vertices = new Vector3[vertices2D.Length * 4];
		
				float halfDepth = -mcd.meshDepth/2.0f;
				float halfVerticalPixel = 0.5f/imageHeight;
				float halfHorizontalPixel = 0.5f/imageWidth;
				for (int i=0; i<vertices2D.Length; i++) {
					float vertX = 1.0f - (vertices2D[i].x/imageWidth) - halfHorizontalPixel; // get X point and normalize
					float vertY = vertices2D[i].y/imageHeight + halfVerticalPixel; // get Y point and normalize
					vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
					vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);
					
					vertices[i] = new Vector3(vertX-mcd.pivotWidthOffset, vertY-mcd.pivotHeightOffset, -halfDepth -mcd.pivotDepthOffset);
					vertices[i + vertices2D.Length] = new Vector3(vertX-mcd.pivotWidthOffset, vertY-mcd.pivotHeightOffset, halfDepth-mcd.pivotDepthOffset);
					vertices[i+(vertices2D.Length*2)] = new Vector3(vertX-mcd.pivotWidthOffset, vertY-mcd.pivotHeightOffset, -halfDepth -mcd.pivotDepthOffset); // vertex for side
					vertices[i +(vertices2D.Length*3)] = new Vector3(vertX-mcd.pivotWidthOffset, vertY-mcd.pivotHeightOffset, halfDepth -mcd.pivotDepthOffset);
					
					uvs[i] = mcs.GetUVForIndex(loopCount, i);
					uvs[i+vertices2D.Length] = uvs[i];
					uvs[i+(vertices2D.Length*2)] = uvs[i];
					uvs[i+(vertices2D.Length*3)] = uvs[i];
				}
		
				// make the back side triangle indices
				// double the indices for front and back, 6 times the number of edges on front
				int[] allIndices = new int[(indices.Length*2) + ( (vertices2D.Length ) * 6)];
		
				// copy over the front and back index data
				for (int i = 0; i < indices.Length; i++) {
					allIndices[i] = indices[i] +verticesOffset; // front side uses normal indices returned from the algorithm
					allIndices[(indices.Length*2) - i -1] = indices[i] + vertices2D.Length + verticesOffset; // backside reverses the order
				}
		
				// create the side triangle indices
				// for each edge, create a new set of two triangles
				// edges are just two points from the original set
				for (int i = 0; i < vertices2D.Length - 1; i++) {
					allIndices[(indices.Length*2) + (6 * i)] = (vertices2D.Length *2) + i + 1 + verticesOffset;
					allIndices[(indices.Length*2) + (6 * i) + 1] = (vertices2D.Length *2) +i + verticesOffset;
					allIndices[(indices.Length*2) + (6 * i) + 2] = (vertices2D.Length *2) + i + 1 + vertices2D.Length+ verticesOffset;
					allIndices[(indices.Length*2) + (6 * i) + 3] = (vertices2D.Length *2) + i + 1 + vertices2D.Length+ verticesOffset;
					allIndices[(indices.Length*2) + (6 * i) + 4] = (vertices2D.Length *2) + i + verticesOffset;
					allIndices[(indices.Length*2) + (6 * i) + 5] = (vertices2D.Length *2) + i + vertices2D.Length+ verticesOffset;
				}
		
				// wrap around for the last face
				allIndices[allIndices.Length-6] = (vertices2D.Length *2) + 0+ verticesOffset;
				allIndices[allIndices.Length-5] = (vertices2D.Length *2) +vertices2D.Length-1+ verticesOffset;
				allIndices[allIndices.Length-4] = (vertices2D.Length *2) +vertices2D.Length+ verticesOffset;
				allIndices[allIndices.Length-3] = (vertices2D.Length *2) +vertices2D.Length+ verticesOffset;
				allIndices[allIndices.Length-2] = (vertices2D.Length *2) +vertices2D.Length-1+ verticesOffset;
				allIndices[allIndices.Length-1] = (vertices2D.Length *2) + (vertices2D.Length*2) - 1+ verticesOffset;
				
				foreach(Vector3 v in vertices) {
					completeVertices.Add(v);
				}
				foreach(Vector2 v in uvs) {
					completeUVs.Add(v);
				}
				foreach(int i in allIndices) {
					completeIndices.Add(i);
				}
				
				verticesOffset += vertices.Length;
				uvOffset += uvs.Length;
				indicesOffset += allIndices.Length;
				loopCount++;
			}
			msh.vertices = (Vector3[]) completeVertices.ToArray(typeof(Vector3));
			msh.triangles = (int[]) completeIndices.ToArray(typeof(int));
			msh.uv = (Vector2[]) completeUVs.ToArray(typeof(Vector2));
			msh.RecalculateNormals();
			msh.RecalculateBounds();
			msh.name = mcd.outlineTexture.name + ".asset";
			
			// this will get the pivot drawing in the correct place
			Bounds oldBounds = msh.bounds;
			msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
		}
	}
	
	/*
	*	AssignPlaneMesh() does calculation for a simple plane with uv coordinates
	* at the corners of the images. Really simple.
	*/ 
	public static void AssignPlaneMesh(GameObject gameObject, ref Mesh msh) {
		MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;
		
		// get the outline texture
		string path = AssetDatabase.GetAssetPath(mcd.outlineTexture);
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		textureImporter.isReadable = true;
		AssetDatabase.ImportAsset(path);
			
		// do some size checking
		int imageHeight = mcd.outlineTexture.height;
		int imageWidth = mcd.outlineTexture.width;
		
		if ( ((float)imageWidth)/((float)imageHeight) != mcd.meshWidth/mcd.meshHeight) {
			Debug.LogWarning("Mesh Creator: selected meshWidth and meshHeight is not the same proportion as source image width and height. Results may be distorted.");
			Debug.LogWarning("    You may want to resize your image to be square, it can be easier that way.");
		}
		
		// need a list of ordered 2d points
		Vector2 [] vertices2D = {new Vector2(0.0f,0.0f), new Vector2(0.0f, imageHeight), new Vector2(imageWidth, imageHeight), new Vector2(imageWidth,0.0f)};
        
		// 
		int[] indices = {0,1,2,0,2,3}; // these will be reversed for the back side
		Vector2[] frontUVs = {new Vector2(0.0f,0.0f), new Vector2(0.0f,1.0f), new Vector2(1.0f,1.0f), new Vector2(1.0f,0.0f) };
		Vector2[] uvs = new Vector2[vertices2D.Length];
		// Create the Vector3 vertices
		Vector3[] vertices = new Vector3[vertices2D.Length];
		
		float halfDepth = -mcd.meshDepth/2.0f;
		for (int i=0; i<vertices2D.Length; i++) {
			float vertX = 1.0f - (vertices2D[i].x/imageWidth) ; // get X point and normalize
			float vertY = vertices2D[i].y/imageHeight; // get Y point and normalize
			vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
			vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);

			vertices[i] = new Vector3(vertX -mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset );
			
			uvs[i] = frontUVs[i];
		}
		
		msh.vertices = vertices;
		msh.triangles = indices;
		msh.uv = uvs;
		msh.RecalculateNormals();
		msh.RecalculateBounds();
		msh.name = mcd.outlineTexture.name + ".mesh";
		
		// this will get the pivot drawing in the correct place
		Bounds oldBounds = msh.bounds;
		msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
	}
	
	/*
	*	AssignPlaneMesh() does calculation for a simple plane with uv coordinates
	* at the corners of the images. Really simple.
	*/ 
	public static void AssignPlaneMeshBackside(GameObject gameObject, ref Mesh msh) {
		MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;
		
		// get the outline texture
		string path = AssetDatabase.GetAssetPath(mcd.outlineTexture);
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		textureImporter.isReadable = true;
		AssetDatabase.ImportAsset(path);
						
		// do some size checking
		int imageHeight = mcd.outlineTexture.height;
		int imageWidth = mcd.outlineTexture.width;
		
		if ( ((float)imageWidth)/((float)imageHeight) != mcd.meshWidth/mcd.meshHeight) {
			Debug.LogWarning("Mesh Creator Inspector Warning: selected meshWidth and meshHeight is not the same proportion as source image width and height. Results may be distorted.");
			Debug.LogWarning("    You may want to resize your image to be square, it can be easier that way.");
		}
		
		// need a list of ordered 2d points
		Vector2 [] vertices2D = {new Vector2(0.0f,0.0f), new Vector2(0.0f, imageHeight), new Vector2(imageWidth, imageHeight), new Vector2(imageWidth,0.0f)};
        
		// 
		int[] indices = {2,1,0,3,2,0}; // these will be reversed for the back side
		Vector2[] frontUVs = {new Vector2(0.0f,0.0f), new Vector2(0.0f,1.0f), new Vector2(1.0f,1.0f), new Vector2(1.0f,0.0f) };
		Vector2[] uvs = new Vector2[vertices2D.Length];
		// Create the Vector3 vertices
		Vector3[] vertices = new Vector3[vertices2D.Length];
		
		float halfDepth = mcd.meshDepth/2.0f;
		for (int i=0; i<vertices2D.Length; i++) {
			float vertX = 1.0f - (vertices2D[i].x/imageWidth) ; // get X point and normalize
			float vertY = vertices2D[i].y/imageHeight; // get Y point and normalize
			vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
			vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);
			
			vertices[i] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset);

			uvs[i] = frontUVs[i];
		}
			
		msh.vertices = vertices;
		msh.triangles = indices;
		msh.uv = uvs;
		msh.RecalculateNormals();
		msh.RecalculateBounds();
		msh.name = mcd.outlineTexture.name + ".asset";
		
		// this will get the pivot drawing in the correct place
		Bounds oldBounds = msh.bounds;
		msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
	}
	
	/*
	*	AssignEdgeMesh() does calculation of a uv mapped edge mesh from the raster image.
	*	no front or back planes are included
	*/ 
	public static void AssignEdgeMesh(GameObject gameObject, ref Mesh msh) {
		MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;
		
			string path = AssetDatabase.GetAssetPath(mcd.outlineTexture);
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			textureImporter.isReadable = true;
			AssetDatabase.ImportAsset(path);
			
			Color[] pixels = mcd.outlineTexture.GetPixels();	// get the pixels to build the mesh from
			
			// possibly do some size checking
			int imageHeight = mcd.outlineTexture.height;
			int imageWidth = mcd.outlineTexture.width;
			if ( ((float)imageWidth)/((float)imageHeight) != mcd.meshWidth/mcd.meshHeight) {
				Debug.LogWarning("Mesh Creator: selected meshWidth and meshHeight is not the same proportion as source image width and height. Results may be distorted.");
				Debug.LogWarning("    You may want to resize your image to be square, it can be easier that way.");
			}
			
			// make a surface object to create and store data from image
			MC_SimpleSurfaceEdge mcs = new MC_SimpleSurfaceEdge(pixels,  imageWidth, imageHeight, mcd.pixelTransparencyThreshold/255.0f);
			
			if (!mcs.ContainsIslands()) {
				// need a list of ordered 2d points
				Vector2 [] vertices2D = mcs.GetOutsideEdgeVertices();
        
				// Use the triangulator to get indices for creating triangles
				//Triangulator tr = new Triangulator(vertices2D);
				//int[] indices = tr.Triangulate(); // these will be reversed for the back side
				Vector2[] uvs = new Vector2[vertices2D.Length * 2];
				// Create the Vector3 vertices
				Vector3[] vertices = new Vector3[vertices2D.Length * 2];
			
				float halfDepth = -mcd.meshDepth/2.0f;
				float halfVerticalPixel = 0.5f/imageHeight;
				float halfHorizontalPixel = 0.5f/imageWidth;
				for (int i=0; i<vertices2D.Length; i++) {
					float vertX = 1.0f - (vertices2D[i].x/imageWidth) - halfHorizontalPixel; // get X point and normalize
					float vertY = vertices2D[i].y/imageHeight + halfVerticalPixel; // get Y point and normalize
					vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
					vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);
					
					vertices[i] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth  - mcd.pivotDepthOffset); // vertex for side
					vertices[i +vertices2D.Length] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, halfDepth  - mcd.pivotDepthOffset);
					
					uvs[i] = mcs.GetUVForIndex(i);
					uvs[i+vertices2D.Length] = uvs[i];
				}
			
				// make the back side triangle indices
				// double the indices for front and back, 6 times the number of edges on front
				int[] allIndices = new int[vertices2D.Length  * 6];
			
				// create the side triangle indices
				// for each edge, create a new set of two triangles
				// edges are just two points from the original set
				for (int i = 0; i < vertices2D.Length - 1; i++) {
					allIndices[ (6 * i)] = i + 1;
					allIndices[ (6 * i) + 1] =  i ;
					allIndices[ (6 * i) + 2] =   i + 1 + vertices2D.Length;
					allIndices[ (6 * i) + 3] =   i + 1 + vertices2D.Length;
					allIndices[ (6 * i) + 4] =  i ;
					allIndices[ (6 * i) + 5] = i + vertices2D.Length;
				}
			
				// wrap around for the last face
				allIndices[allIndices.Length-6] = 0;
				allIndices[allIndices.Length-5] = vertices2D.Length-1;
				allIndices[allIndices.Length-4] =vertices2D.Length;
				allIndices[allIndices.Length-3] = vertices2D.Length;
				allIndices[allIndices.Length-2] = vertices2D.Length-1;
				allIndices[allIndices.Length-1] = (vertices2D.Length*2) - 1;
		
			
				msh.vertices = vertices;
				msh.triangles = allIndices;
				msh.uv = uvs;
				msh.RecalculateNormals();
				msh.RecalculateBounds();
				msh.name = mcd.outlineTexture.name + ".asset";
				
				// this will get the pivot drawing in the correct place
				Bounds oldBounds = msh.bounds;
				msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
			}
			else { // there be islands here, so treat mesh creation slightly differently
				ArrayList allVertexLoops = mcs.GetAllEdgeVertices();
				
				ArrayList completeVertices = new ArrayList();
				ArrayList completeIndices = new ArrayList();
				ArrayList completeUVs = new ArrayList();
				int verticesOffset = 0;
				int indicesOffset = 0;
				int uvOffset = 0;
				int loopCount = 0;
				foreach (Vector2[] vertices2D in allVertexLoops) {
					Vector2[] uvs = new Vector2[vertices2D.Length * 4];
					// Create the Vector3 vertices
					Vector3[] vertices = new Vector3[vertices2D.Length * 4];
			
					float halfDepth = -mcd.meshDepth/2.0f;
					float halfVerticalPixel = 0.5f/imageHeight;
					float halfHorizontalPixel = 0.5f/imageWidth;
					for (int i=0; i<vertices2D.Length; i++) {
						float vertX = 1.0f - (vertices2D[i].x/imageWidth) - halfHorizontalPixel; // get X point and normalize
						float vertY = vertices2D[i].y/imageHeight + halfVerticalPixel; // get Y point and normalize
						vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
						vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);
						
						vertices[i] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset);
						vertices[i + vertices2D.Length] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, halfDepth - mcd.pivotDepthOffset);
						
						uvs[i] = mcs.GetUVForIndex(loopCount, i);
						uvs[i+vertices2D.Length] = uvs[i];
					}
			
					// make the back side triangle indices
					// double the indices for front and back, 6 times the number of edges on front
					int[] allIndices = new int[vertices2D.Length * 6];
			
					// create the side triangle indices
					// for each edge, create a new set of two triangles
					// edges are just two points from the original set
					for (int i = 0; i < vertices2D.Length - 1; i++) {
						allIndices[(6 * i)] = i + 1 + verticesOffset;
						allIndices[(6 * i) + 1] = i + verticesOffset;
						allIndices[(6 * i) + 2] =  i + 1 + vertices2D.Length+ verticesOffset;
						allIndices[ (6 * i) + 3] =  i + 1 + vertices2D.Length+ verticesOffset;
						allIndices[(6 * i) + 4] =  i + verticesOffset;
						allIndices[(6 * i) + 5] =  i + vertices2D.Length+ verticesOffset;
					}
			
					// wrap around for the last face
					allIndices[allIndices.Length-6] =  0+ verticesOffset;
					allIndices[allIndices.Length-5] =vertices2D.Length-1+ verticesOffset;
					allIndices[allIndices.Length-4] = vertices2D.Length+ verticesOffset;
					allIndices[allIndices.Length-3] = vertices2D.Length+ verticesOffset;
					allIndices[allIndices.Length-2] = vertices2D.Length-1+ verticesOffset;
					allIndices[allIndices.Length-1] = (vertices2D.Length*2) - 1+ verticesOffset;
					
					foreach(Vector3 v in vertices) {
						completeVertices.Add(v);
					}
					foreach(Vector2 v in uvs) {
						completeUVs.Add(v);
					}
					foreach(int i in allIndices) {
						completeIndices.Add(i);
					}
					
					verticesOffset += vertices.Length;
					uvOffset += uvs.Length;
					indicesOffset += allIndices.Length;
					loopCount++;
				}
				msh.vertices = (Vector3[]) completeVertices.ToArray(typeof(Vector3));
				msh.triangles = (int[]) completeIndices.ToArray(typeof(int));
				msh.uv = (Vector2[]) completeUVs.ToArray(typeof(Vector2));
				msh.RecalculateNormals();
				msh.RecalculateBounds();
				msh.name = mcd.outlineTexture.name + ".asset";
				
				// this will get the pivot drawing in the correct place
				Bounds oldBounds = msh.bounds;
				msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
			}
	}
	
	private static String GetTimestamp() {
		return DateTime.Now.ToString("yyyyMMddHHmmssffff");
	}

    // copies a texture and saves into the project
    public static Material CopyTexture(string baseNameLocation, 
        string newNameLocation,
        Texture texture)
    {
        Material mat;
        AssetDatabase.CopyAsset(baseNameLocation, newNameLocation);
        AssetDatabase.ImportAsset(newNameLocation);
        mat = (Material)AssetDatabase.LoadAssetAtPath(newNameLocation, typeof(Material));
        // mat.name = mcd.outlineTexture.name + ".Material"; // this probably isn't needed
        mat.mainTexture = texture;
        AssetDatabase.SaveAssets();
        return mat;
    }
	
	// generates a unique string for mesh naming
	// from http://madskristensen.net/post/Generate-unique-strings-and-numbers-in-C.aspx
	public static string GenerateId()
	{
 		long i = 1;
 		foreach (byte b in Guid.NewGuid().ToByteArray())
 		{
  			i *= ((int)b + 1);
 		}
 		return string.Format("{0:x}", i - DateTime.Now.Ticks);
	}
	
	public static bool IdExistsInScene(MeshCreatorData mcd)
	{
		// check all objects in this scene for a matching unique number
		object[] objs = GameObject.FindObjectsOfType( typeof(GameObject));
		foreach (GameObject go in objs)
		{
			MeshCreatorData meshcd = go.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;
			if (meshcd && go != mcd.gameObject)
			{
				if (meshcd.idNumber == mcd.idNumber) return true;
			}
		}
		return false;
	}
}
