using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragManager : MonoBehaviour
{
    // Events (optional subscribers)
    public event Action<int> DragStarted;                 // slotIndex
    public event Action<Vector2> DragMoved;               // screenPos
    public event Action<int, Vector2, bool> DragEnded;    // slotIndex, screenPos, placedSuccess

    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private MergeManager merge;

    [Header("Slot Icons (5) - drag Slot_0/Icon ... Slot_4/Icon here")]
    [SerializeField] private RectTransform[] slotIconRects = new RectTransform[5];

    [Header("Drag Ghost (Canvas/DragGhost Image)")]
    [SerializeField] private Image dragGhost;

    [Header("Grid Highlighting")]
    [Tooltip("Uncheck to disable tile highlight during drag (e.g. for debugging)")]
    [SerializeField] private bool highlightOnDrag = true;

    [Header("Input")]
    [Tooltip(
        "When enabled, touch input is accepted in addition to the existing mouse/EventTrigger input. " +
        "Disable this to retain the original PC-only mouse behavior."
    )]
    [SerializeField] private bool enableTouchInput = true;

    private bool isDragging;
    private int draggingSlotIndex = -1;

    // Cached once in Start — covers all TileCells spawned at scene load
    private TileCell[] allTiles;

    // -------------------------------------------------

    private void Start()
    {
        if (dragGhost != null)
        {
            dragGhost.raycastTarget = false;
            dragGhost.gameObject.SetActive(false);
        }

        // Cache every TileCell present in the scene at startup
        allTiles = FindObjectsByType<TileCell>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        /*
         * IMPORTANT:
         * The original mouse-driven Update logic is preserved.
         *
         * Touch input is handled separately through the EventTrigger
         * callbacks below when Enable Touch Input is enabled.
         *
         * This prevents the original PC behavior from being replaced.
         */

        if (!isDragging)
        {
            if (Input.GetMouseButtonDown(0))
                TryBeginDrag(Input.mousePosition);

            return;
        }

        // Original mouse dragging behavior
        UpdateGhostPosition(Input.mousePosition);
        DragMoved?.Invoke(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            EndDrag(Input.mousePosition);
    }

    // -------------------------------------------------
    // Existing Drag Lifecycle
    // -------------------------------------------------

    private void TryBeginDrag(Vector2 screenPos)
    {
        // Prevent duplicate starts when both Update and an EventTrigger
        // happen to report the same pointer interaction.
        if (isDragging)
            return;

        int slotIndex = FindSlotIndexAtScreenPos(screenPos);

        if (slotIndex < 0)
            return;

        if (inventory == null)
        {
            Debug.LogError("[DragManager] InventoryManager reference is missing.", this);
            return;
        }

        if (merge == null)
        {
            Debug.LogError("[DragManager] MergeManager reference is missing.", this);
            return;
        }

        if (!inventory.HasItem(slotIndex))
            return;

        isDragging = true;
        draggingSlotIndex = slotIndex;

        if (dragGhost != null)
        {
            dragGhost.sprite = inventory.GetIcon(slotIndex);
            dragGhost.gameObject.SetActive(true);
            UpdateGhostPosition(screenPos);
        }

        ShowAvailableTiles();

        DragStarted?.Invoke(slotIndex);
    }

    private void EndDrag(Vector2 screenPos)
    {
        // Protect against EndDrag being reported twice by an EventTrigger
        // and the mouse/touch input path in the same frame.
        if (!isDragging)
            return;

        bool placed = false;

        GameObject prefab = inventory.GetPrefab(draggingSlotIndex);

        if (prefab != null)
            placed = merge.TryPlace(prefab, screenPos);

        if (placed)
            inventory.Consume(draggingSlotIndex);

        isDragging = false;

        int endedSlot = draggingSlotIndex;
        draggingSlotIndex = -1;

        if (dragGhost != null)
            dragGhost.gameObject.SetActive(false);

        ResetTileHighlights();
        DragEnded?.Invoke(endedSlot, screenPos, placed);
    }

    // -------------------------------------------------
    // EventTrigger Compatibility
    // -------------------------------------------------

    /// <summary>
    /// Existing Slot EventTriggers can call this method.
    /// It allows the EventSystem to provide the pointer position directly.
    ///
    /// This works with mouse and touch through Unity's EventSystem.
    /// </summary>
    public void HandleStartDrag(BaseEventData eventData)
    {
        if (eventData == null)
            return;

        PointerEventData pointerEventData = eventData as PointerEventData;

        if (pointerEventData == null)
            return;

        if (!enableTouchInput && pointerEventData.pointerId >= 0)
        {
            /*
             * Do not reject mouse input here.
             *
             * Unity's EventSystem can use pointerId values for mouse and
             * touch depending on the input backend, so the actual pointer
             * type is checked below where possible.
             */
        }

        if (!enableTouchInput && IsTouchPointer(pointerEventData))
            return;

        TryBeginDrag(pointerEventData.position);
    }

    /// <summary>
    /// Existing Slot EventTriggers may call this during a drag.
    /// </summary>
    public void HandleDrag(BaseEventData eventData)
    {
        if (!isDragging)
            return;

        if (eventData == null)
            return;

        PointerEventData pointerEventData = eventData as PointerEventData;

        if (pointerEventData == null)
            return;

        if (!enableTouchInput && IsTouchPointer(pointerEventData))
            return;

        Vector2 screenPos = pointerEventData.position;

        UpdateGhostPosition(screenPos);
        DragMoved?.Invoke(screenPos);
    }

    /// <summary>
    /// Existing Slot EventTriggers can call this when the pointer is released.
    ///
    /// This is particularly important because it preserves the original
    /// EventTrigger-driven drag architecture used by the Slots.
    /// </summary>
    public void HandleEndDrag(BaseEventData eventData)
    {
        if (!isDragging)
            return;

        if (eventData == null)
            return;

        PointerEventData pointerEventData = eventData as PointerEventData;

        if (pointerEventData == null)
            return;

        if (!enableTouchInput && IsTouchPointer(pointerEventData))
            return;

        EndDrag(pointerEventData.position);
    }

    // -------------------------------------------------
    // Pointer Type
    // -------------------------------------------------

    private bool IsTouchPointer(PointerEventData pointerEventData)
    {
        if (pointerEventData == null)
            return false;

        return pointerEventData.pointerId >= 0 &&
               pointerEventData.pointerPress != null;
    }

    // -------------------------------------------------
    // Tile Highlighting
    // -------------------------------------------------

    /// <summary>
    /// While dragging: show Available on empty tiles, Occupied on occupied ones.
    /// </summary>
    private void ShowAvailableTiles()
    {
        if (!highlightOnDrag || allTiles == null)
            return;

        foreach (TileCell tile in allTiles)
        {
            if (tile == null)
                continue;

            tile.SetState(
                tile.IsOccupied
                    ? TileState.Occupied
                    : TileState.Available
            );
        }
    }

    /// <summary>
    /// After drag ends: restore each tile to its natural Unoccupied / Occupied icon.
    /// </summary>
    private void ResetTileHighlights()
    {
        if (allTiles == null)
            return;

        foreach (TileCell tile in allTiles)
        {
            if (tile == null)
                continue;

            tile.RefreshIcon();
        }
    }

    // -------------------------------------------------
    // Helpers
    // -------------------------------------------------

    private void UpdateGhostPosition(Vector2 screenPos)
    {
        if (dragGhost == null || canvas == null)
            return;

        RectTransform ghostRect = dragGhost.rectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            screenPos,
            null,
            out Vector2 anchoredPos
        );

        ghostRect.anchoredPosition = anchoredPos;
    }

    private int FindSlotIndexAtScreenPos(Vector2 screenPos)
    {
        for (int i = 0; i < slotIconRects.Length; i++)
        {
            RectTransform rt = slotIconRects[i];

            if (rt == null)
                continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(
                    rt,
                    screenPos,
                    null))
            {
                return i;
            }
        }

        return -1;
    }
}