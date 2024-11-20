using AppScaffolding;
using System.Collections;

namespace LibationUiBase;

public static class BindingListExtensions
{
    public static void Swap(this IList list, int first, int second)
    {
        (list[first], list[second]) = (list[second], list[first]);
    }

    public static void MoveDown(this IList list, object obj)
    {
        var idx = list.IndexOf(obj);
        Ensure.IsValid(nameof(obj), idx != -1, "Object not in list");
        Ensure.IsValid(nameof(obj), idx < list.Count - 1, "Object already at end of list");
        list.Swap(idx, idx + 1);
    }

    public static void MoveUp(this IList list, object obj)
    {
        var idx = list.IndexOf(obj);
        Ensure.IsValid(nameof(obj), idx != -1, "Object not in list");
        Ensure.IsValid(nameof(obj), idx > 0, "Object already at beginning of list");
        list.Swap(idx, idx - 1);
    }
}