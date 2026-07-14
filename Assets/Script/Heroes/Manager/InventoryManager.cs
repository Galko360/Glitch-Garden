using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Image iconImage;
        public GameObject unitPrefab;
        public Sprite iconSprite;
    }

    [Header("Fixed 5 slots")]
    [SerializeField] private Slot[] slots = new Slot[5];

    private void Awake()
    {
        RefreshUI();
    }

    public int SlotCount => slots.Length;

    public bool HasItem(int slotIndex)
    {
        if (!IsValid(slotIndex)) return false;
        return slots[slotIndex].unitPrefab != null;
    }

    public GameObject GetPrefab(int slotIndex)
    {
        if (!IsValid(slotIndex)) return null;
        return slots[slotIndex].unitPrefab;
    }

    public Sprite GetIcon(int slotIndex)
    {
        if (!IsValid(slotIndex)) return null;
        return slots[slotIndex].iconSprite;
    }

    public void Consume(int slotIndex)
    {
        if (!IsValid(slotIndex)) return;
        slots[slotIndex].unitPrefab = null;
        slots[slotIndex].iconSprite = null;
        RefreshUI();
    }

    public bool TryAddToFirstEmpty(GameObject prefab, Sprite icon)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].unitPrefab == null)
            {
                slots[i].unitPrefab = prefab;
                slots[i].iconSprite = icon;
                RefreshUI();
                return true;
            }
        }
        return false;
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].iconImage == null)
                continue;

            bool hasItem = slots[i].unitPrefab != null;
            slots[i].iconImage.enabled = hasItem;
            slots[i].iconImage.sprite = hasItem ? slots[i].iconSprite : null;
        }
    }

    private bool IsValid(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < slots.Length;
    }
}
