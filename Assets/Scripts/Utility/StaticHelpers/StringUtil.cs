using UnityEngine;
using System.Collections;

public class StringUtil : MonoBehaviour {

    public static string capitalize(string s) {
        string first = s.Substring(0, 1);
        string rest = s.Substring(1);
        return first.ToUpper() + rest;
    }
}
