namespace FantaniaLib;

public static class IListExtensions
{
    public static int IndexOf<T>(this IReadOnlyList<T> self, T item)
    {
        for (int i = 0; i < self.Count; i++)
        {
            if (self[i]!.Equals(item))
                return i;
        }
        return -1;
    }

    public static bool RemoveFast<T>(this IList<T> self, T item)
    {
        int index = self.IndexOf(item);
        if (index >= 0)
        {
            RemoveAtFast(self, index);
            return true;
        }
        return false;
    }

    public static void RemoveAtFast<T>(this IList<T> self, int index)
    {
        if (index != self.Count - 1)
        {
            self[index] = self[self.Count - 1];
        }
        self.RemoveAt(self.Count - 1);
    }

    public static void RemoveLast<T>(this IList<T> self)
    {
        if (self.Count > 0)
        {
            self.RemoveAt(self.Count - 1);
        }
    }

    public static void StableSort<T>(this IList<T> self, IComparer<T>? comparer = null)
    {
        if (comparer == null)
        {
            comparer = Comparer<T>.Default;
        }
        StableSort(self, 0, self.Count - 1, comparer);
    }

    private static void StableSort<T>(IList<T> input, int p, int r, IComparer<T> comparer)
    {
        if (p < r)
        {
            int q = (p + r) / 2;
            StableSort(input, p, q, comparer);
            StableSort(input, q + 1, r, comparer);
            InternalMerge(input, p, q, r, comparer);
        }
    }

    private static void InternalMerge<T>(IList<T> input, int p, int q, int r, IComparer<T> comparer)
    {
        int i, j, k;
        int n1 = q - p + 1;
        int n2 = r - q;
        T[] L = new T[n1];
        T[] R = new T[n2];
        for (i = 0; i < n1; i++)
        {
            L[i] = input[p + i];
        }
        for (j = 0; j < n2; j++)
        {
            R[j] = input[q + 1 + j];
        }
        i = 0;
        j = 0;
        k = p;
        while (i < n1 && j < n2)
        {
            if (comparer.Compare(L[i], R[j]) <= 0)
            {
                input[k] = L[i];
                i++;
            }
            else
            {
                input[k] = R[j];
                j++;
            }
            k++;
        }
        while (i < n1)
        {
            input[k] = L[i];
            i++;
            k++;
        }
        while (j < n2)
        {
            input[k] = R[j];
            j++;
            k++;
        }
    }

    public static void StableSort<T>(this IList<T> self, Comparison<T> comparison)
    {
        StableSort(self, 0, self.Count - 1, Comparer<T>.Create(comparison));
    }
}