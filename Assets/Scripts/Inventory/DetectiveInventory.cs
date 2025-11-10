using System;
using System.Collections.Generic;
using UnityEngine;

public class DetectiveInventory : MonoBehaviour
{
    [SerializeField] private int capacity = 7;
    [SerializeField] private InventoryUI ui;
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    public int Capacity => capacity;
    public IReadOnlyList<ItemData> Items => items;

    public event Action OnInventoryChanged;

    public bool AddItem(ItemData data)
    {
        if (items.Count >= capacity) return false;
        items.Add(data);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItemAt(int index)
    {
        if (index < 0 || index >= items.Count) return false;
        items.RemoveAt(index);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }

    // public void TryAdd()
    // {
        
    // }
}