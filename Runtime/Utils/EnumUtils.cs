using System;
using System.Collections.Generic;
using System.Linq;

namespace giorgiokalmund.Dora.Utils
{
    public static class EnumUtils
    {
        public static T GetRandom<T>(params T[] excluding) where T : Enum
        {
            var all = Enum.GetValues(typeof(T)).Cast<T>().ToHashSet();
            
            foreach (var val in excluding)
                all.Remove(val);
            
            if (all.Count == 0)
                throw new Exception("EnumUtils.GetRandom<" + typeof(T).Name + ">(): Cannot exclude all available elements!");

            var list = all.ToList();
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        
        public static T GetNext<T>(this T enumValue) where T : Enum
        {
            var all = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            int indexOfElement = all.IndexOf(enumValue);
            return all[(indexOfElement + 1) % all.Count];
        }
        
        public static List<T> GetAll<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }
        
        public static T Get<T>(int index) where T : Enum
        {
            return GetAll<T>()[index];
        }
        
        public static T GetFirst<T>() where T : Enum
        {
            return GetAll<T>().First();
        }
        
        public static T GetLast<T>() where T : Enum
        {
            return GetAll<T>().Last();
        }
        
        public static T GetPrevious<T>(this T enumValue) where T : Enum
        {
            var all = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            int indexOfElement = all.IndexOf(enumValue);
            return all[(indexOfElement - 1 + all.Count) % all.Count];
        }
        
        public static int GetIndex<T>(this T enumValue) where T : Enum
        {
            var all = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            return all.IndexOf(enumValue);
        }
    }
}