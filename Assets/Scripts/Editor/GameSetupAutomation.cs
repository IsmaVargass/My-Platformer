using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
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
            Debug.Log("✔ <b>BlackScreen:</b> Creado y configurado.");
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
            Debug.Log("✔ <b>DamageFlash:</b> Creado y configurado.");
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

        // 8. Vincular Player a Managers (FORZADO)
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        ui.playerController = pc;
        gm.playerController = pc;

        // 4. Vincular Refs en UIManager
        ui.blackScreen = blackScreenObj.GetComponent<Image>();
        ui.damageFlashImage = damageFlashObj.GetComponent<Image>();
        if (ui.mobileControls == null) ui.mobileControls = FindObjectIncludingInactive(canvasObj, "MobileControls") ?? FindObjectIncludingInactive(canvasObj, "Mobile Controls");

        // 5. Vincular Corazones por POSICIÓN (Solo si son parte del CANVAS para no pillar pinchos)
        Image[] heartsInScene = canvasObj.GetComponentsInChildren<Image>(true)
            .Where(img => img.name.ToLower().Contains("heart") || img.name.ToLower().Contains("corazon"))
            .OrderBy(img => img.transform.position.x)
            .ToArray();

        if (heartsInScene.Length > 0)
        {
            hm.hearts = heartsInScene;
            for(int i=0; i < heartsInScene.Length; i++)
            {
                string suffix = (i==0) ? "IZQUIERDA" : (i==1) ? "CENTRO" : "DERECHA";
                heartsInScene[i].gameObject.name = "UI_Corazon_" + suffix;
            }
            Debug.Log($"✔ <b>HealthManager:</b> {heartsInScene.Length} corazones UI vinculados.");
        }

        // 6. Buscar Sprites de Corazón
        string[] heartSprites = AssetDatabase.FindAssets("heart t:Sprite");
        foreach (string guid in heartSprites)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid).ToLower();
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (path.Contains("heart.png") || path.Contains("full")) hm.FullHeartSprite = s;
            else if (path.Contains("half")) hm.HalfHeartSprite = s;
            else if (path.Contains("empty") || path.Contains("(1)")) hm.EmptyHeartSprite = s;
        }

        // 9. Sonido de daño
        if (pc != null && pc.hurtSound == null)
        {
            string[] audioClips = AssetDatabase.FindAssets("hurt t:AudioClip");
            if (audioClips.Length > 0)
            {
                AudioSource source = pc.gameObject.GetComponent<AudioSource>() ?? pc.gameObject.AddComponent<AudioSource>();
                source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(audioClips[0]));
                source.playOnAwake = false;
                pc.hurtSound = source;
            }
        }

        // 10. Menú de Pausa (FORZAR VÍNCULO)
        GameObject pauseMenu = FindObjectIncludingInactive(canvasObj, "PauseMenu");
        if (pauseMenu == null)
        {
            pauseMenu = CreatePauseMenuUI(canvasObj);
        }
        ui.pauseMenuPanel = pauseMenu;
        pauseMenu.SetActive(false);

        // --- ARREGLO VISUAL DE PINCHOS ---
        SpriteRenderer[] allRenderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        int spikesFound = 0;
        foreach(var sr in allRenderers)
        {
            string n = sr.gameObject.name.ToLower();
            if (n.Contains("spike") || n.Contains("pincho") || n.Contains("trampa") || sr.gameObject.GetComponent<Spikes>() != null)
            {
                sr.enabled = true;
                sr.color = new Color(1, 1, 1, 1); // Opacidad máxima
                sr.sortingOrder = 100; // Por delante de TODO
                
                if (sr.gameObject.layer == LayerMask.NameToLayer("UI")) 
                    sr.gameObject.layer = LayerMask.NameToLayer("Default");

                spikesFound++;
                Debug.Log($"✔ <b>Spike Fixed:</b> {sr.gameObject.name} (Order: 100, Opacidad: 100%)");
            }
        }
        if (spikesFound == 0) Debug.LogWarning("⚠ No se encontraron objetos con 'Spike' en el nombre o con el script Spikes.");

        // --- CHEQUEO DE CÁMARA (Para error de RenderTexture) ---
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            if (!cam.CompareTag("MainCamera")) cam.tag = "MainCamera";
            // Aseguramos que el targetTexture sea NULL (vuelve a la pantalla)
            if (cam.targetTexture != null)
            {
                Debug.LogWarning("⚠ La cámara tenía un RenderTexture asignado que podía causar errores. Limpiando...");
                cam.targetTexture = null;
            }
        }

        EditorUtility.SetDirty(gManagerObj);
        if (pc != null) EditorUtility.SetDirty(pc.gameObject);
        EditorUtility.SetDirty(canvasObj);
        if (cam != null) EditorUtility.SetDirty(cam.gameObject);
        
        Debug.Log("<color=cyan><b>✅ ¡RESCATE TOTAL COMPLETADO!</b></color>");
    }

    private static GameObject CreatePauseMenuUI(GameObject canvas)
    {
        // Usamos una forma más segura de crear objetos UI
        GameObject menu = new GameObject("PauseMenu", typeof(RectTransform));
        menu.transform.SetParent(canvas.transform, false);
        StretchUI(menu.GetComponent<RectTransform>());

        PauseMenu pmScript = menu.AddComponent<PauseMenu>();

        // Fondo oscuro
        GameObject bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(menu.transform, false);
        StretchUI(bg.GetComponent<RectTransform>());
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.8f);

        // Título
        GameObject title = new GameObject("Title", typeof(RectTransform));
        title.transform.SetParent(menu.transform, false);
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "PAUSA";
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 50;
        title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 150);

        // Botón Reanudar
        pmScript.resumeButton = CreateButton(menu, "Btn_Reanudar", "REANUDAR", new Vector2(0, 40));

        // Botón Controles
        pmScript.controlsButton = CreateButton(menu, "Btn_Controles", "CONTROLES", new Vector2(0, -40));

        // Botón Salir
        pmScript.quitButton = CreateButton(menu, "Btn_Salir", "SALIR AL MENÚ", new Vector2(0, -120));

        // Vincular panel de controles (buscando incluso si está desactivado)
        GameObject controls = FindObjectIncludingInactive(canvas, "ControlsPanel");
        if (controls != null) pmScript.controlsPanel = controls;

        return menu;
    }

    private static Button CreateButton(GameObject parent, string name, string label, Vector2 pos)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(parent.transform, false);
        
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250, 60);
        rect.anchoredPosition = pos;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button btn = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        StretchUI(textObj.GetComponent<RectTransform>());
        
        TextMeshProUGUI t = textObj.AddComponent<TextMeshProUGUI>();
        t.text = label;
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = 24;
        t.color = Color.white;

        return btn;
    }

    [MenuItem("Tools/Frosty Fortune/Create Controls UI")]
    public static void CreateControlsPanel()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) return;

        GameObject panel = FindObjectIncludingInactive(canvasObj, "ControlsPanel");
        if (panel == null)
        {
            panel = new GameObject("ControlsPanel", typeof(RectTransform));
            panel.transform.SetParent(canvasObj.transform, false);
            
            StretchUI(panel.GetComponent<RectTransform>());
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.9f);
            
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 400);
            rect.anchoredPosition = Vector2.zero;

            GameObject textObj = new GameObject("ControlsText", typeof(RectTransform));
            textObj.transform.SetParent(panel.transform, false);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "<b>CONTROLES DE JUEGO</b>\n\n" +
                        "<color=yellow>MOVIMIENTO:</color> WASD / FLECHAS\n" +
                        "<color=yellow>SALTAR:</color> ESPACIO / W / ARRIBA\n" +
                        "<color=yellow>PAUSA:</color> ESC\n\n" +
                        "<size=20>Pulsa cualquier botón para cerrar</size>";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 30;
            text.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(550, 350);

            // Añadir botón invisible para cerrar
            Button closeBtn = panel.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => panel.SetActive(false));
            
            Debug.Log("✔ <b>Panel de Controles:</b> Creado y configurado.");
        }
        else
        {
            panel.SetActive(true);
            Debug.Log("✔ <b>Panel de Controles:</b> Ya existía, se ha activado.");
        }
    }

    [MenuItem("Tools/Frosty Fortune/Check Scene Health")]
    public static void CheckSceneHealth()
    {
        Debug.Log("<color=yellow><b>🔍 Iniciando Chequeo de Salud de la Escena...</b></color>");
        
        bool allGood = true;

        // Jugador
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
             player = Object.FindFirstObjectByType<PlayerController>()?.gameObject;
             if (player != null && !player.CompareTag("Player"))
             {
                 Debug.LogWarning("⚠ El Jugador NO tiene el Tag 'Player'. Corrigiendo...");
                 player.tag = "Player";
             }
             else if (player == null)
             {
                 Debug.LogError("❌ No se encontró al Jugador en la escena.");
                 allGood = false;
             }
        }

        // Suelo
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc.groundCheck == null) Debug.LogWarning("⚠ Al Jugador le falta asignar el 'Ground Check'.");
        }

        // Managers
        if (Object.FindFirstObjectByType<GameManager>() == null) { Debug.LogError("❌ Falta GameManager."); allGood = false; }
        if (Object.FindFirstObjectByType<HealthManager>() == null) { Debug.LogError("❌ Falta HealthManager."); allGood = false; }

        if (allGood)
            EditorUtility.DisplayDialog("Salud de Escena", "¡Todo parece estar en orden!", "Perfecto");
        else
            EditorUtility.DisplayDialog("Salud de Escena", "Se han encontrado errores críticos. Revisa la Consola.", "Entendido");
    }

    private static void StretchUI(RectTransform rect)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static GameObject FindObjectIncludingInactive(GameObject parent, string name)
    {
        Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            if (t.name == name) return t.gameObject;
        }
        return null;
    }
}
