using System;
using System.Collections.Generic;

namespace Signia.OmakaseCategoryFeeder.ApiClient.Extensions
{
    public static class CollectionExtensions
    {
        public static void ForEachDo<TItem>(this IEnumerable<TItem> sequence, Action<TItem> action)
        {
            if (sequence == null)
                return;

            foreach (var current in sequence)
            {
                action(current);
            }
        }
    }
}
