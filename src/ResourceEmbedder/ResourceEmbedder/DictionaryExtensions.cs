using System;
using System.Collections.Generic;

namespace ResourceEmbedder
{
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Returns a dictionary.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="collection"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static Dictionary<TKey, List<TValue>> ToMultiDictionary<TKey, TValue>(this IEnumerable<TValue> collection, Func<TValue, TKey> selector)
		{
			var dict = new Dictionary<TKey, List<TValue>>();
			foreach (var value in collection)
			{
				var key = selector(value);
				if (dict.ContainsKey(key))
				{
					dict[key].Add(value);
				}
				else
				{
					dict.Add(key, new List<TValue> { value });
				}
			}
			return dict;
		}
	}
}