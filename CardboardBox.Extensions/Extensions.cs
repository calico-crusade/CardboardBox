using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardboardBox.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Splits a collection of data into chunks
        /// </summary>
        /// <typeparam name="T">The type of data to split</typeparam>
        /// <param name="inData">The data to split</param>
        /// <param name="chunkSize">The max length of a chunk</param>
        /// <returns>The chunked data</returns>
        public static IEnumerable<T[]> SplitChunks<T>(this IEnumerable<T> inData, int chunkSize)
        {
            var data = inData.ToArray();
            var chunk = new List<T>();
            for (var i = 0; i < data.Length; i++)
            {
                if (chunk.Count == chunkSize)
                {
                    yield return chunk.ToArray();
                    chunk = new List<T>();
                }

                chunk.Add(data[i]);
            }
            if (chunk.Count > 0)
                yield return chunk.ToArray();
        }

        /// <summary>
        /// Safely removes a chunk from a string ignoring out of bounds errors
        /// </summary>
        /// <param name="str">The string to remove from</param>
        /// <param name="start">The start index to remove from</param>
        /// <param name="length">How long the chunk to remove is</param>
        /// <returns>The data without the specified chunk</returns>
        public static string SafeRemove(this string str, int start, int length)
        {
            string o = "";
            for (var i = 0; i < str.Length; i++)
            {
                if (i >= start && i < start + length)
                    continue;
                o += str[i];
            }
            return o;
        }

        public static IEnumerable<T> FirstInstanceOf<T>(this IEnumerable<T> data, params T[] chunks)
        {
            var ar = data.ToArray();

            if (chunks.Length <= 0)
                return ar;

            int i = -1;
            while ((i = Array.IndexOf(ar, chunks[0], i + 1)) != -1)
            {
                bool worked = true;
                for (var x = 1; x < chunks.Length; x++)
                {
                    if (i + x >= ar.Length)
                    {
                        return ar;
                    }

                    if (!ar[i + x].Equals(chunks[x]))
                    {
                        worked = false;
                        break;
                    }
                }

                if (worked)
                    return ar.Take(i);
            }

            return ar;

        }

        public static IEnumerable<T> Extend<T>(this IEnumerable<T> data, params T[] items)
        {
            foreach (var item in data)
                yield return item;

            foreach (var item in items)
                yield return item;
        }

        /// <summary>
        /// Gets all of the posible Enum Flags from a sepific enum
        /// </summary>
        /// <typeparam name="T">The type of enum</typeparam>
        /// <param name="enumFlag">The enum to get the flags of</param>
        /// <returns>A collection of all the flags in the enum</returns>
        public static T[] AllFlags<T>(this T enumFlag) where T : IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("enumFlag must be an Enum type");

            return Enum.GetValues(typeof(T)).Cast<T>().OrderBy(t => t.ToInt32(null)).ToArray();
        }
    }
}
