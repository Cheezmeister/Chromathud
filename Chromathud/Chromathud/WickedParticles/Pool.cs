using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WickedLibrary.Collections
{
    public interface INode
    {
        INode Next { get; set; }

        /// <summary>
        /// Clears the state of the object
        /// </summary>
        void Reset();
    }


    /// <summary>
    /// Resource pool for generating default items at initialization that 
    /// can be pulled from and returned to the pool during runtime without
    /// generating garbage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T> where T : class, INode, new()
    {
        /// <summary>
        /// Total number of items handled by the pool
        /// </summary>
        public int Size { get { return size; } }
        int size;

        /// <summary>
        /// Current number of items left in the pool
        /// </summary>
        public int Count { get { return count; } }
        int count;

        T FreeList;
        T FreeListEnd;

        /// <summary>
        /// Creates a pool and fills it with items
        /// </summary>
        /// <param name="size">Number of items in the pool</param>
        public Pool(int size)
        {
            this.size = size;

            //Build the linked list
            T newNode;
            for (int i = 0; i < size; i++)
            {
                newNode = new T();
                Return(newNode);
            }
        }

        /// <summary>
        /// Makes the pool larger by generating new items
        /// </summary>
        /// <param name="numToAdd"></param>
        public void Grow(int numToAdd)
        {
            for (int i = 0; i < numToAdd; i++)
            {
                Return(new T());
            }
        }

        /// <summary>
        /// Gets an item from the pool. Item must be returned to
        /// the pool when no longer needed.
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            if (FreeListEnd == null)
                return null;

            //Grab from beginning of list
            T front = FreeList;
            T next = (T)FreeList.Next;

            front.Next = null;
            FreeList = next;

            if (FreeList == null || FreeList.Next == null)
                FreeListEnd = FreeList;

            count--;

            return front;
        }

        /// <summary>
        /// Returns an item to the pool and resets it
        /// </summary>
        /// <param name="retVal"></param>
        public void Return(T retVal)
        {
            if (retVal == null)
                throw new ArgumentNullException("retVal");

            retVal.Reset();
            retVal.Next = null;

            //Insert item at the end of the list

            if (FreeList == null)
                FreeList = retVal;
            else
                FreeListEnd.Next = retVal;
            FreeListEnd = retVal;

            count++;
        }
    }
}
