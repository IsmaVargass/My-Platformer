using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSetupAutomation : EditorWindow
{
    [MenuItem("Tools/Frosty Fortune/Fix Everything Automatically")]
    public static void SetupGame()
    {
        Debug.Log("<color=orange><b>🚀 Iniciando Automatización Total...</b></color>");

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("❌ No se encontró un objeto llamado 'Canvas'. Por favor, crea uno primero.");
            return;
        }

        Canvas canvas = canvasObj.GetComponent<Canvas>();

        // 1. Crear BlackScreen (Fundidos)
        GameObject blackScreenObj = GameObject.Find("BlackScreen");
        if (blackScreenObj == null)
        {
            blackScreenObj = new GameObject("BlackScreen");
            blackScreenObj.transform.SetParent(canvas.transform, false);
            Image img = blackScreenObj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); 
            img.raycastTarget = false;
            StretchUI(blackScreenObj.GetComponent<RectTransform>());
            Debug.Log("✔ <b>BlackScreen:</b> Creado y configurado como transparente.");
        }

        // 2. Crear DamageFlash (Pantalla Roja)
        GameObject damageFlashObj = GameObject.Find("DamageFlash");
        if (damageFlashObj == null)
        {
            damageFlashObj = new GameObject("DamageFlash");
            damageFlashObj.transform.SetParent(canvas.transform, false);
            Image img = damageFlashObj.AddComponent<Image>();
            img.color = new Color(1, 0, 0, 0); 
            img.raycastTarget = false;
            StretchUI(damageFlashObj.GetComponent<RectTransform>());
            Debug.Log("✔ <b>DamageFlash:</b> Creado y configurado como transparente.");
        }

        // 3. Configurar GameManager y Scripts
        GameObject gManagerObj = GameObject.Find("GameManager");
        if (gManagerObj == null)
        {
            gManagerObj = new GameObject("GameManager");
            Debug.Log("✔ <b>GameManager:</b> Creado objeto nuevo.");
        }

        GameManager gm = gManagerObj.GetComponent<GameManager>() ?? gManagerObj.AddComponent<GameManager>();
        UIManager ui = gManagerObj.GetComponent<UIManager>() ?? gManagerObj.AddComponent<UIManager>();
        HealthManager hm = gManagerObj.GetComponent<HealthManager>() ?? gManagerObj.AddComponent<HealthManager>();

        // 4. Vincular Refs en UIManager
        ui.blackScreen = blackScreenObj.GetComponent<Image>();
        ui.damageFlashImage = damageFlashObj.GetComponent<Image>();
        if (ui.mobileControls == null) ui.mobileControls = GameObject.Find("MobileControls") ?? GameObject.Find("Mobile Controls");
        Debug.Log("✔ <b>UIManager:</b> Referencias de pantalla vinculadas.");

        // 5. Vincular Referencias en HealthManager
        GameObject heartUI0 = GameObject.Find("Heart");
        GameObject heartUI1 = GameObject.Find("Heart (1)");
        GameObject heartUI2 = GameObject.Find("Heart (2)");

        if (heartUI0 != null)
        {
            hm.hearts = new Image[3];
            hm.hearts[0] = heartUI0.GetComponent<Image>();
            if (heartUI1 != null) hm.hearts[1] = heartUI1.GetComponent<Image>();
            if (heartUI2 != null) hm.hearts[2] = heartUI2.GetComponent<Image>();
            Debug.Log("✔ <b>HealthManager:</b> Objetos de Corazones de la UI vinculados.");
        }

        // 6. BUSCAR Y ASIGNAR SPRITES DE CORAZÓN AUTOMÁTICAMENTE
        string[] heartSprites = AssetDatabase.FindAssets("heart t:Sprite");
        foreach (string guid in heartSprites)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            
            if (path.Contains("heart.png")) hm.FullHeartSprite = s;
            else if (path.Contains("heart (1).png")) hm.EmptyHeartSprite = s;
            // Si no hay half heart, usamos el full como fallback
            if (hm.HalfHeartSprite == null) hm.HalfHeartSprite = s;
        }
        Debug.Log("✔ <b>Sprites:</b> Se han buscado y asignado los dibujos de los corazones.");

        // 7. Vincular Panel de Muerte
        if (gm.deathMenuPanel == null)
        {
            gm.deathMenuPanel = GameObject.Find("DeathMenuPanel") ?? GameObject.Find("LevelExitPanel") ?? GameObject.Find("Death Panel");
        }
        if (gm.deathMenuPanel != null) Debug.Log("✔ <b>Death Menu:</b> Panel '" + gm.deathMenuPanel.name + "' vinculado.");

        // 8. Buscar Efecto de Daño (Opcional)
        if (hm.damageEffect == null)
        {
            string[] fx = AssetDatabase.FindAssets("PickupEffect1 t:Prefab");
            if (fx.Length > 0)
            {
                hm.damageEffect = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(fx[0]));
                Debug.Log("✔ <b>FX:</b> Efecto visual de daño asignado.");
            }
        }

        EditorUtility.SetDirty(gManagerObj);
        EditorUtility.SetDirty(canvasObj);
        
        Debug.Log("<color=cyan><b>✅ ¡TODO LISTO!</b> El juego ya tiene vidas, efectos de pantalla y menús configurados.</color>");
        
        EditorUtility.DisplayDialog("Automatización Completa", 
            "He hecho lo siguiente:\n\n" +
            "1. Creado BlackScreen y DamageFlash (para fundidos y sangre).\n" +
            "2. Configurado el GameManager con todos sus scripts.\n" +
            "3. Conectado los 3 corazones de tu UI.\n" +
            "4. Buscado y puesto los dibujos de los corazones automáticamente.\n" +
            "5. Conectado el Panel de Muerte.\n\n" +
            "¡Ya puedes darle al Play!", "¡A jugar!");
    }

    private static void StretchUI(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
