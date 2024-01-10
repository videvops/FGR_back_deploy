using System.Linq;

namespace Detenidos.Utilidades
{
    static class Extensions
    {
        public static bool ItemsEqual<TSource>(this TSource[] array1, TSource[] array2)
        {
            if (array1 == null && array2 == null)
                return true;
            if (array1 == null || array2 == null)
                return false;
            if (array1.Length != array2.Length)
                return false;
            return !array1.Except(array2).Any() && !array2.Except(array1).Any();
        }

        public static T[] Append<T>(this T[] array, T item)
        {
            if (array == null)
            {
                return new T[] { item };
            }

            T[] result = new T[array.Length + 1];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i];
            }

            result[array.Length] = item;
            return result;
        }
    }
}
