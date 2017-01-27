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
using System.Collections;
using UnityEditor;

// enum for the dropdown object type selector
public enum ObjectMeshType
{
    Flat2D = 0,
    Full3D = 1
}

public enum ObjectColliderType
{
    Boxes = 0,
    Mesh = 1,
    BoundingBox = 2,
    None = 3
}

// thanks to Chris Reilly for changing this to EditorWindow from Wizard
public class MeshCreatorWizard : EditorWindow
{
    private const float versionNumber = 0.8f;
    private Texture2D gameLabLogo;
    public Texture2D textureToCreateMeshFrom;

    public bool withColliders;

    public string gameObjectName = "Mesh Creator Object";

    // enum for the meshtype(2d,3d) to be created
    public ObjectMeshType meshType = ObjectMeshType.Flat2D;

    // enum for they collider type to be created
    public ObjectColliderType colliderType = ObjectColliderType.Boxes;

    // window size
    static public Vector2 minWindowSize = new Vector2(600, 425);

    // Add menu named "Create Mesh Object" to the GameObject menu
    [MenuItem("GameObject/Create Mesh Object")]

    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MeshCreatorWizard window = (MeshCreatorWizard)EditorWindow.GetWindow(typeof(MeshCreatorWizard), true, "Create Mesh Object v" + versionNumber);
        window.minSize = minWindowSize;
    }

    void OnGUI()
    {

        EditorGUIUtility.AddCursorRect(new Rect(10, 10, 400, 150), MouseCursor.Link);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();


        // display game lab logo & link 
        if (GUILayout.Button(gameLabLogo))
        {
            Application.OpenURL("http://games.ucla.edu/");
        }

        GUILayout.FlexibleSpace();
        //basic instructions
        GUILayout.Label("Choose a texture with alpha channel to create a mesh from\nSquare images are recommended.\n\nThen select whether to create depth on the mesh and whether you\nwant colliders for your new mesh.\n\nEnter a game object name and you are good to go.\n\nAdvanced control is available once you create the object.", GUILayout.Width(400));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical();
        //source texture
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Texture to Create Mesh From", GUILayout.Width(175));
        GUILayoutOption[] textureDisplaySize = { GUILayout.Width(150), GUILayout.Height(150) };



        if (textureToCreateMeshFrom != null)
        {
            if (textureToCreateMeshFrom.height != textureToCreateMeshFrom.width)
            {
                if (textureToCreateMeshFrom.width > textureToCreateMeshFrom.height)
                {
                    textureDisplaySize[0] = GUILayout.Width(150);
                    textureDisplaySize[1] = GUILayout.Height(150 * textureToCreateMeshFrom.height / textureToCreateMeshFrom.width);
                }
                else
                {
                    textureDisplaySize[0] = GUILayout.Width(150 * textureToCreateMeshFrom.width / textureToCreateMeshFrom.height);
                    textureDisplaySize[1] = GUILayout.Height(150 );
                }
            }
        }



        textureToCreateMeshFrom = (Texture2D)EditorGUILayout.ObjectField(textureToCreateMeshFrom, typeof(Texture2D), false, textureDisplaySize);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        // what type of object being created, 2d or 3d?
        GUILayout.BeginHorizontal();
        meshType = (ObjectMeshType)EditorGUILayout.EnumPopup("Mesh Type", meshType, GUILayout.Width(330));
        GUILayout.EndHorizontal();

        //with colliders?
        GUILayout.BeginHorizontal();
        colliderType = (ObjectColliderType)EditorGUILayout.EnumPopup("Collider Type", colliderType, GUILayout.Width(330));
        GUILayout.EndHorizontal();

        //object name
        GUILayout.BeginHorizontal();
        GUILayout.Label("Game Object Name", GUILayout.Width(175));
        gameObjectName = GUILayout.TextField(gameObjectName, 50, GUILayout.Width(175));
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        //submit button
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create Mesh", GUILayout.Width(100))
            && textureToCreateMeshFrom != null)
        {
            // register the Undo
            //[!!!]
            //Undo.RegisterSceneUndo("Create New Mesh Object");
            Undo.SetCurrentGroupName("Create New Mesh Object");
            // create the new object and set the proper variables		
            GameObject newObject = new GameObject(gameObjectName);


            Undo.RegisterCreatedObjectUndo(newObject, Undo.GetCurrentGroupName());


            MeshCreatorData mcd = newObject.AddComponent<MeshCreatorData>() as MeshCreatorData;

            // set up mesh creator data
            mcd.outlineTexture = textureToCreateMeshFrom;
            mcd.useAutoGeneratedMaterial = true;

            // for height and width, maintain the image's aspect ratio
            if (textureToCreateMeshFrom.height != textureToCreateMeshFrom.width)
            {
                float height = textureToCreateMeshFrom.height;
                float width = textureToCreateMeshFrom.width;
                Debug.LogWarning("MeshCreatorWizard:: image " + textureToCreateMeshFrom.name + " has non-square size " + width + "x" + height + ", adjusting scale to match.");
                if (height > width)
                {
                    mcd.meshHeight = 1.0f;
                    mcd.meshWidth = width / height;
                }
                else
                {
                    mcd.meshHeight = height / width;
                    mcd.meshWidth = 1.0f;
                }
            }
            else
            {
                mcd.meshHeight = 1.0f;
                mcd.meshWidth = 1.0f;
            }

            mcd.meshDepth = 1.0f;

            // set up the depth options
            if (meshType == ObjectMeshType.Full3D)
            {
                mcd.uvWrapMesh = true;
                mcd.createEdges = false;
                mcd.createBacksidePlane = false;
            }
            else
            {
                mcd.uvWrapMesh = false;
                mcd.createEdges = false;
                mcd.createBacksidePlane = false;
            }

            // set up the collider options
            if (colliderType == ObjectColliderType.Boxes)
            {
                mcd.generateCollider = true;
                mcd.usePrimitiveCollider = true;
                mcd.useAABBCollider = false;
                mcd.maxNumberBoxes = 20;
                mcd.usePhysicMaterial = false;
                //mcd.addRigidBody = false;
            }
            else if (colliderType == ObjectColliderType.Mesh)
            {
                mcd.generateCollider = true;
                mcd.usePrimitiveCollider = false;
                mcd.useAABBCollider = false;
                mcd.maxNumberBoxes = 20;
                mcd.usePhysicMaterial = false;
                //mcd.addRigidBody = false;
            }
            else if (colliderType == ObjectColliderType.BoundingBox)
            {
                mcd.generateCollider = true;
                mcd.usePrimitiveCollider = false;
                mcd.useAABBCollider = true;
                mcd.maxNumberBoxes = 20;
                mcd.usePhysicMaterial = false;
                //mcd.addRigidBody = false;
            }
            else // default to none
            {
                mcd.generateCollider = false;
                mcd.usePrimitiveCollider = false;
                mcd.maxNumberBoxes = 20;
                mcd.usePhysicMaterial = false;
                //mcd.addRigidBody = false;
            }

            Close();

            // update the mesh
            MeshCreator.UpdateMesh(newObject, false);


            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());


        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

    }

    void OnEnable()
    {
        gameLabLogo = Resources.Load("games.ucla.logo.small") as Texture2D;
    }
}
