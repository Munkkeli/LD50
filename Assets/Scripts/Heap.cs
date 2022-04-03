using UnityEngine;
using System.Collections;
using System;

public interface IHeapItem<T> : IComparable<T> {
    int HeapIndex {
        get;
        set;
    }
}

public class Heap<TItem> where TItem : IHeapItem<TItem> {
    readonly TItem[] _items;
    
    public int Count { get; private set; }

    public Heap(int size) {
        _items = new TItem[size];
    }

    public void Add(TItem item) {
        item.HeapIndex = Count;
        _items[Count] = item;
        SortUp(item);
        Count++;
    }

    public TItem RemoveFirst() {
        TItem firstItem = _items[0];
        Count--;
        _items[0] = _items[Count];
        _items[0].HeapIndex = 0;
        SortDown(_items[0]);
        return firstItem;
    }

    public void UpdateItem(TItem item) {
        SortUp(item);
    }

    public bool Contains(TItem item) {
        return Equals(_items[item.HeapIndex], item);
    }

    private void SortDown(TItem item) {
        while (true) {
            var childIndexLeft = item.HeapIndex * 2 + 1;
            var childIndexRight = item.HeapIndex * 2 + 2;

            if (childIndexLeft < Count) {
                var swapIndex = childIndexLeft;

                if (childIndexRight < Count) {
                    if (_items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0) {
                        swapIndex = childIndexRight;
                    }
                }

                if (item.CompareTo(_items[swapIndex]) < 0) {
                    Swap(item, _items[swapIndex]);
                } else {
                    return;
                }

            } else {
                return;
            }

        }
    }

    private void SortUp(TItem item) {
        var parentIndex = (item.HeapIndex - 1) / 2;

        while (true) {
            var parentItem = _items[parentIndex];
            if (item.CompareTo(parentItem) > 0) {
                Swap(item, parentItem);
            } else {
                break;
            }

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    private void Swap(TItem itemA, TItem itemB) {
        _items[itemA.HeapIndex] = itemB;
        _items[itemB.HeapIndex] = itemA;
        (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
    }
}