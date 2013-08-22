// Created by Daniele Giardini - 2011 - Holoville - http://www.holoville.com
// Mesh Creator team found this at http://wiki.unity3d.com/index.php?title=EditorUndoManager
// we stripped out many of the comments and changed names of things

using UnityEditor;
using UnityEngine;

public class MeshCreatorUndoManager
{

    // VARS ///////////////////////////////////////////////////

    private Object defTarget;
    private string defName;
    private bool autoSetDirty;
    private bool listeningForGuiChanges;
    private bool isMouseDown;
    private Object waitingToRecordPrefab; // If different than NULL indicates the prefab instance that will need to record its state as soon as the mouse is released. 

    // ***********************************************************************************
    // CONSTRUCTOR
    // ***********************************************************************************

    /// Creates a new MeshCreatorUndoManager,
    /// setting it so that the target is marked as dirty each time a new undo is stored. 
    /// params:
    /// The default Object you want to save undo info for.
    /// The default name of the thing to undo (displayed as "Undo [name]" in the main menu).
    public MeshCreatorUndoManager(Object p_target, string p_name) : this(p_target, p_name, true) { }

    /// Creates a new MeshCreatorUndoManager.
    /// params:
    /// The default Object you want to save undo info for.
    /// The default name of the thing to undo (displayed as "Undo [name]" in the main menu).
    /// If TRUE, marks the target as dirty each time a new undo is stored.
    public MeshCreatorUndoManager(Object p_target, string p_name, bool p_autoSetDirty)
    {
        defTarget = p_target;
        defName = p_name;
        autoSetDirty = p_autoSetDirty;
    }

    // ===================================================================================
    // METHODS ---------------------------------------------------------------------------

    /// Call this method BEFORE any undoable UnityGUI call.
    /// Manages undo for the default target, with the default name.
    public void CheckUndo() { CheckUndo(defTarget, defName); }

    /// Call this method BEFORE any undoable UnityGUI call.
    /// Manages undo for the given target, with the default name.
    /// params:
    /// The object you want to save undo info for.
    public void CheckUndo(Object p_target) { CheckUndo(p_target, defName); }

    /// Call this method BEFORE any undoable UnityGUI call.
    /// Manages undo for the given target, with the given name.
    /// params:
    /// The object you want to save undo info for.
    /// The name of the thing to undo (displayed as "Undo [name]" in the main menu).
    public void CheckUndo(Object p_target, string p_name)
    {
        Event e = Event.current;

        if (waitingToRecordPrefab != null)
        {
            // Record eventual prefab instance modification.
            // TODO Avoid recording if nothing changed (no harm in doing so, but it would be nicer).
            switch (e.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.KeyDown:
                case EventType.KeyUp:
                    PrefabUtility.RecordPrefabInstancePropertyModifications(waitingToRecordPrefab);
                    break;
            }
        }

        if ((e.type == EventType.MouseDown && e.button == 0) || (e.type == EventType.KeyUp && e.keyCode == KeyCode.Tab))
        {
            // When the LMB is pressed or the TAB key is released,
            // store a snapshot, but don't register it as an undo
            // (so that if nothing changes we avoid storing a useless undo).
            Undo.SetSnapshotTarget(p_target, p_name);
            Undo.CreateSnapshot();
            Undo.ClearSnapshotTarget(); // Not sure if this is necessary.
            listeningForGuiChanges = true;
        }
    }

    /// Call this method AFTER any undoable UnityGUI call.
    /// Manages undo for the default target, with the default name,
    /// and returns a value of TRUE if the target is marked as dirty.
    public bool CheckDirty() { return CheckDirty(defTarget, defName); }

    /// Call this method AFTER any undoable UnityGUI call.
    /// Manages undo for the given target, with the default name,
    /// and returns a value of TRUE if the target is marked as dirty.
    /// params:
    /// The object you want to save undo info for.
    public bool CheckDirty(Object p_target) { return CheckDirty(p_target, defName); }

    /// Call this method AFTER any undoable UnityGUI call.
    /// Manages undo for the given target, with the given name,
    /// and returns a value of TRUE if the target is marked as dirty.
    /// params:
    /// The object you want to save undo info for.
    /// The name of the thing to undo (displayed as "Undo [name]" in the main menu).
    public bool CheckDirty(Object p_target, string p_name)
    {
        if (listeningForGuiChanges && GUI.changed)
        {
            // Some GUI value changed after pressing the mouse
            // or releasing the TAB key.
            // Register the previous snapshot as a valid undo.
            SetDirty(p_target, p_name);
            return true;
        }
        return false;
    }

    /// Call this method AFTER any undoable UnityGUI call.
    /// Forces undo for the default target, with the default name.
    /// Used to undo operations that are performed by pressing a button,
    /// which doesn't set the GUI to a changed state.
    public void ForceDirty() { ForceDirty(defTarget, defName); }

    /// Call this method AFTER any undoable UnityGUI call.
    /// Forces undo for the given target, with the default name.
    /// Used to undo operations that are performed by pressing a button,
    /// which doesn't set the GUI to a changed state.
    /// params:
    /// The object you want to save undo info for.
    public void ForceDirty(Object p_target) { ForceDirty(p_target, defName); }

    /// Call this method AFTER any undoable UnityGUI call.
    /// Forces undo for the given target, with the given name.
    /// Used to undo operations that are performed by pressing a button,
    /// which doesn't set the GUI to a changed state.
    /// params:
    /// The object you want to save undo info for.
    /// The name of the thing to undo (displayed as "Undo [name]" in the main menu).
    public void ForceDirty(Object p_target, string p_name)
    {
        if (!listeningForGuiChanges)
        {
            // Create a new snapshot.
            Undo.SetSnapshotTarget(p_target, p_name);
            Undo.CreateSnapshot();
            Undo.ClearSnapshotTarget();
        }
        SetDirty(p_target, p_name);
    }

    // ===================================================================================
    // PRIVATE METHODS -------------------------------------------------------------------

    private void SetDirty(Object p_target, string p_name)
    {
        Undo.SetSnapshotTarget(p_target, p_name);
        Undo.RegisterSnapshot();
        Undo.ClearSnapshotTarget(); // Not sure if this is necessary.
        if (autoSetDirty) EditorUtility.SetDirty(p_target);
        listeningForGuiChanges = false;

        if (CheckTargetIsPrefabInstance(p_target))
        {
            // Prefab instance: record immediately and also wait for value to be changed and than re-record it
            // (otherwise prefab instances are not updated correctly when using Custom Inspectors).
            PrefabUtility.RecordPrefabInstancePropertyModifications(p_target);
            waitingToRecordPrefab = p_target;
        }
        else
        {
            waitingToRecordPrefab = null;
        }
    }

    private bool CheckTargetIsPrefabInstance(Object p_target)
    {
        return (PrefabUtility.GetPrefabType(p_target) == PrefabType.PrefabInstance);
    }
}