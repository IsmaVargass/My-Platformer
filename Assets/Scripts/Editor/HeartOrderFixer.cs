using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HeartOrderFixer : Editor
{
    [MenuItem("Tools/Frosty Fortune/Fix Heart Order Only")]
    public static void FixHeartOrder()
    {
        HealthManager hm = Object.FindFirstObjectByType<HealthManager>();
        if (hm == null)
        {
            Debug.LogError("No se encontró HealthManager en la escena.");
            return;
        }

        // Buscamos todas las imágenes que se llamen "Heart" o similar en la UI
        Image[] allHearts = Object.FindObjectsByType<Image>(FindObjectsSortMode.None)
            .Where(img => img.name.ToLower().Contains("heart"))
            .OrderBy(img => img.transform.position.x) // Ordenar de izquierda a derecha por posición X
            .ToArray();

        if (allHearts.Length > 0)
        {
            hm.hearts = allHearts;
            EditorUtility.SetDirty(hm);
            Debug.Log($"<color=green>✔ <b>Orden de Corazones Corregido:</b> Se han asignado {allHearts.Length} corazones ordenados de izquierda a derecha.</color>");
        }
        else
        {
            Debug.LogWarning("No se encontraron objetos con 'Heart' en el nombre para ordenar.");
        }
    }
}
