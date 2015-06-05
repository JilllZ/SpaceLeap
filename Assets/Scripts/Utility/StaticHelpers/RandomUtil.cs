using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public static class RandomUtil {

    public static T chooseRandom<T>(this IEnumerable<T> enumerable) {
        T[] tArray = enumerable.ToArray();
        int index = Random.Range(0, tArray.Length);
        return tArray[index];
    }
}
