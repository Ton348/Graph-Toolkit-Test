using System;
using UnityEngine;

namespace Dreamteck
{
	public static class ArrayUtility
	{
		public static void Add<T>(ref T[] array, T item)
		{
			var newArray = new T[array.Length + 1];
			array.CopyTo(newArray, 0);
			newArray[newArray.Length - 1] = item;
			array = newArray;
		}

		public static bool Contains<T>(T[] array, T item)
		{
			for (var i = 0; i < array.Length; i++)
			{
				try
				{
					if (array[i].Equals(item))
					{
						return true;
					}
				}
				catch
				{
				}
			}

			return false;
		}

		public static int IndexOf<T>(T[] array, T value)
		{
			for (var i = 0; i < array.Length; i++)
			{
				if (array[i].Equals(value))
				{
					return i;
				}
			}

			return 0;
		}

		public static void Insert<T>(ref T[] array, int index, T item)
		{
			var newArray = new T[array.Length + 1];
			for (var i = 0; i < newArray.Length; i++)
			{
				if (i < index)
				{
					newArray[i] = array[i];
				}
				else if (i > index)
				{
					newArray[i] = array[i - 1];
				}
				else
				{
					newArray[i] = item;
				}
			}

			array = newArray;
		}


		public static void RemoveAt<T>(ref T[] array, int index)
		{
			if (array.Length == 0)
			{
				return;
			}

			var newArray = new T[array.Length - 1];
			for (var i = 0; i < array.Length; i++)
			{
				if (i < index)
				{
					newArray[i] = array[i];
				}
				else if (i > index)
				{
					newArray[i - 1] = array[i];
				}
			}

			array = newArray;
		}

		public static void ForEach<T>(this T[] source, Action<T> onLoop)
		{
			foreach (T item in source)
			{
				onLoop(item);
			}
		}

		public static void SetLength<T>(ref T[] source, int newCount)
		{
			var newArray = new T[newCount];
			for (var i = 0; i < Mathf.Min(newCount, source.Length); i++)
			{
				newArray[i] = source[i];
			}

			source = newArray;
		}

		public static void ShiftLeft<T>(this T[] source, int startIndex = 0, bool loop = true)
		{
			T startItem = source[startIndex];
			for (int i = startIndex; i < source.Length - 1; i++)
			{
				source[i] = source[i + 1];
			}

			source[source.Length - 1] = loop ? startItem : default;
		}

		public static void ShiftRight<T>(this T[] source, int startIndex = 0, bool loop = true)
		{
			T startItem = source[source.Length - 1];
			for (int i = startIndex + 1; i < source.Length; i++)
			{
				source[i] = source[i - 1];
			}

			source[startIndex] = loop ? startItem : default;
		}

		public static TArray[] QuickSort<TArray, T>(
			this TArray[] array,
			Func<TArray, T> getProperty,
			int leftIndex,
			int rightIndex) where T : IComparable
		{
			int i = leftIndex;
			int j = rightIndex;
			T pivot = getProperty(array[leftIndex]);
			while (i <= j)
			{
				while (getProperty(array[i]).CompareTo(pivot) == -1)
				{
					i++;
				}

				while (getProperty(array[j]).CompareTo(pivot) == 1)
				{
					j--;
				}

				if (i <= j)
				{
					TArray temp = array[i];
					array[i] = array[j];
					array[j] = temp;
					i++;
					j--;
				}
			}

			if (leftIndex < j)
			{
				array.QuickSort(getProperty, leftIndex, j);
			}

			if (i < rightIndex)
			{
				array.QuickSort(getProperty, i, rightIndex);
			}

			return array;
		}
	}
}