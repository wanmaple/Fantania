namespace FantaniaLib;

public static class StringExtensions
{
    public static string ToStandardPath(this string path)
    {
        return path.Replace('\\', '/');
    }
}