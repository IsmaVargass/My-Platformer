# Documento de Diseño: Nivel 2D (Frosty Fortune)

**Elaborado por:** Ismael Vargas Duque  
**Motor:** Unity 6 LTS  
**Duración Estimada:** 1 - 3 minutos

---

## Evidencias del Proyecto

### 📸 Capturas de Pantalla
*(Alumno: Sustituye el texto [AQUÍ TU ENLACE] por el enlace o nombre del archivo de tus imágenes)*

![Menú Principal]([AQUÍ TU ENLACE])

Menú de inicio del juego y selección de niveles.

![Gameplay]([AQUÍ TU ENLACE])

Nivel en progreso, mostrando HUD funcional (corazones, monedas, tiempo) y plataformas de Tilemap.

![Menú de Pausa]([AQUÍ TU ENLACE])

Menú de pausa durante el juego.

![Controles]([AQUÍ TU ENLACE])

Pantalla de controles y configuración.

![Muerte]([AQUÍ TU ENLACE])

Pantalla de fin de partida (Game Over).

![Pantalla de Victoria]([AQUÍ TU ENLACE])

Condición de fin alcanzada. Pantalla mostrando el resumen de la partida y el récord.

### 🎥 Vídeo de Gameplay Completo
*(Alumno: Inserta el enlace a tu vídeo aquí. Recuerda: mínimo 1 minuto, desde el inicio del nivel hasta completarlo)*

📺 **[AQUÍ ENLACE AL VÍDEO COMPLETO]**

---

## 1. Contexto y Descripción General
Este documento detalla el diseño y la implementación de un nivel 2D funcional, desarrollado como parte de la evaluación de flujo de trabajo en Unity. El objetivo principal de **Frosty Fortune** es demostrar la integración técnica y estética de sistemas fundamentales: mecánicas de plataformas, física 2D, Tilemaps, gestión de estado mediante C# y una UI pulida.

El nivel se concibe como una experiencia cerrada (standalone) donde el jugador debe explorar el entorno, recolectar todos los objetos clave y llegar a la zona de salida sin perder todas sus vidas.

---

## 2. Decisiones de Diseño y Jugabilidad (GDD)

### 2.1. Layout y Ritmo (Pacing)
El nivel está diseñado para tener una dificultad progresiva. Se introduce al jugador primero al movimiento base, seguido de saltos sencillos y, finalmente, plataformas más precisas que requieren del uso del "doble salto". 
*   **Decisión:** Se implementó una pantalla inicial de "Tutorial/Objetivo" que pausa el juego. Esto asegura que el jugador comprenda las reglas (recoger todas las monedas, cuidado con los pinchos) antes de iniciar el cronómetro, garantizando una experiencia de usuario clara.

### 2.2. Mecánicas Principales implementadas vía Scripting
*   **Movimiento (PlayerController):** Se optó por un controlador físico puro usando `Rigidbody2D` (Forces y Velocities) en lugar de cinemático para asegurar una interacción fluida con colisiones y gravedad natural.
*   **Doble Salto:** Añade profundidad a las plataformas. Requiere gestión de estados (conocer el número de saltos dados en el aire y reiniciar al tocar suelo).
*   **Condición de Victoria Obligatoria:** No basta con llegar al final; el `ExitTrigger` verifica constantemente con el `GameManager` si se ha recogido un 100% de las monedas. Esto fomenta la exploración completa del mapa.

### 2.3. Dificultad y Hazards (Obstáculos)
La dificultad es moderada, penalizando errores concretos:
*   **Pinchos (Daño Táctico):** No matan instantáneamente para no frustrar. Restan un corazón (1 vida) de los 3 disponibles y aplican un "Knockback" (fuerza de empuje) para alejar al jugador del peligro de forma realista.
*   **Vacío (Killzone):** Zonas de muerte instantánea. Obligan al jugador a dominar la mecánica del doble salto en áreas elevadas.

---

## 3. Implementación Técnica (Unity)

### 3.1. Escenario y Tilemaps
*   **Construcción Modular:** Se usaron Tilemaps para pintar el nivel de forma eficiente, separar capas visuales y optimizar el rendimiento.
*   **Colisiones Optimizadas:** Se empleó `TilemapCollider2D` junto con `CompositeCollider2D`. *Decisión técnica*: El Composite Collider unifica de forma eficiente todas las cajas de colisión individuales en una sola malla, puliendo el movimiento del personaje y evitando atascos entre tiles adyacentes.

### 3.2. Gestión de Estados (GameManager & UIManager)
El juego se rige por un patrón Singleton en el `GameManager` y `UIManager`.
*   El `GameManager` centraliza: conteo de monedas locales vs totales del mapa (buscadas al Vuelo en el Start), inicio y parada de cronómetro, estado de Game Over y cálculo del **Récord Local** de guardado mediante `PlayerPrefs`.
*   El `UIManager` gestiona las transiciones visuales (Fade in/out), actualizaciones de barras de vida y paneles de victoria.

### 3.3. Estética y Feedback (Visual y Audio)
*   **Coherencia de Assets:** Uso de un paquete de assets de estética pixel art invernal limpia. Interfaz con diseño moderno y "premium" (colores sólidos, espaciado amplio, sin sobrecarga).
*   **Feedback Sonoro:** Implementación de componentes `AudioSource` individuales para eventos clave como saltar, recibir daño, y recoger monedas, mejorando el "*game feel*".

---

*Proyecto final comprimido en formato .zip.*
