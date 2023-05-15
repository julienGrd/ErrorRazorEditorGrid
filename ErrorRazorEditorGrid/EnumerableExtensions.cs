using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Is.Geckos.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> pLstSource)
        {
            if (pLstSource.Any())
            {
                Random lRandom = new Random();
                int lIdx = lRandom.Next(0, pLstSource.Count());
                return pLstSource.ElementAt(lIdx);
            }
            else
            {
                return default(T);
            }
        }


        /// <summary>
        /// divise la liste en n groupes passé en paramètre
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int size)
        {
            if (!source?.Any() ?? false)
            {
                return Enumerable.Empty<IEnumerable<T>>();
            }
            var chunks = new List<IEnumerable<T>>();

            //if (source.Count() % size > 0)
            //    chunkCount++;

            var itemsByGroup = (int)Math.Ceiling(((double)source.Count() / size));

            for (var i = 0; i < size; i++)
            {
                chunks.Add(source.Skip(i * itemsByGroup).Take(itemsByGroup));
            }
                

            return chunks;
        }

        public static Type GetItemType<T>(this IEnumerable<T> source)
        {
            return typeof(T);
        }

        public static T GetItemAt<T>(this IEnumerable<T> source, int idx)
        {
            if (idx >= 0 && idx < source.Count())
            {
                return source.ElementAt(idx);
            }
            else
            {
                return default;
            }
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source ?? Enumerable.Empty<T>());
        }

    }
}
