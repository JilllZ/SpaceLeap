using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public static class RandomUtil {

    public static T chooseRandom<T>(this IEnumerable<T> enumerable) {
        T[] tArray = enumerable.ToArray();
        int index = UnityEngine.Random.Range(0, tArray.Length);
        return tArray[index];
    }

    public static T chooseRandom<T>(this IEnumerable<T> enumerable, Func<T, bool> isValid) {
        List<T> tList = enumerable.ToList();
        while (true) {
            int index = UnityEngine.Random.Range(0, tList.Count);
            if (isValid(tList[index])) {
                return tList[index];
            }

            if (tList.Count == 1) {
                throw new System.Exception("No valid elements to be chosen!");
            }
            tList[index] = tList[tList.Count - 1];
            tList.RemoveAt(tList.Count - 1);
        }
    }
}
