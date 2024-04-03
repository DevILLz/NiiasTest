using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

// Copy of List<> from Microsoft in order the get the array to do uunsafe code...

namespace ConvexHull
{
    /// <summary>
    /// Represents a strongly typed list of objects that can be accessed by index. Provides methods to search, sort, and manipulate lists.To browse the .NET Framework source code for this type, see the Reference Source.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam><filterpriority>1</filterpriority>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public class List<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
        private static readonly T[] emptyArray = new T[0];
        private const int defaultCapacity = 4;
        public T[] Items;
        private int size;
        private int version;
        [NonSerialized]
        private object syncRoot;

        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold without resizing.
        /// </summary>
        /// 
        /// <returns>
        /// The number of elements that the <see cref="T:ConvexHull.List`1"/> can contain before resizing is required.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><see cref="P:ConvexHull.List`1.Capacity"/> is set to a value that is less than <see cref="P:ConvexHull.List`1.Count"/>. </exception><exception cref="T:System.OutOfMemoryException">There is not enough memory available on the system.</exception>
        public int Capacity {
            get {
                return Items.Length;
            }
            set {
                if (value < size) {
                    throw new ArgumentOutOfRangeException("value should be >= size");
                }

                if (value == Items.Length) {
                    return;
                }

                if (value > 0) {
                    T[] objArray = new T[value];
                    if (size > 0)
                        Array.Copy(Items, 0, objArray, 0, size);
                    Items = objArray;
                }
                else {
                    Items = emptyArray;
                }
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The number of elements contained in the <see cref="T:ConvexHull.List`1"/>.
        /// </returns>
        public int Count => size;

        bool IList.IsFixedSize => false;

        bool ICollection<T>.IsReadOnly => false;

        bool IList.IsReadOnly => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot {
            get {
                if (syncRoot == null) {
                    Interlocked.CompareExchange<object>(ref syncRoot, new object(), (object) null);
                }

                return syncRoot;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// 
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the element to get or set.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.-or-<paramref name="index"/> is equal to or greater than <see cref="P:ConvexHull.List`1.Count"/>. </exception>
        public T this[int index] {
            get {
                if ((uint) index > (uint) size) {
                    throw new ArgumentOutOfRangeException();
                }

                return Items[index];
            }
            set {
                if ((uint) index >= (uint) size) {
                    throw new ArgumentOutOfRangeException();
                }

                Items[index] = value;
                version = version + 1;
            }
        }

        object IList.this[int index] {
            get => (object) this[index];
            set {
                if (value == null && !(default(T) == null)) {
                    throw new ArgumentNullException("value can't be null");
                }

                try {
                    this[index] = (T) value;
                }
                catch (InvalidCastException ex) {
                    throw new ArgumentException("Wrong value type argument");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ConvexHull.List`1"/> class that is empty and has the default initial capacity.
        /// </summary>
        public List() {
            Items = List<T>.emptyArray;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ConvexHull.List`1"/> class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0. </exception>
        public List(int capacity) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException("capacity < 0");
            }

            if (capacity == 0) {
                Items = List<T>.emptyArray;
            }
            else {
                Items = new T[capacity];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ConvexHull.List`1"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param><exception cref="T:System.ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public List(IEnumerable<T> collection) {
            if (collection == null)
                throw new ArgumentNullException("collection can't be null");
            if (collection is ICollection<T> collection1) {
                int count = collection1.Count;
                if (count == 0) {
                    Items = List<T>.emptyArray;
                }
                else {
                    Items = new T[count];
                    collection1.CopyTo(Items, 0);
                    size = count;
                }
            }
            else {
                size = 0;
                Items = List<T>.emptyArray;
                foreach (var obj in collection) {
                    Add(obj);
                }
            }
        }

        private static bool IsCompatibleObject(object value) {
            if (value is T)
                return true;
            if (value == null)
                return (object) default(T) == null;
            return false;
        }

        /// <summary>
        /// Adds an object to the end of the <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// <param name="item">The object to be added to the end of the <see cref="T:ConvexHull.List`1"/>. The value can be null for reference types.</param>
        public void Add(T item) {
            if (size == Items.Length)
                EnsureCapacity(size + 1);
            T[] objArray = Items;
            int num = size;
            size = num + 1;
            int index = num;
            T obj = item;
            objArray[index] = obj;
            version = version + 1;
        }

        int IList.Add(object item) {
            if (item == null && !(default(T) == null))
                throw new ArgumentNullException("item can't be null");
            try {
                Add((T) item);
            }
            catch (InvalidCastException ex) {
                throw new ArgumentException("Wrong value type");
            }
            return Count - 1;
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="T:System.Collections.Generic.List`1"/>.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="T:System.Collections.Generic.List`1"/>. The collection itself cannot be null, but it can contain elements that are null, if type <paramref name="T"/> is a reference type.</param><exception cref="T:System.ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public void AddRange(IEnumerable<T> collection) {
            InsertRange(size, collection);
        }

        /// <summary>
        /// Returns a read-only <see cref="T:System.Collections.Generic.IList`1"/> wrapper for the current collection.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> that acts as a read-only wrapper around the current <see cref="T:ConvexHull.List`1"/>.
        /// </returns>
        public ReadOnlyCollection<T> AsReadOnly() {
            return new ReadOnlyCollection<T>((IList<T>) this);
        }

        /// <summary>
        /// Searches a range of elements in the sorted <see cref="T:System.Collections.Generic.List`1"/> for an element using the specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of <paramref name="item"/> in the sorted <see cref="T:System.Collections.Generic.List`1"/>, if <paramref name="item"/> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item"/> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.List`1.Count"/>.
        /// </returns>
        /// <param name="index">The zero-based starting index of the range to search.</param><param name="count">The length of the range to search.</param><param name="item">The object to locate. The value can be null for reference types.</param><param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1"/> implementation to use when comparing elements, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/>.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.-or-<paramref name="count"/> is less than 0. </exception><exception cref="T:System.ArgumentException"><paramref name="index"/> and <paramref name="count"/> do not denote a valid range in the <see cref="T:System.Collections.Generic.List`1"/>.</exception><exception cref="T:System.InvalidOperationException"><paramref name="comparer"/> is null, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/> cannot find an implementation of the <see cref="T:System.IComparable`1"/> generic interface or the <see cref="T:System.IComparable"/> interface for type <paramref name="T"/>.</exception>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) {
            if (index < 0)
                throw new ArgumentOutOfRangeException("Index should be >= 0");
            if (count < 0)
                throw new ArgumentOutOfRangeException("Count should be >= 0");
            if (size - index < count)
                throw new ArgumentException("size - index should be >= count");
            return Array.BinarySearch<T>(Items, index, count, item, comparer);
        }

        /// <summary>
        /// Searches the entire sorted <see cref="T:System.Collections.Generic.List`1"/> for an element using the default comparer and returns the zero-based index of the element.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of <paramref name="item"/> in the sorted <see cref="T:System.Collections.Generic.List`1"/>, if <paramref name="item"/> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item"/> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.List`1.Count"/>.
        /// </returns>
        /// <param name="item">The object to locate. The value can be null for reference types.</param><exception cref="T:System.InvalidOperationException">The default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/> cannot find an implementation of the <see cref="T:System.IComparable`1"/> generic interface or the <see cref="T:System.IComparable"/> interface for type <paramref name="T"/>.</exception>
        public int BinarySearch(T item) {
            return BinarySearch(0, Count, item, (IComparer<T>) null);
        }

        /// <summary>
        /// Searches the entire sorted <see cref="T:System.Collections.Generic.List`1"/> for an element using the specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of <paramref name="item"/> in the sorted <see cref="T:System.Collections.Generic.List`1"/>, if <paramref name="item"/> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item"/> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.List`1.Count"/>.
        /// </returns>
        /// <param name="item">The object to locate. The value can be null for reference types.</param><param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1"/> implementation to use when comparing elements.-or-null to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/>.</param><exception cref="T:System.InvalidOperationException"><paramref name="comparer"/> is null, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/> cannot find an implementation of the <see cref="T:System.IComparable`1"/> generic interface or the <see cref="T:System.IComparable"/> interface for type <paramref name="T"/>.</exception>
        public int BinarySearch(T item, IComparer<T> comparer) {
            return BinarySearch(0, Count, item, comparer);
        }

        /// <summary>
        /// Removes all elements from the <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        public void Clear() {
            if (size > 0) {
                Array.Clear((Array) Items, 0, size);
                size = 0;
            }
            version = version + 1;
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:ConvexHull.List`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:ConvexHull.List`1"/>. The value can be null for reference types.</param>
        public bool Contains(T item) {
            if ((object) item == null) {
                for (int index = 0; index < size; ++index) {
                    if ((object) Items[index] == null)
                        return true;
                }
                return false;
            }
            EqualityComparer<T> @default = EqualityComparer<T>.Default;
            for (int index = 0; index < size; ++index) {
                if (@default.Equals(Items[index], item))
                    return true;
            }
            return false;
        }

        bool IList.Contains(object item) {
            if (List<T>.IsCompatibleObject(item))
                return Contains((T) item);
            return false;
        }

        /// <summary>
        /// Converts the elements in the current <see cref="T:ConvexHull.List`1"/> to another type, and returns a list containing the converted elements.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:ConvexHull.List`1"/> of the target type containing the converted elements from the current <see cref="T:ConvexHull.List`1"/>.
        /// </returns>
        /// <param name="converter">A <see cref="T:System.Converter`2"/> delegate that converts each element from one type to another type.</param><typeparam name="TOutput">The type of the elements of the target array.</typeparam><exception cref="T:System.ArgumentNullException"><paramref name="converter"/> is null.</exception>
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) {
            if (converter == null)
                throw new ArgumentNullException("converter can't be null");
            List<TOutput> list = new List<TOutput>(size);
            for (int index = 0; index < size; ++index)
                list.Items[index] = converter(Items[index]);
            list.size = size;
            return list;
        }

        /// <summary>
        /// Copies the entire <see cref="T:ConvexHull.List`1"/> to a compatible one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:ConvexHull.List`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:ConvexHull.List`1"/> is greater than the number of elements that the destination <paramref name="array"/> can contain.</exception>
        public void CopyTo(T[] array) {
            CopyTo(array, 0);
        }

        void ICollection.CopyTo(Array array, int arrayIndex) {
            if (array != null && array.Rank != 1)
                throw new ArgumentException("array != null && array.Rank != 1");
            try {
                Array.Copy((Array) Items, 0, array, arrayIndex, size);
            }
            catch (ArrayTypeMismatchException ex) {
                throw new ArgumentException("Invalid array type");
            }
        }

        /// <summary>
        /// Copies a range of elements from the <see cref="T:ConvexHull.List`1"/> to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="index">The zero-based index in the source <see cref="T:ConvexHull.List`1"/> at which copying begins.</param><param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:ConvexHull.List`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><param name="count">The number of elements to copy.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.-or-<paramref name="arrayIndex"/> is less than 0.-or-<paramref name="count"/> is less than 0. </exception><exception cref="T:System.ArgumentException"><paramref name="index"/> is equal to or greater than the <see cref="P:ConvexHull.List`1.Count"/> of the source <see cref="T:ConvexHull.List`1"/>.-or-The number of elements from <paramref name="index"/> to the end of the source <see cref="T:ConvexHull.List`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>. </exception>
        public void CopyTo(int index, T[] array, int arrayIndex, int count) {
            if (size - index < count)
                throw new ArgumentException("size - index < count");
            Array.Copy((Array) Items, index, (Array) array, arrayIndex, count);
        }

        /// <summary>
        /// Copies the entire <see cref="T:ConvexHull.List`1"/> to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:ConvexHull.List`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:ConvexHull.List`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex) {
            Array.Copy((Array) Items, 0, (Array) array, arrayIndex, size);
        }

        private void EnsureCapacity(int min) {
            if (Items.Length >= min)
                return;
            int num = Items.Length == 0 ? 4 : Items.Length * 2;
            if ((uint) num > 2146435071U)
                num = 2146435071;
            if (num < min)
                num = min;
            Capacity = num;
        }

        /// <summary>
        /// Determines whether the <see cref="T:ConvexHull.List`1"/> contains elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="T:ConvexHull.List`1"/> contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.
        /// </returns>
        /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the elements to search for.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception>
        public bool Exists(Predicate<T> match) {
            return FindIndex(match) != -1;
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="T:System.Collections.Generic.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T"/>.
        /// </returns>
        /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the element to search for.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception>
        public T Find(Predicate<T> match) {
            if (match == null)
                throw new ArgumentNullException("match can't be null");
            for (int index = 0; index < size; ++index) {
                if (match(Items[index]))
                    return Items[index];
            }
            return default(T);
        }

        /// <summary>
        /// Retrieves all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:ConvexHull.List`1"/> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:ConvexHull.List`1"/>.
        /// </returns>
        /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the elements to search for.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception>
        public List<T> FindAll(Predicate<T> match) {
            if (match == null)
                throw new ArgumentNullException("match can't be null");
            List<T> list = new List<T>();
            for (int index = 0; index < size; ++index) {
                if (match(Items[index]))
                    list.Add(Items[index]);
            }
            return list;
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match"/>, if found; otherwise, –1.
        /// </returns>
        /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the element to search for.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int FindIndex(Predicate<T> match) {
            return FindIndex(0, size, match);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:ConvexHull.List`1"/> that extends from the specified index to the last element.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match"/>, if found; otherwise, –1.
        /// </returns>
        /// <param name="startIndex">The zero-based starting index of the search.</param><param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the element to search for.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="startIndex"/> is outside the range of valid indexes for the <see cref="T:ConvexHull.List`1"/>.</exception>
        public int FindIndex(int startIndex, Predicate<T> match) {
            return FindIndex(startIndex, size - startIndex, match);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:ConvexHull.List`1"/> that starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match"/>, if found; otherwise, –1.
        /// </returns>
        /// <param name="startIndex">The zero-based starting index of the search.</param><param name="count">The number of elements in the section to search.</param><param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the element to search for.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="startIndex"/> is outside the range of valid indexes for the <see cref="T:ConvexHull.List`1"/>.-or-<paramref name="count"/> is less than 0.-or-<paramref name="startIndex"/> and <paramref name="count"/> do not specify a valid section in the <see cref="T:ConvexHull.List`1"/>.</exception>
        public int FindIndex(int startIndex, int count, Predicate<T> match) {
            if ((uint) startIndex > (uint) size)
                throw new ArgumentOutOfRangeException("(uint)startIndex > (uint)size");
            if (count < 0 || startIndex > size - count)
                throw new ArgumentOutOfRangeException("count < 0 || startIndex > size - count");
            if (match == null)
                throw new ArgumentNullException("match can't be null");
            int num = startIndex + count;
            for (int index = startIndex; index < num; ++index) {
                if (match(Items[index]))
                    return index;
            }
            return -1;
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="T:System.Collections.Generic.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T"/>.
        /// </returns>
        /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the element to search for.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception>
        public T FindLast(Predicate<T> match) {
            if (match == null)
                throw new ArgumentNullException("match can't be null");
            for (int index = size - 1; index >= 0; --index) {
                if (match(Items[index]))
                    return Items[index];
            }
            return default(T);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match"/>, if found; otherwise, –1.
        /// </returns>
        /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the element to search for.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int FindLastIndex(Predicate<T> match) {
            return FindLastIndex(size - 1, size, match);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:ConvexHull.List`1"/> that extends from the first element to the specified index.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match"/>, if found; otherwise, –1.
        /// </returns>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param><param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the element to search for.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="startIndex"/> is outside the range of valid indexes for the <see cref="T:ConvexHull.List`1"/>.</exception>
        public int FindLastIndex(int startIndex, Predicate<T> match) {
            int startIndex1 = startIndex;
            int num = 1;
            int count = startIndex1 + num;
            Predicate<T> match1 = match;
            return FindLastIndex(startIndex1, count, match1);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:ConvexHull.List`1"/> that contains the specified number of elements and ends at the specified index.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match"/>, if found; otherwise, –1.
        /// </returns>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param><param name="count">The number of elements in the section to search.</param><param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the element to search for.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="startIndex"/> is outside the range of valid indexes for the <see cref="T:ConvexHull.List`1"/>.-or-<paramref name="count"/> is less than 0.-or-<paramref name="startIndex"/> and <paramref name="count"/> do not specify a valid section in the <see cref="T:ConvexHull.List`1"/>.</exception>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match) {
            if (match == null)
                throw new ArgumentNullException("match can't be null");
            if (size == 0) {
                if (startIndex != -1)
                    throw new ArgumentOutOfRangeException("startIndex != -1");
            }
            else if ((uint) startIndex >= (uint) size)
                throw new ArgumentOutOfRangeException("(uint)startIndex >= (uint)size");
            if (count < 0 || startIndex - count + 1 < 0)
                throw new ArgumentOutOfRangeException("count < 0 || startIndex - count + 1 < 0");
            int num = startIndex - count;
            for (int index = startIndex; index > num; --index) {
                if (match(Items[index]))
                    return index;
            }
            return -1;
        }

        /// <summary>
        /// Performs the specified action on each element of the <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// <param name="action">The <see cref="T:System.Action`1"/> delegate to perform on each element of the <see cref="T:ConvexHull.List`1"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="action"/> is null.</exception><exception cref="T:System.InvalidOperationException">An element in the collection has been modified. CautionThis exception is thrown starting with the .NET Framework 4.5. </exception>
        public void ForEach(Action<T> action) {
            if (action == null)
                throw new ArgumentNullException("action can't be null");

            for (int index = 0; index < size; ++index)
                action(Items[index]);

            return;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:ConvexHull.List`1.Enumerator"/> for the <see cref="T:ConvexHull.List`1"/>.
        /// </returns>
        public List<T>.Enumerator GetEnumerator() {
            return new List<T>.Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return (IEnumerator<T>) new List<T>.Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator) new List<T>.Enumerator(this);
        }

        /// <summary>
        /// Creates a shallow copy of a range of elements in the source <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A shallow copy of a range of elements in the source <see cref="T:ConvexHull.List`1"/>.
        /// </returns>
        /// <param name="index">The zero-based <see cref="T:ConvexHull.List`1"/> index at which the range starts.</param><param name="count">The number of elements in the range.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.-or-<paramref name="count"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the <see cref="T:ConvexHull.List`1"/>.</exception>
        public List<T> GetRange(int index, int count) {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index < 0");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count < 0");
            if (size - index < count)
                throw new ArgumentException("size - index < count");
            List<T> list = new List<T>(count);
            Array.Copy((Array) Items, index, (Array) list.Items, 0, count);
            list.size = count;
            return list;
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="item"/> within the entire <see cref="T:ConvexHull.List`1"/>, if found; otherwise, –1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:ConvexHull.List`1"/>. The value can be null for reference types.</param>
        public int IndexOf(T item) {
            return Array.IndexOf<T>(Items, item, 0, size);
        }

        int IList.IndexOf(object item) {
            if (List<T>.IsCompatibleObject(item))
                return IndexOf((T) item);
            return -1;
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:ConvexHull.List`1"/> that extends from the specified index to the last element.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="item"/> within the range of elements in the <see cref="T:ConvexHull.List`1"/> that extends from <paramref name="index"/> to the last element, if found; otherwise, –1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:ConvexHull.List`1"/>. The value can be null for reference types.</param><param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is outside the range of valid indexes for the <see cref="T:ConvexHull.List`1"/>.</exception>
        public int IndexOf(T item, int index) {
            if (index > size)
                throw new ArgumentOutOfRangeException("index > size");
            return Array.IndexOf<T>(Items, item, index, size - index);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:ConvexHull.List`1"/> that starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="item"/> within the range of elements in the <see cref="T:ConvexHull.List`1"/> that starts at <paramref name="index"/> and contains <paramref name="count"/> number of elements, if found; otherwise, –1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:ConvexHull.List`1"/>. The value can be null for reference types.</param><param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param><param name="count">The number of elements in the section to search.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is outside the range of valid indexes for the <see cref="T:ConvexHull.List`1"/>.-or-<paramref name="count"/> is less than 0.-or-<paramref name="index"/> and <paramref name="count"/> do not specify a valid section in the <see cref="T:ConvexHull.List`1"/>.</exception>
        public int IndexOf(T item, int index, int count) {
            if (index > size)
                throw new ArgumentOutOfRangeException("index > size");
            if (count < 0 || index > size - count)
                throw new ArgumentOutOfRangeException("count < 0 || index > size - count");
            return Array.IndexOf<T>(Items, item, index, count);
        }

        /// <summary>
        /// Inserts an element into the <see cref="T:ConvexHull.List`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert. The value can be null for reference types.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.-or-<paramref name="index"/> is greater than <see cref="P:ConvexHull.List`1.Count"/>.</exception>
        public void Insert(int index, T item) {
            if ((uint) index > (uint) size)
                throw new ArgumentOutOfRangeException("(uint)index > (uint)size");
            if (size == Items.Length)
                EnsureCapacity(size + 1);
            if (index < size)
                Array.Copy((Array) Items, index, (Array) Items, index + 1, size - index);
            Items[index] = item;
            size = size + 1;
            version = version + 1;
        }

        void IList.Insert(int index, object item) {
            if (item == null && !(default(T) == null))
                throw new ArgumentNullException("item can't be null");

            try {
                Insert(index, (T) item);
            }
            catch (InvalidCastException ex) {
                throw new ArgumentException("item is of the wrong type");
            }
        }

        /// <summary>
        /// Inserts the elements of a collection into the <see cref="T:System.Collections.Generic.List`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param><param name="collection">The collection whose elements should be inserted into the <see cref="T:System.Collections.Generic.List`1"/>. The collection itself cannot be null, but it can contain elements that are null, if type <paramref name="T"/> is a reference type.</param><exception cref="T:System.ArgumentNullException"><paramref name="collection"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.-or-<paramref name="index"/> is greater than <see cref="P:System.Collections.Generic.List`1.Count"/>.</exception>
        public void InsertRange(int index, IEnumerable<T> collection) {
            if (collection == null)
                throw new ArgumentNullException("collection == null");
            if ((uint) index > (uint) size)
                throw new ArgumentOutOfRangeException("(uint)index > (uint)size");
            ICollection<T> collection1 = collection as ICollection<T>;
            if (collection1 != null) {
                int count = collection1.Count;
                if (count > 0) {
                    EnsureCapacity(size + count);
                    if (index < size)
                        Array.Copy((Array) Items, index, (Array) Items, index + count, size - index);
                    if (this == collection1) {
                        T[] objArray1 = Items;
                        int sourceIndex = 0;
                        T[] objArray2 = Items;
                        int num = index;
                        Array.Copy((Array) objArray1, sourceIndex, (Array) objArray2, num, num);
                        Array.Copy((Array) Items, index + count, (Array) Items, index * 2, size - index);
                    }
                    else {
                        T[] array = new T[count];
                        collection1.CopyTo(array, 0);
                        array.CopyTo((Array) Items, index);
                    }
                    size = size + count;
                }
            }
            else {
                foreach (T obj in collection)
                    Insert(index++, obj);
            }
            version = version + 1;
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the entire <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the last occurrence of <paramref name="item"/> within the entire the <see cref="T:ConvexHull.List`1"/>, if found; otherwise, –1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:ConvexHull.List`1"/>. The value can be null for reference types.</param>
        public int LastIndexOf(T item) {
            if (size == 0)
                return -1;
            return LastIndexOf(item, size - 1, size);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:ConvexHull.List`1"/> that extends from the first element to the specified index.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the last occurrence of <paramref name="item"/> within the range of elements in the <see cref="T:ConvexHull.List`1"/> that extends from the first element to <paramref name="index"/>, if found; otherwise, –1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:ConvexHull.List`1"/>. The value can be null for reference types.</param><param name="index">The zero-based starting index of the backward search.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is outside the range of valid indexes for the <see cref="T:ConvexHull.List`1"/>. </exception>
        public int LastIndexOf(T item, int index) {
            if (index >= size)
                throw new ArgumentOutOfRangeException("index should be less than the size");
            T obj = item;
            int index1 = index;
            int num = 1;
            int count = index1 + num;
            return LastIndexOf(obj, index1, count);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:ConvexHull.List`1"/> that contains the specified number of elements and ends at the specified index.
        /// </summary>
        /// 
        /// <returns>
        /// The zero-based index of the last occurrence of <paramref name="item"/> within the range of elements in the <see cref="T:ConvexHull.List`1"/> that contains <paramref name="count"/> number of elements and ends at <paramref name="index"/>, if found; otherwise, –1.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:ConvexHull.List`1"/>. The value can be null for reference types.</param><param name="index">The zero-based starting index of the backward search.</param><param name="count">The number of elements in the section to search.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is outside the range of valid indexes for the <see cref="T:ConvexHull.List`1"/>.-or-<paramref name="count"/> is less than 0.-or-<paramref name="index"/> and <paramref name="count"/> do not specify a valid section in the <see cref="T:ConvexHull.List`1"/>. </exception>
        public int LastIndexOf(T item, int index, int count) {
            if (Count != 0 && index < 0)
                throw new ArgumentOutOfRangeException("count can't be 0 and index < 0");
            if (Count != 0 && count < 0)
                throw new ArgumentOutOfRangeException("Count != 0 && count < 0");
            if (size == 0)
                return -1;
            if (index >= size)
                throw new ArgumentOutOfRangeException("index >= size");
            if (count > index + 1)
                throw new ArgumentOutOfRangeException("count > index + 1");
            return Array.LastIndexOf<T>(Items, item, index, count);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// true if <paramref name="item"/> is successfully removed; otherwise, false.  This method also returns false if <paramref name="item"/> was not found in the <see cref="T:ConvexHull.List`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:ConvexHull.List`1"/>. The value can be null for reference types.</param>
        public bool Remove(T item) {
            int index = IndexOf(item);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        void IList.Remove(object item) {
            if (!List<T>.IsCompatibleObject(item))
                return;
            Remove((T) item);
        }

        /// <summary>
        /// Removes all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// 
        /// <returns>
        /// The number of elements removed from the <see cref="T:ConvexHull.List`1"/> .
        /// </returns>
        /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the elements to remove.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int RemoveAll(Predicate<T> match) {
            if (match == null)
                throw new ArgumentNullException("match can't be null");
            int index1 = 0;
            while (index1 < size && !match(Items[index1]))
                ++index1;
            if (index1 >= size)
                return 0;
            int index2 = index1 + 1;
            while (index2 < size) {
                while (index2 < size && match(Items[index2]))
                    ++index2;
                if (index2 < size)
                    Items[index1++] = Items[index2++];
            }
            Array.Clear((Array) Items, index1, size - index1);
            int num = size - index1;
            size = index1;
            version = version + 1;
            return num;
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.-or-<paramref name="index"/> is equal to or greater than <see cref="P:ConvexHull.List`1.Count"/>.</exception>
        public void RemoveAt(int index) {
            if ((uint) index >= (uint) size)
                throw new ArgumentOutOfRangeException("index should be less than the size");
            size = size - 1;
            if (index < size)
                Array.Copy((Array) Items, index + 1, (Array) Items, index, size - index);
            Items[size] = default(T);
            version = version + 1;
        }

        /// <summary>
        /// Removes a range of elements from the <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param><param name="count">The number of elements to remove.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.-or-<paramref name="count"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the <see cref="T:ConvexHull.List`1"/>.</exception>
        public void RemoveRange(int index, int count) {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index should be >= 0");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count should be >= 0");
            if (size - index < count)
                throw new ArgumentException("size - index should be >= count");
            if (count <= 0)
                return;
            int num = size;
            size = size - count;
            if (index < size)
                Array.Copy((Array) Items, index + count, (Array) Items, index, size - index);
            Array.Clear((Array) Items, size, count);
            version = version + 1;
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:ConvexHull.List`1"/> to a new array.
        /// </summary>
        /// 
        /// <returns>
        /// An array containing copies of the elements of the <see cref="T:ConvexHull.List`1"/>.
        /// </returns>
        public T[] ToArray() {
            T[] objArray = new T[size];
            Array.Copy((Array) Items, 0, (Array) objArray, 0, size);
            return objArray;
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="T:ConvexHull.List`1"/>, if that number is less than a threshold value.
        /// </summary>
        public void TrimExcess() {
            if (size >= (int) ((double) Items.Length * 0.9))
                return;
            Capacity = size;
        }

        /// <summary>
        /// Determines whether every element in the <see cref="T:ConvexHull.List`1"/> matches the conditions defined by the specified predicate.
        /// </summary>
        /// 
        /// <returns>
        /// true if every element in the <see cref="T:ConvexHull.List`1"/> matches the conditions defined by the specified predicate; otherwise, false. If the list has no elements, the return value is true.
        /// </returns>
        /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions to check against the elements.</param><exception cref="T:System.ArgumentNullException"><paramref name="match"/> is null.</exception>
        public bool TrueForAll(Predicate<T> match) {
            if (match == null)
                throw new ArgumentNullException("match can't be null");
            for (int index = 0; index < size; ++index) {
                if (!match(Items[index]))
                    return false;
            }
            return true;
        }

        internal static IList<T> Synchronized(List<T> list) {
            return (IList<T>) new List<T>.SynchronizedList(list);
        }

        [Serializable]
        internal class SynchronizedList : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
        {
            private List<T> list;
            private object root;

            public int Count {
                get {
                    object obj = root;
                    bool lockTaken = false;
                    try {
                        Monitor.Enter(obj, ref lockTaken);
                        return list.Count;
                    }
                    finally {
                        if (lockTaken)
                            Monitor.Exit(obj);
                    }
                }
            }

            public bool IsReadOnly {
                get {
                    return ((ICollection<T>) list).IsReadOnly;
                }
            }

            public T this[int index] {
                get {
                    object obj = root;
                    bool lockTaken = false;
                    try {
                        Monitor.Enter(obj, ref lockTaken);
                        return list[index];
                    }
                    finally {
                        if (lockTaken)
                            Monitor.Exit(obj);
                    }
                }
                set {
                    object obj = root;
                    bool lockTaken = false;
                    try {
                        Monitor.Enter(obj, ref lockTaken);
                        list[index] = value;
                    }
                    finally {
                        if (lockTaken)
                            Monitor.Exit(obj);
                    }
                }
            }

            internal SynchronizedList(List<T> list) {
                list = list;
                root = ((ICollection) list).SyncRoot;
            }

            public void Add(T item) {
                object obj = root;
                bool lockTaken = false;
                try {
                    Monitor.Enter(obj, ref lockTaken);
                    list.Add(item);
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(obj);
                }
            }

            public void Clear() {
                object obj = root;
                bool lockTaken = false;
                try {
                    Monitor.Enter(obj, ref lockTaken);
                    list.Clear();
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(obj);
                }
            }

            public bool Contains(T item) {
                object obj = root;
                bool lockTaken = false;
                try {
                    Monitor.Enter(obj, ref lockTaken);
                    return list.Contains(item);
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(obj);
                }
            }

            public void CopyTo(T[] array, int arrayIndex) {
                object obj = root;
                bool lockTaken = false;
                try {
                    Monitor.Enter(obj, ref lockTaken);
                    list.CopyTo(array, arrayIndex);
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(obj);
                }
            }

            public bool Remove(T item) {
                object obj = root;
                bool lockTaken = false;
                try {
                    Monitor.Enter(obj, ref lockTaken);
                    return list.Remove(item);
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(obj);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                object obj = root;
                bool lockTaken = false;
                try {
                    Monitor.Enter(obj, ref lockTaken);
                    return (IEnumerator) list.GetEnumerator();
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(obj);
                }
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator() {
                object obj = root;
                bool lockTaken = false;
                try {
                    Monitor.Enter(obj, ref lockTaken);
                    return ((IEnumerable<T>) list).GetEnumerator();
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(obj);
                }
            }

            public int IndexOf(T item) {
                object obj = root;
                bool lockTaken = false;
                try {
                    Monitor.Enter(obj, ref lockTaken);
                    return list.IndexOf(item);
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(obj);
                }
            }

            public void Insert(int index, T item) {
                object obj = root;
                bool lockTaken = false;
                try {
                    Monitor.Enter(obj, ref lockTaken);
                    list.Insert(index, item);
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(obj);
                }
            }

            public void RemoveAt(int index) {
                object obj = root;
                bool lockTaken = false;
                try {
                    Monitor.Enter(obj, ref lockTaken);
                    list.RemoveAt(index);
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(obj);
                }
            }
        }

        /// <summary>
        /// Enumerates the elements of a <see cref="T:ConvexHull.List`1"/>.
        /// </summary>
        [Serializable]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private List<T> list;
            private int index;
            private int version;
            private T current;

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            /// 
            /// <returns>
            /// The element in the <see cref="T:ConvexHull.List`1"/> at the current position of the enumerator.
            /// </returns>
            public T Current {
                get {
                    return current;
                }
            }

            object IEnumerator.Current {
                get {
                    if (index == 0 || index == list.size + 1)
                        throw new InvalidOperationException("Can't get current");
                    return (object) Current;
                }
            }

            internal Enumerator(List<T> list) {
                this.list = list;
                index = 0;
                version = list.version;
                current = default(T);
            }

            /// <summary>
            /// Releases all resources used by the <see cref="T:ConvexHull.List`1.Enumerator"/>.
            /// </summary>
            public void Dispose() {
            }

            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="T:ConvexHull.List`1"/>.
            /// </summary>
            /// 
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public bool MoveNext() {
                List<T> list = this.list;
                if (version != list.version || (uint) index >= (uint) list.size) {
                    return MoveNextRare();
                }

                current = list.Items[index];
                index++;
                return true;
            }

            private bool MoveNextRare() {
                if (version != list.version) {
                    throw new InvalidOperationException("Can't MoveNextRare if version are different");
                }

                index = list.size + 1;
                current = default;
                return false;
            }

            void IEnumerator.Reset() {
                if (version != list.version) {
                    throw new InvalidOperationException("Can't reset while version are different");
                }

                index = 0;
                current = default;
            }
        }
    }
}
