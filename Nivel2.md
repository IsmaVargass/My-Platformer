# 🚩 GUÍA: CÓMO CREAR EL NIVEL 2 (Perfecto)

Sigue estos pasos para que tu segundo nivel funcione a la primera:

### 1. Duplicar la Escena Actual
Es mejor duplicar el Nivel 1 para no tener que configurar la cámara y el Personaje desde cero:
- En la carpeta de escenas dentro de Unity, haz clic en tu nivel actual y pulsa `Ctrl + D` (Duplicar).
- Cámbiale el nombre a `Nivel 2`.

### 2. Limpiar y Diseñar
- Abre la nueva escena `Nivel 2` haciendo doble clic.
- Borra los enemigos o plataformas viejas y dibuja tu nuevo nivel.
- **IMPORTANTE**: Asegúrate de que el objeto **Player** está en la posición donde quieres que empiece este nivel.

### 3. Configurar el "Build Settings" (Crucial)
Para que el juego sepa que existe el Nivel 2 y pueda saltar a él:
- Ve a `File > Build Settings`.
- Si ves escenas viejas que no usas, selecciónalas y pulsa la tecla `Delete`.
- Arrastra tu **Nivel 1** a la lista (debe ser el número 0 o 1).
- Arrastra tu **Nivel 2** justo debajo (debe tener el número siguiente).

### 4. Activar el Paso de Nivel
En tu **Nivel 1**:
- Busca el objeto del final (el cuadrado verde).
- Asegúrate de que tiene el script **Exit Trigger** asignado.
- Revisa que el **Box Collider 2D** tenga marcada la casilla **"Is Trigger"**.
- Al tocarlo en el juego, verás el panel de "Nivel Completado" y al continuar cargarás el Nivel 2.

---
¡Listo! Con esto ya tienes tu sistema de niveles funcionando.
