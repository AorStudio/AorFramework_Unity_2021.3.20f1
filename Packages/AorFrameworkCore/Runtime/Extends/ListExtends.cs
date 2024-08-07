#pragma warning disable
#if FRAMEWORKDEF
#else

using System.Collections;
using System.Collections.Generic;

namespace AORCore
{

    /// <summary>
    /// ListExtends;
    /// </summary>
    /// Last Update : 4-1-2021
    public static class ListExtends
    {
        
        /// <summary>
        /// 快速删除
        /// </summary>
        public static void FastRemove<T>(this List<T> list, T removeTarget) where T : class
        {
            int index = list.IndexOf(removeTarget);
            if (index != -1) 
            {
                int last = list.Count - 1;
                if(index != last)
                    list[index] = list[last];
                list.RemoveAt(last);
            }
        }
        /// <summary>
        /// 快速删除指定下标对象
        /// </summary>
        public static void FastRemoveAt<T>(this List<T> list, int index) where T : class
        {
            int last = list.Count - 1;
            if(index >= 0 && index <= last)
            {
                if (index != last)
                    list[index] = list[last];
                list.RemoveAt(last);
            }
        }

    }
}




#endif