using UnityEngine;

public enum TileState { Unoccupied, Occupied, Available }

public class TileCell : MonoBehaviour
{
    public Vector2Int GridPos { get; private set; }
    public bool IsOccupied { get; private set; }

    public Vector3 CenterWorld => transform.position;

    // ----- Visuals -----
    [Header("Tile State Icon")]
    [SerializeField] private SpriteRenderer stateIcon;   // child SpriteRenderer for the overlay icon
    [SerializeField] private Sprite unoccupiedSprite;
    [SerializeField] private Sprite occupiedSprite;
    [SerializeField] private Sprite availableSprite;

    // ----- Lane -----
    public Lane ParentLane { get; private set; }
    public void SetLane(Lane lane) { ParentLane = lane; }

    // ----- Lifecycle -----

    private void Awake()
    {
        // Ensure default visual even for pre-placed tiles that never call Init()
        RefreshIcon();
    }

    public void Init(Vector2Int gridPos)
    {
        GridPos = gridPos;
        IsOccupied = false;
        RefreshIcon();
    }

    // ----- Placement API -----

    public bool TryOccupy(Transform unit)
    {
        if (IsOccupied) return false;

        IsOccupied = true;
        unit.position = transform.position;
        SetState(TileState.Occupied);
        return true;
    }

    public void Clear()
    {
        IsOccupied = false;
        SetState(TileState.Unoccupied);
    }

    // ----- Visual State -----

    /// <summary>
    /// Directly set the visual state of this tile.
    /// Called by DragManager for the Available highlight,
    /// and internally by TryOccupy / Clear.
    /// </summary>
    public void SetState(TileState state)
    {
        if (stateIcon == null) return;

        stateIcon.sprite = state switch
        {
            TileState.Occupied  => occupiedSprite,
            TileState.Available => availableSprite,
            _                   => unoccupiedSprite,   // Unoccupied
        };
    }

    /// <summary>Re-apply the icon that matches the current IsOccupied value.</summary>
    public void RefreshIcon()
    {
        SetState(IsOccupied ? TileState.Occupied : TileState.Unoccupied);
    }

    // ----- Gizmos -----

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }
}
