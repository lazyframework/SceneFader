#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class Select : EditorWindow
{
    [MenuItem("Edit/Select Children of Selected GameObject %a")]
    static void SelectChildren()
    {
        GameObject activeObj = Selection.activeGameObject;
        if (activeObj != null)
        {
            List<Object> selectionObjects = new List<Object>();
            
            // Ambil semua anak di dalam objek yang dipilih, tapi abaikan objek utamanya (Demo)
            foreach (Transform child in activeObj.GetComponentsInChildren<Transform>())
            {
                if (child != activeObj.transform)
                {
                    selectionObjects.Add(child.gameObject);
                }
            }
            
            // Terapkan seleksi ke Hierarchy
            Selection.objects = selectionObjects.ToArray();
        }
        else
        {
            Debug.LogWarning("Pilih GameObject 'Demo' terlebih dahulu di Hierarchy!");
        }
    }
}
#endif
