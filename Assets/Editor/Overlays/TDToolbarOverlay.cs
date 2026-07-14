using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

// =============================================================================
//  TDToolbarOverlay
//  Custom Scene-view toolbar for Glitch Garden.
//
//  Shows three controls in the Scene view header bar:
//    [Validate]     — runs scene validation (same as Ctrl+Alt+G)
//    [Clear Board]  — removes all placed units (same as Ctrl+Alt+T)
//    [Gizmos ▣]    — toggle button that mirrors the Scene view's own gizmos switch
//
//  Covers assignment requirement 14 (ToolbarOverlay).
// =============================================================================

/// <summary>
/// Registers the overlay with the Scene view.
/// The constructor passes the IDs of each toolbar element to include.
/// </summary>
[Overlay(typeof(SceneView), id: "glitch-garden-td-toolbar", displayName: "Glitch Garden")]
public class TDToolbarOverlay : ToolbarOverlay
{
    public TDToolbarOverlay() : base(
        TDValidateButton.id,
        TDClearBoardButton.id,
        TDGizmosToggle.id
    ) { }
}

// =============================================================================
//  BUTTON 1 — Validate Scene
// =============================================================================

/// <summary>
/// Runs the same scene validation as Tools > Glitch Garden > Validate Scene.
/// </summary>
[EditorToolbarElement(id, typeof(SceneView))]
public class TDValidateButton : EditorToolbarButton
{
    public const string id = "GlitchGarden/ValidateScene";

    public TDValidateButton()
    {
        text    = "Validate";
        tooltip = "Run scene validation — same as Tools > Glitch Garden > Validate Scene  (Ctrl+Alt+G)";

        // Try to use a built-in Unity icon; gracefully skip if it doesn't exist.
        Texture2D ico = EditorGUIUtility.IconContent("d_Valid").image as Texture2D;
        if (ico != null) icon = ico;

        clicked += ProjectEditorMenu.ValidateScene;
    }
}

// =============================================================================
//  BUTTON 2 — Clear Board
// =============================================================================

/// <summary>
/// Removes all placed units from the board, with a confirmation dialog.
/// </summary>
[EditorToolbarElement(id, typeof(SceneView))]
public class TDClearBoardButton : EditorToolbarButton
{
    public const string id = "GlitchGarden/ClearBoard";

    public TDClearBoardButton()
    {
        text    = "Clear Board";
        tooltip = "Remove all placed units from the board — same as Tools > Glitch Garden > Clear All Tiles  (Ctrl+Alt+T)";

        Texture2D ico = EditorGUIUtility.IconContent("d_TreeEditor.Trash").image as Texture2D;
        if (ico != null) icon = ico;

        clicked += ProjectEditorMenu.ClearAllTiles;
    }
}

// =============================================================================
//  TOGGLE — Gizmos
// =============================================================================

/// <summary>
/// Toggle button that mirrors the Scene view's built-in gizmos switch.
/// Uses IAccessContainerWindow so it always targets the correct Scene view
/// when multiple views are open.
/// </summary>
[EditorToolbarElement(id, typeof(SceneView))]
public class TDGizmosToggle : EditorToolbarToggle, IAccessContainerWindow
{
    public const string id = "GlitchGarden/ToggleGizmos";

    // Provided by Unity when the element is attached to a window.
    public EditorWindow containerWindow { get; set; }

    public TDGizmosToggle()
    {
        tooltip = "Toggle gizmos on/off in the Scene view";

        Texture2D ico = EditorGUIUtility.IconContent("d_SceneViewVisibility").image as Texture2D;
        if (ico != null) icon = ico;
        else text = "Gizmos"; // fallback text if icon not found

        // React to user clicking the toggle.
        this.RegisterValueChangedCallback(OnToggleChanged);

        // Sync the initial visual state with the current Scene view after layout.
        EditorApplication.delayCall += SyncState;
    }

    private void OnToggleChanged(ChangeEvent<bool> evt)
    {
        // Prefer the exact Scene view this toolbar belongs to.
        SceneView sv = containerWindow as SceneView ?? SceneView.lastActiveSceneView;
        if (sv == null) return;

        sv.drawGizmos = evt.newValue;
        sv.Repaint();

        Debug.Log($"[GlitchGarden] Scene gizmos: {(evt.newValue ? "ON" : "OFF")}");
    }

    private void SyncState()
    {
        SceneView sv = containerWindow as SceneView ?? SceneView.lastActiveSceneView;
        if (sv != null)
            SetValueWithoutNotify(sv.drawGizmos); // set visual state without firing the callback
    }
}
