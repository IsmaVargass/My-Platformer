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

        // 10. Menú de Pausa (FORZAR RE-CREACIÓN PARA ASEGURAR CONTENIDO)
        GameObject controlsPanel = FindObjectIncludingInactive(canvasObj, "ControlsPanel");
        if (controlsPanel == null)
        {
            CreateControlsPanel();
            controlsPanel = FindObjectIncludingInactive(canvasObj, "ControlsPanel");
        }

        GameObject existingPause = FindObjectIncludingInactive(canvasObj, "PauseMenu");
        if (existingPause != null) Object.DestroyImmediate(existingPause);
        
        GameObject pauseMenu = CreatePauseMenuUI(canvasObj);
        ui.pauseMenuPanel = pauseMenu;
        ui.controlsPanel = controlsPanel; 
        pauseMenu.transform.SetAsLastSibling();
        pauseMenu.transform.localScale = Vector3.one;
        pauseMenu.SetActive(false);

        // 11. Menú de Game Over
        GameObject existingGameOver = FindObjectIncludingInactive(canvasObj, "GameOverPanel");
        if (existingGameOver != null) Object.DestroyImmediate(existingGameOver);
        
        GameObject gameOverPanel = CreateGameOverUI(canvasObj);
        gm.deathMenuPanel = gameOverPanel;
        gameOverPanel.SetActive(false);

        // 12. Menú de Nivel Completado
        GameObject existingVictory = FindObjectIncludingInactive(canvasObj, "LevelCompletePanel");
        if (existingVictory != null) Object.DestroyImmediate(existingVictory);
        
        GameObject victoryPanel = CreateLevelCompleteUI(canvasObj);
        gm.levelCompletePanel = victoryPanel;
        victoryPanel.SetActive(false);

        // 15. Pantalla de Instrucciones / Tutorial (NUEVO)
        GameObject existingTutorial = FindObjectIncludingInactive(canvasObj, "TutorialPanel");
        if (existingTutorial != null) Object.DestroyImmediate(existingTutorial);

        GameObject tutorialPanel = CreateTutorialUI(canvasObj);
        gm.tutorialPanel = tutorialPanel;
        tutorialPanel.SetActive(false);

        // 13. HUD Timer
        GameObject timerHUD = FindObjectIncludingInactive(canvasObj, "HUD_Timer");
        if (timerHUD == null)
        {
            timerHUD = new GameObject("HUD_Timer", typeof(RectTransform));
            timerHUD.transform.SetParent(canvasObj.transform, false);
            RectTransform rt = timerHUD.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-20, -20);
            rt.sizeDelta = new Vector2(200, 50);

            TextMeshProUGUI t = timerHUD.AddComponent<TextMeshProUGUI>();
            t.text = "00:00.00";
            t.fontSize = 28;
            t.alignment = TextAlignmentOptions.Right;
            t.color = Color.yellow;
            t.fontStyle = FontStyles.Bold;
            
            // Sombra
            timerHUD.AddComponent<Outline>().effectColor = Color.black;
        }
        gm.timerHUDText = timerHUD.GetComponent<TextMeshProUGUI>();

        // Vincular los textos del panel de victoria
        gm.leveCompletePanelTitle = victoryPanel.transform.Find("MainPanel/Title")?.GetComponent<TMP_Text>();
        gm.levelCompleteCoins = victoryPanel.transform.Find("MainPanel/Coins")?.GetComponent<TMP_Text>();
        gm.currentLevelTimeText = victoryPanel.transform.Find("MainPanel/CurrentTime")?.GetComponent<TMP_Text>();
        gm.bestLevelTimeText = victoryPanel.transform.Find("MainPanel/BestTime")?.GetComponent<TMP_Text>();
        
        // Búsqueda ROBUSTA del texto de monedas
        if (gm.coinText == null)
        {
            gm.coinText = canvasObj.GetComponentsInChildren<TextMeshProUGUI>(true)
                .FirstOrDefault(t => t.name.ToLower().Contains("coin") || t.name.ToLower().Contains("count") || t.name.ToLower().Contains("moneda"));
        }

        // 14. Aviso de Monedas (NUEVO)
        GameObject coinWarn = FindObjectIncludingInactive(canvasObj, "CoinWarning");
        if (coinWarn == null)
        {
            coinWarn = CreateCoinWarningUI(canvasObj);
        }
        ui.coinWarning = coinWarn;
        coinWarn.SetActive(false);

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
        
        // --- CHEQUEO DE EVENT SYSTEM (VITAL!) ---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            Debug.Log("✔ EventSystem: No se encontró. Creando uno nuevo...");
            GameObject eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        Debug.Log("<color=cyan><b>✅ ¡RESCATE TOTAL COMPLETADO!</b></color>");
    }

    private static GameObject CreatePauseMenuUI(GameObject canvas)
    {
        GameObject menu = new GameObject("PauseMenu", typeof(RectTransform));
        menu.transform.SetParent(canvas.transform, false);
        menu.transform.SetAsLastSibling();
        
        RectTransform rt = menu.GetComponent<RectTransform>();
        StretchUI(rt);
        rt.localScale = Vector3.one;
        rt.localPosition = Vector3.zero;

        // Blindaje Nuclear: Canvas propio para estar por encima de TODO
        Canvas menuCanvas = menu.AddComponent<Canvas>();
        menuCanvas.overrideSorting = true;
        menuCanvas.sortingOrder = 20000; // Por debajo de los controles (30k) pero encima del HUD
        menu.AddComponent<GraphicRaycaster>();

        PauseMenu pmScript = menu.AddComponent<PauseMenu>();

        // Fondo oscuro PREMIUM con bloqueo de clicks
        GameObject bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(menu.transform, false);
        StretchUI(bg.GetComponent<RectTransform>());
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.12f, 0.95f); 
        bgImg.raycastTarget = true; 

        // Panel Central (Contenedor)
        GameObject panel = new GameObject("MainPanel", typeof(RectTransform));
        panel.transform.SetParent(menu.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 500);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // Título con sombra (Efecto premium)
        GameObject title = new GameObject("Title", typeof(RectTransform));
        title.transform.SetParent(panel.transform, false);
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "PAUSA";
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 54;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.9f, 0.9f, 1f);
        title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 180);

        // Botón Reanudar (Más grande y llamativo)
        pmScript.resumeButton = CreateButton(panel, "Btn_Reanudar", "REANUDAR JUEGO", new Vector2(0, 40), new Color(0.2f, 0.6f, 0.2f));

        // Botón Controles
        pmScript.controlsButton = CreateButton(panel, "Btn_Controles", "CONTROLES", new Vector2(0, -60), new Color(0.3f, 0.3f, 0.4f));

        // Botón Salir (SALIR DEL JUEGO directamente)
        pmScript.quitButton = CreateButton(panel, "Btn_Salir", "SALIR DEL JUEGO", new Vector2(0, -160), new Color(0.6f, 0.2f, 0.2f));

        // Vincular panel de controles
        GameObject controls = FindObjectIncludingInactive(canvas, "ControlsPanel");
        if (controls != null) pmScript.controlsPanel = controls;

        return menu;
    }

    private static Button CreateButton(GameObject parent, string name, string label, Vector2 pos, Color btnColor)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(parent.transform, false);
        
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 70);
        rect.anchoredPosition = pos;

        Image img = btnObj.AddComponent<Image>();
        img.color = btnColor;
        
        // Efecto visual: borde sutil
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.2f);
        outline.effectDistance = new Vector2(2, -2);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = btnColor * 1.2f;
        colors.pressedColor = btnColor * 0.8f;
        btn.colors = colors;

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        StretchUI(textObj.GetComponent<RectTransform>());
        
        TextMeshProUGUI t = textObj.AddComponent<TextMeshProUGUI>();
        t.text = label;
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = 22;
        t.fontStyle = FontStyles.Bold;
        t.color = Color.white;

        return btn;
    }

    [MenuItem("Tools/Frosty Fortune/Fix Main Menu Buttons")]
    public static void FixMainMenu()
    {
        MainMenu mm = Object.FindFirstObjectByType<MainMenu>();
        if (mm == null)
        {
            Debug.LogError("❌ No se encontró el script 'MainMenu' en esta escena. Asegúrate de estar en la escena de Menú.");
            return;
        }

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // --- CHEQUEO DE EVENT SYSTEM (VITAL!) ---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            Debug.Log("✔ <b>EventSystem:</b> No se encontró en el Menú. Creando uno...");
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        // Intentar encontrar el panel de controles
        GameObject controls = FindObjectIncludingInactive(canvas, "ControlsPanel");
        if (controls == null)
        {
            // Si no existe, lo creamos
            CreateControlsPanel();
            controls = FindObjectIncludingInactive(canvas, "ControlsPanel");
        }
        
        if (controls != null) controls.transform.SetAsLastSibling(); // Asegurar que bloquea clicks

        // Asignar el panel al script
        if (controls != null) mm.controlsPanel = controls;
        
        Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);
        foreach (var btn in allButtons)
        {
            // Aplicar estilo PREMIUM a los botones del menú principal para que coincidan con la pausa
            string n = btn.name.ToLower();
            
            // 1. Estilo Visual (Fondo y Borde)
            Image img = btn.GetComponent<Image>();
            if (img == null) img = btn.gameObject.AddComponent<Image>();
            
            Color baseColor = new Color(0.12f, 0.12f, 0.2f, 1f); // Por defecto azul oscuro premium
            if (n.Contains("salir") || n.Contains("quit")) baseColor = new Color(0.6f, 0.2f, 0.2f);
            else if (n.Contains("play") || n.Contains("jugar")) baseColor = new Color(0.2f, 0.5f, 0.2f);
            
            img.color = baseColor;
            
            Outline ol = btn.GetComponent<Outline>();
            if (ol == null) ol = btn.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(1, 1, 1, 0.2f);
            ol.effectDistance = new Vector2(2, -2);
            
            // 2. Texto
            TextMeshProUGUI t = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (t != null)
            {
                t.color = Color.white;
                t.fontStyle = FontStyles.Bold;
                t.fontSize = 24;
                if (n.Contains("play") || n.Contains("jugar")) t.text = "JUGAR";
                else if (n.Contains("salir") || n.Contains("quit")) t.text = "SALIR";
                else if (n.Contains("control")) t.text = "CONTROLES";
            }

            // 3. Listeners
            btn.onClick.RemoveAllListeners();
            if (n.Contains("play") || n.Contains("jugar")) btn.onClick.AddListener(mm.PlayGame);
            else if (n.Contains("control")) btn.onClick.AddListener(mm.OpenControls);
            else if (n.Contains("quit") || n.Contains("salir")) btn.onClick.AddListener(mm.QuitGame);
        }

        Debug.Log("✔ <b>Menú Principal Premium:</b> Botones restilizados y vinculados automáticamente.");
        EditorUtility.SetDirty(mm);
    }

    [MenuItem("Tools/Frosty Fortune/Create Controls UI")]
    public static void CreateControlsPanel()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) return;

        GameObject root = FindObjectIncludingInactive(canvasObj, "ControlsPanel");
        if (root != null) Object.DestroyImmediate(root); 

        // 1. Contenedor Raíz (Bloqueador de clicks total)
        root = new GameObject("ControlsPanel", typeof(RectTransform));
        root.transform.SetParent(canvasObj.transform, false);
        root.transform.SetAsLastSibling(); 
        StretchUI(root.GetComponent<RectTransform>());
        
        // Bloqueo Nuclear: Añadir Canvas propio para estar por encima de TODO
        Canvas rootCanvas = root.AddComponent<Canvas>();
        rootCanvas.overrideSorting = true;
        rootCanvas.sortingOrder = 30000; // Valor altísimo para ganar a cualquier botón
        root.AddComponent<GraphicRaycaster>(); // Receptor de clicks propio

        Image blockerImg = root.AddComponent<Image>();
        blockerImg.color = new Color(0, 0, 0, 0.85f); // Más oscuro para que los botones de atrás no distraigan
        blockerImg.raycastTarget = true; 

        // 2. Caja Visual Central (Diseño Premium)
        GameObject box = new GameObject("VisualBox", typeof(RectTransform));
        box.transform.SetParent(root.transform, false);
        RectTransform boxRect = box.GetComponent<RectTransform>();
        boxRect.sizeDelta = new Vector2(700, 500);
        boxRect.anchoredPosition = Vector2.zero;
        Image boxImg = box.AddComponent<Image>();
        boxImg.color = new Color(0.12f, 0.12f, 0.2f, 1f);
        boxImg.raycastTarget = true;

        Outline outline = box.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.2f);
        outline.effectDistance = new Vector2(2, -2);

        // 3. Texto de Controles
        GameObject textObj = new GameObject("ControlsText", typeof(RectTransform));
        textObj.transform.SetParent(box.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "<size=40><b>CONTROLES</b></size>\n\n" +
                    "<color=yellow>MOVIMIENTO:</color> WASD / FLECHAS\n" +
                    "<color=yellow>SALTAR:</color> ESPACIO / W / ARRIBA\n" +
                    "<color=yellow>PAUSA (ESC/P):</color> MENU\n\n" +
                    "<size=20><color=#AAAAAA>Usa WASD para moverte por el mundo\ny Space para saltar obstáculos.</color></size>";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 32;
        text.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(650, 400);
        textRect.anchoredPosition = new Vector2(0, 50);

        // 4. Botón CERRAR (Único botón interactivo permitido)
        Button backBtn = CreateButton(box, "Btn_Cerrar", "CERRAR CONTROLES", new Vector2(0, -180), new Color(0.7f, 0.3f, 0.3f));
        
        // Listener persistente (Sin lambda para evitar errores de serialización en el Editor)
        UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(backBtn.onClick, root.SetActive, false);
        
        Debug.Log("✔ <b>Panel de Controles Blindado:</b> Bloqueo total activado con botón Volver.");
        root.SetActive(false); 
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

    private static GameObject CreateGameOverUI(GameObject canvas)
    {
        GameObject root = new GameObject("GameOverPanel", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        root.transform.SetAsLastSibling();
        StretchUI(root.GetComponent<RectTransform>());

        // Canvas propio para estar por encima del HUD (pero debajo de los controles si hiciera falta)
        Canvas c = root.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 15000; 
        root.AddComponent<GraphicRaycaster>();

        // Fondo Rojo Oscuro de Muerte
        GameObject bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(root.transform, false);
        StretchUI(bg.GetComponent<RectTransform>());
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0, 0, 0.9f);
        bgImg.raycastTarget = true;

        // Panel Central
        GameObject panel = new GameObject("MainPanel", typeof(RectTransform));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(500, 400);
        Image pImg = panel.AddComponent<Image>();
        pImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        panel.AddComponent<Outline>().effectColor = Color.red;

        // Título GAME OVER (Revertido por petición)
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "<color=red><size=60><b>GAME OVER</b></size></color>"; 
        title.alignment = TextAlignmentOptions.Center;
        title.rectTransform.anchoredPosition = new Vector2(0, 100);

        GameManager gm = Object.FindFirstObjectByType<GameManager>();

        // Botón Reiniciar
        Button restartBtn = CreateButton(panel, "Btn_Restart", "REINTENTAR", new Vector2(0, -20), new Color(0.3f, 0.6f, 0.3f));
        if (gm != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(restartBtn.onClick, gm.RestartLevel);

        // Botón Salir
        Button quitBtn = CreateButton(panel, "Btn_Quit", "SALIR", new Vector2(0, -120), new Color(0.6f, 0.2f, 0.2f));
        if (gm != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(quitBtn.onClick, gm.QuitGame);

        return root;
    }

    private static GameObject CreateLevelCompleteUI(GameObject canvas)
    {
        GameObject root = new GameObject("LevelCompletePanel", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        root.transform.SetAsLastSibling();
        StretchUI(root.GetComponent<RectTransform>());

        // Canvas propio (Prioridad alta)
        Canvas c = root.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 25000; 
        root.AddComponent<GraphicRaycaster>();

        // Fondo Oscuro Elegante (Más opaco)
        GameObject bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(root.transform, false);
        StretchUI(bg.GetComponent<RectTransform>());
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.02f, 0.02f, 0.05f, 0.98f);
        bgImg.raycastTarget = true;

        // Panel Central (Aún más grande)
        GameObject panel = new GameObject("MainPanel", typeof(RectTransform));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 700);
        Image pImg = panel.AddComponent<Image>();
        pImg.color = new Color(0.12f, 0.12f, 0.18f, 1f);
        panel.AddComponent<Outline>().effectColor = new Color(0f, 0.8f, 1f, 0.6f);

        // Título ENHORABUENA / RÉCORD
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "<color=#00FFCC>¡ENHORABUENA!</color>";
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 64; // Título más grande
        title.fontStyle = FontStyles.Bold;
        title.rectTransform.anchoredPosition = new Vector2(0, 240); // Bajado un poco
        title.rectTransform.sizeDelta = new Vector2(750, 100);

        // Subtítulo personalizado
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform));
        subObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI subtitle = subObj.AddComponent<TextMeshProUGUI>();
        subtitle.text = "Has terminado el nivel, ¡buen trabajo!";
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.fontSize = 24;
        subtitle.color = new Color(0.8f, 0.8f, 0.8f);
        subtitle.rectTransform.anchoredPosition = new Vector2(0, 160);
        subtitle.rectTransform.sizeDelta = new Vector2(750, 40);

        // --- ESTADÍSTICAS (Con más separación) ---
        
        // Monedas
        GameObject coinsObj = new GameObject("Coins", typeof(RectTransform));
        coinsObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI coinsText = coinsObj.AddComponent<TextMeshProUGUI>();
        coinsText.text = "MONEDAS: 0 / 0";
        coinsText.fontSize = 36;
        coinsText.alignment = TextAlignmentOptions.Center;
        coinsText.rectTransform.anchoredPosition = new Vector2(0, 60);
        coinsText.rectTransform.sizeDelta = new Vector2(750, 60);

        // Tiempo Actual
        GameObject currentObj = new GameObject("CurrentTime", typeof(RectTransform));
        currentObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI currentTimeText = currentObj.AddComponent<TextMeshProUGUI>();
        currentTimeText.text = "TIEMPO: 00:00.00";
        currentTimeText.fontSize = 36;
        currentTimeText.alignment = TextAlignmentOptions.Center;
        currentTimeText.rectTransform.anchoredPosition = new Vector2(0, -30); // Bajado para separar
        currentTimeText.rectTransform.sizeDelta = new Vector2(750, 60);

        // Mejor Tiempo (Récord)
        GameObject bestObj = new GameObject("BestTime", typeof(RectTransform));
        bestObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI bestTimeText = bestObj.AddComponent<TextMeshProUGUI>();
        bestTimeText.text = "RÉCORD: 00:00.00";
        bestTimeText.fontSize = 30; // Un poco más pequeño que el actual para jerarquía
        bestTimeText.color = new Color(1, 0.85f, 0); // Amarillo dorado vibrante
        bestTimeText.alignment = TextAlignmentOptions.Center;
        bestTimeText.rectTransform.anchoredPosition = new Vector2(0, -110); // Más separación
        bestTimeText.rectTransform.sizeDelta = new Vector2(750, 60);

        GameManager gm = Object.FindFirstObjectByType<GameManager>();

        // Botón Volver al Menú
        Button menuBtn = CreateButton(panel, "Btn_Menu", "VOLVER AL MENÚ", new Vector2(0, -220), new Color(0.15f, 0.4f, 0.8f));
        if (gm != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.onClick, gm.GoToMainMenu);

        // Botón Reintentar
        Button retryBtn = CreateButton(panel, "Btn_Retry", "REPETIR NIVEL", new Vector2(0, -310), new Color(0.2f, 0.6f, 0.2f));
        if (gm != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(retryBtn.onClick, gm.RestartLevel);

        return root;
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

    public static GameObject CreateTutorialUI(GameObject canvas)
    {
        GameObject root = new GameObject("TutorialPanel", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        root.transform.SetAsLastSibling();
        StretchUI(root.GetComponent<RectTransform>());

        // Canvas propio (Prioridad absoluta)
        Canvas c = root.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 35000; 
        root.AddComponent<GraphicRaycaster>();

        // Fondo Oscuro
        GameObject bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(root.transform, false);
        StretchUI(bg.GetComponent<RectTransform>());
        bg.AddComponent<Image>().color = new Color(0, 0, 0, 0.95f);

        // Panel Central
        GameObject panel = new GameObject("MainPanel", typeof(RectTransform));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(850, 650);
        panel.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.2f, 1f);
        panel.AddComponent<Outline>().effectColor = new Color(0.2f, 0.8f, 1f, 0.5f);

        // Título
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "OBJETIVO DEL NIVEL";
        title.color = new Color(0f, 1f, 0.8f); // Cyan-ish color without using tags
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 54;
        title.rectTransform.anchoredPosition = new Vector2(0, 240);
        title.rectTransform.sizeDelta = new Vector2(800, 80);

        // Contenido (Instrucciones)
        GameObject descObj = new GameObject("Instructions", typeof(RectTransform));
        descObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI desc = descObj.AddComponent<TextMeshProUGUI>();
        desc.text = "- MONEDAS: Debes recoger TODAS para abrir la salida.\n\n" +
                    "- VIDAS: Tienes 3 corazones. No los pierdas.\n\n" +
                    "- CONTROLES: WASD para moverte y ESPACIO para saltar.\n" +
                    "(Doble salto disponible en el aire)";
        desc.alignment = TextAlignmentOptions.Center;
        desc.fontSize = 30;
        desc.lineSpacing = 20;
        desc.rectTransform.anchoredPosition = new Vector2(0, 40);
        desc.rectTransform.sizeDelta = new Vector2(750, 300);

        GameManager gm = Object.FindFirstObjectByType<GameManager>();

        // Botón Empezar
        Button startBtn = CreateButton(panel, "Btn_Start", "¡EMPEZAR PARTIDA!", new Vector2(0, -220), new Color(0.2f, 0.7f, 0.2f));
        startBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 80);
        if (gm != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(startBtn.onClick, gm.StartGame);

        return root;
    }

    public static GameObject CreateCoinWarningUI(GameObject canvas)
    {
        GameObject root = new GameObject("CoinWarning", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 60);
        rt.anchoredPosition = new Vector2(0, -100); // Debajo de los corazones/contador original
        
        Image img = root.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.8f);
        
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(root.transform, false);
        StretchUI(textObj.GetComponent<RectTransform>());
        
        TextMeshProUGUI t = textObj.AddComponent<TextMeshProUGUI>();
        t.text = "<color=red>⚠</color> ¡Te faltan monedas para terminar el nivel!";
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = 20;
        t.color = Color.white;
        t.fontStyle = FontStyles.Bold;
        
        root.AddComponent<Outline>().effectColor = Color.red;
        
        return root;
    }
}
