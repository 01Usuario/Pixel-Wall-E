# Pixel Wall-E 🎮🖌️

¡Ayuda a Wall-E a crear increíbles obras de pixel art mediante comandos! Este proyecto es una aplicación Unity que combina programación y arte, permitiendo dibujar en un lienzo mediante instrucciones como `Spawn`, `DrawLine`, y más.

---

## Características Principales ✨
- **Editor de código integrado** con numeración de líneas y resaltado.
- **Lienzo redimensionable** (desde 10x10 hasta 512x512 píxeles).
- **Comandos básicos**:
  - `Spawn(x, y)`: Posiciona a Wall-E en el lienzo.
  - `Color("NombreColor")`: Cambia el color del píxel.
  - `DrawLine(dirX, dirY, distance)`: Dibuja líneas en 8 direcciones.
- **Carga/Guardado** de archivos `.pw`.
- **Interfaz intuitiva** con dos escenas separadas: edición y ejecución.

## Composicion
-**FileManager.cs**
  **Descripción:** 
    Gestiona operaciones de E/S para archivos de código fuente.
    Objetivo: Proporcionar persistencia para programas mediante carga/guardado.
  **Técnicas:**
    Uso de System.IO para operaciones de archivos
    Patrón de diseño Singleton implícito (vinculado a UI)
    Manejo de errores con try-catch para operaciones de disco

**LineNumberSync.cs**
  **Descripción:** Sincroniza números de línea con el editor de código.
  Objetivo: Mejorar la experiencia de edición mostrando referencias visuales. 
  **Técnicas:**
  Event-driven programming con onValueChanged
  Procesamiento eficiente de strings con Split('\n')
  Actualización dinámica del layout

**CanvasManager.cs**
  **Descripción:**
    Control central del lienzo de dibujo.
    Objetivo: Administrar el estado y representación del área de dibujo.
  **Técnicas:**
    Gestión de texturas 2D con Texture2D
    Mapeo bidireccional colores-nombres (Dictionary<Color, string>)
    Conversión de sistemas de coordenadas (píxeles a world space)

**DrawingEngine.cs**
  **Descripción:**: Motor de renderizado de primitivas gráficas.
  Objetivo: Implementar algoritmos de dibujo eficientes.
  **Técnicas:**
  Algoritmo de Bresenham para líneas
  Flood fill para relleno de áreas
  Manipulación directa de píxeles via arrays multidimensionales

**Brush.cs**
  **Descripción:**: Modela el estado del pincel virtual.
  Objetivo: Mantener y actualizar propiedades de dibujo actuales.
  **Técnicas:**
  Patrón de diseño Memento implícito
  Propiedades autoimplementadas para estado mutable

**Lexer.cs**
  **Descripción:**: Analizador léxico del lenguaje.
  Objetivo: Convertir texto fuente en tokens válidos.
  **Técnicas:**
  Máquina de estados finitos para clasificación de tokens
  Uso de HashSet para búsqueda rápida de keywords
  Manejo de errores léxicos con excepciones específicas

**Parser.cs**
  **Descripción:**: Analizador sintáctico descendente recursivo.
  Objetivo: Construir AST a partir de secuencia de tokens.
  **Técnicas:**
  Parsing expression grammar (PEG) implícito
  Manejo de precedencia de operadores
  Técnica de lookahead con PeekNextToken

**ValidatorRunner.cs**
  **Descripción:**: Coordinador de validación semántica.
  Objetivo: Verificar corrección contextual del AST.
  **Técnicas:**
  Patrón Visitor para recorrido de nodos
  Inyección de dependencias para validadores específicos
  Acumulación de errores/warnings en contexto compartido

**Evaluator.cs**
  **Descripción:**: Intérprete del árbol sintáctico.
  Objetivo: Ejecutar instrucciones y modificar estado del programa.
  **Técnicas:**
  Patrón Interpreter para evaluación de nodos
  Manejo de scope mediante Dictionary para variables
  Evaluación lazy de expresiones booleanas

**Autocompleter.cs**
  **Descripción:**: Sistema de sugerencias de código.
  Objetivo: Asistir al usuario durante la edición.
  **Técnicas:**
  Búsqueda por prefijos con comparación insensible a mayúsculas
  Dynamic UI pooling para sugerencias
  Posicionamiento relativo al cursor

**IASTValidator.cs**
  **Descripción:**: Sistema de validación modular.
  Objetivo: Permitir extensión de reglas semánticas.
  **Técnicas:**
  Uso de genéricos para validadores tipo-safe
  Patrón Strategy para implementaciones específicas
  Composición de validaciones mediante interface común

**Token.cs**
  **Descripción:**: Modelo de datos para unidades léxicas.
  Objetivo: Representar componentes elementales del código.
  **Técnicas:**
  Enumeraciones fuertemente tipadas
  Inmutabilidad para seguridad en hilos

**ASTNode.cs**
  **Descripción:**: Jerarquía de nodos del árbol sintáctico.
  Objetivo: Representar estructura del programa.
  **Técnicas:**
  Patrón Composite para estructura de árbol
  Clases base abstractas para polimorfismo
  Jerarquía de tipos para diferentes instrucciones

## Requisitos 📋
- **Unity 2021.3 o superior**.
- **Paquetes recomendados**:
  - [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) (ya incluido).
