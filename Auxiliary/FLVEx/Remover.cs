using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.FLVEx
{
    public struct Remover<TItem>
    {
        public readonly List<TItem> List;
        public int RemovedCount;
        public int Limit;

        private int lowerBracket;

        public Remover(List<TItem> list) : this(list, list.Count)
        {
        }

        public Remover(List<TItem> list, int limit)
        {
            List = list;
            Limit = limit;
            RemovedCount = 0;
            lowerBracket = -1;
        }

        /// <summary>
        /// Mark element referenced by <paramref name="index"/> as removed.
        /// </summary>
        /// <param name="index">Enumeration index, zero-based.</param>
        public void Remove(ref int index)
        {
            // update bracket if needed
            if (lowerBracket == -1)
                lowerBracket = index;

            // we're not actually deleting, so advance index
            index++;
        }

        /// <summary>
        /// Skip element referenced by <paramref name="index"/> with removing prevously marker items, if any.
        /// </summary>
        /// <param name="index">Enumeration index, zero-based.</param>
        public void Skip(ref int index)
        {
            // is we have no bracket
            if (lowerBracket == -1)
            {
                // just advance
                index++;
                return;
            }

            // only one item to delete
            if (lowerBracket == index - 1)
            {
                List.RemoveAt(lowerBracket);
                // don't update index here, it is moved backward by one and advanced by one
                // decrease limit as we actually deleting
                Limit--;
                // increase removed counter
                RemovedCount++;

                // remove bracket
                lowerBracket = -1;
                return;
            }

            // deletion block length
            int count = index - lowerBracket - 1;
            // remove block
            List.RemoveRange(lowerBracket, count);
            // decrease limit by size of deleted block
            Limit -= count;
            // update removed counter
            RemovedCount += count;
            // block is deleted so index is reset to block start
            index = lowerBracket;

            // remove bracket
            lowerBracket = -1;
        }

        /// <summary>
        /// Finishes the enumeration with deletion of all pending elements.
        /// </summary>
        /// <param name="count">Count of elements which was enumerated. Exclusive.</param>
        public void Finish(int count)
        {
            count++;
            Skip(ref count);
        }
    }
}
