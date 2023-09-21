﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RandomizerCore.Collections
{
    /// <summary>
    /// Stable binary min-heap (equal priorities -> first-in first-out)
    /// </summary>
    public class PriorityQueue<TKey, TValue> where TKey : IComparable<TKey>
    {
        private PriorityEntry[] list;
        int count;
        int version;

        private int _capacity;
        public int Capacity 
        {
            get => _capacity;
            private set
            {
                _capacity = value;
                PriorityEntry[] arr = new PriorityEntry[value];
                Array.Copy(list, arr, list.Length);
                list = arr;
            }
        }

        public int Count => count;
        /// <summary>
        /// Gets a collection that enumerates the elements of the queue in an unordered manner.
        /// </summary>
        public IReadOnlyCollection<(TKey, TValue)> UnorderedItems { get => _unorderedItems ??= new UnorderedItemsCollection(this); }
        private UnorderedItemsCollection? _unorderedItems;

        public PriorityQueue() : this(4) { }
        public PriorityQueue(int capacity)
        {
            list = new PriorityEntry[capacity];
            _capacity = capacity;
        }

        public PriorityQueue(IEnumerable<TValue> ts, Func<TValue, TKey> prioritySelector)
        {
            if (ts is ICollection<TValue> c) list = new PriorityEntry[c.Count];
            else list = new PriorityEntry[4];
            foreach (TValue t in ts) Enqueue(prioritySelector(t), t);
        }

        public void Clear()
        {
            Array.Clear(list, 0, Count);
            count = 0;
            version = 0;
        }

        public void Enqueue(TKey priority, TValue t)
        {
            int i = count;
            EnsureCapacity(++count);
            list[i] = new PriorityEntry(priority, version++, t);
            int p = GetParent(i);

            while (i > 0 && IsLessThan(i, p))
            {
                Swap(i, p);
                i = p;
                p = GetParent(i);
            }
        }

        public bool TryPeek([MaybeNullWhen(false)] out TKey priority, [MaybeNullWhen(false)] out TValue t)
        {
            if (count > 0)
            {
                priority = list[0].priority;
                t = list[0].t;
                return true;
            }
            else
            {
                priority = default;
                t = default;
                return false;
            }
        }

        public void UpdateHead(TKey newPriority)
        {
            if (Count == 0) throw new InvalidOperationException("Priority queue empty");

            list[0] = new(newPriority, list[0].version, list[0].t);

            int p = 0;
            int l; int r;
            while (true)
            {
                l = GetLeftChild(p);
                r = GetRightChild(p);

                if (l >= count) break;
                else if (r >= count)
                {
                    if (IsLessThan(l, p))
                    {
                        Swap(l, p);
                    }
                    break;
                }
                else if (IsLessThan(l, r))
                {
                    if (IsLessThan(l, p))
                    {
                        Swap(l, p);
                        p = l;
                    }
                    else break;
                }
                else
                {
                    if (IsLessThan(r, p))
                    {
                        Swap(r, p);
                        p = r;
                    }
                    else break;
                }
            }
        }

        public void ExtractMin()
        {
            ExtractMin(out _, out _);
        }

        public void ExtractMin(out TValue t)
        {
            ExtractMin(out _, out t);
        }

        public void ExtractMin(out TKey priority, out TValue t)
        {
            if (!TryExtractMin(out priority!, out t!)) throw new InvalidOperationException("Priority queue empty.");
        }

        public bool TryExtractMin([MaybeNullWhen(false)] out TKey priority, [MaybeNullWhen(false)] out TValue t)
        {
            if (count > 0)
            {
                priority = list[0].priority;
                t = list[0].t;

                list[0] = list[--count];
                list[count] = default;
                int p = 0;
                int l; int r;
                while (true)
                {
                    l = GetLeftChild(p);
                    r = GetRightChild(p);

                    if (l >= count) break;
                    else if (r >= count)
                    {
                        if (IsLessThan(l, p))
                        {
                            Swap(l, p);
                        }
                        break;
                    }
                    else if (IsLessThan(l, r))
                    {
                        if (IsLessThan(l, p))
                        {
                            Swap(l, p);
                            p = l;
                        }
                        else break;
                    }
                    else
                    {
                        if (IsLessThan(r, p))
                        {
                            Swap(r, p);
                            p = r;
                        }
                        else break;
                    }
                }

                return true;
            }
            else
            {
                priority = default;
                t = default;
                return false;
            }
        }

        /// <summary>
        /// Returns an enumerable which, when enumerated, extracts and returns the elements of the priority queue in order.
        /// </summary>
        public IEnumerable<(TKey, TValue)> GetConsumingEnumerable()
        {
            while (TryExtractMin(out TKey priority, out TValue t))
            {
                yield return (priority, t);
            }
        }

        private void EnsureCapacity(int min)
        {
            if (list.Length < min) Capacity = Math.Max(min, 2 * list.Length);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsLessThan(int i, int p)
        {
            return list[i].CompareTo(list[p]) < 0;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int i, int p)
        {
            PriorityEntry temp = list[i];
            list[i] = list[p];
            list[p] = temp;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetParent(int i) => (i - 1) / 2;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetLeftChild(int i) => 2 * i + 1;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetRightChild(int i) => 2 * i + 2;

        private readonly struct PriorityEntry : IComparable<PriorityEntry>
        {
            public readonly TKey priority;
            public readonly int version;
            public readonly TValue t;

            public PriorityEntry(TKey priority, int version, TValue t)
            {
                this.priority = priority;
                this.version = version;
                this.t = t;
            }

            public int CompareTo(PriorityEntry other)
            {
                int c = priority.CompareTo(other.priority);
                if (c != 0) return c;
                return version.CompareTo(other.version);
            }

            public static implicit operator ValueTuple<TKey, TValue>(PriorityEntry e) => (e.priority, e.t);
        }

        private class UnorderedItemsCollection : IReadOnlyCollection<(TKey, TValue)>, ICollection<(TKey, TValue)>
        {
            private readonly PriorityQueue<TKey, TValue> parent;
            public UnorderedItemsCollection(PriorityQueue<TKey, TValue> parent) => this.parent = parent;
            public int Count => parent.Count;
            bool ICollection<(TKey, TValue)>.IsReadOnly => true;
            public IEnumerator<(TKey, TValue)> GetEnumerator()
            {
                for (int i = 0; i < parent.Count; i++) yield return parent.list[i];
            }
            void ICollection<(TKey, TValue)>.Add((TKey, TValue) item) => throw new NotImplementedException();
            void ICollection<(TKey, TValue)>.Clear() => throw new NotImplementedException();
            bool ICollection<(TKey, TValue)>.Contains((TKey, TValue) item)
            {
                for (int i = 0; i < parent.Count; i++) if (Equals(parent.list[i].priority, item.Item1) && Equals(parent.list[i].t, item.Item2)) return true;
                return false;
            }
            void ICollection<(TKey, TValue)>.CopyTo((TKey, TValue)[] array, int arrayIndex)
            {
                for (int i = 0; i < Count; i++) array[arrayIndex + i] = parent.list[i];
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            bool ICollection<(TKey, TValue)>.Remove((TKey, TValue) item) => throw new NotImplementedException();
        }
    }
}
