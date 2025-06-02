using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DrawingEngine
{
    private int canvasSize;
    private Color[,] pixels;
    private Dictionary<string, Color> colorMap = new()
    {
        ["Red"] = Color.red,
        ["Blue"] = Color.blue,
        ["Green"] = Color.green,
        ["Yellow"] = Color.yellow,
        ["Orange"] = new Color(1f, 0.65f, 0f),
        ["Purple"] = new Color(0.5f, 0f, 0.5f),
        ["Black"] = Color.black,
        ["White"] = Color.white,
        ["Transparent"] = new Color(0, 0, 0, 0)
    };

    public DrawingEngine(int size)
    {
        canvasSize = size;
        pixels = new Color[canvasSize, canvasSize];
        ClearCanvas();
    }

    public void ClearCanvas()
    {
        for (int y = 0; y < canvasSize; y++)
            for (int x = 0; x < canvasSize; x++)
            {
                pixels[x, y] = colorMap["White"];
            }
    }

    public void UpdateTexture(Texture2D texture)
    {
        Color[] flatPixels = new Color[canvasSize * canvasSize];
        for (int y = 0; y < canvasSize; y++)
            for (int x = 0; x < canvasSize; x++)
            {
                flatPixels[y * canvasSize + x] = pixels[x, y];
            }
        texture.SetPixels(flatPixels);
        texture.Apply();
    }
    public void DrawLine(int startX, int startY, int endX, int endY, string colorName, int brushSize)
    {
        if (!colorMap.TryGetValue(colorName, out Color color))
        {
            Debug.LogError($"Color desconocido: {colorName}");
            return;
        }

        // 2. Aplicar algoritmo de Bresenham
        int dx = Mathf.Abs(endX - startX);
        int dy = Mathf.Abs(endY - startY);
        int sx = startX < endX ? 1 : -1;
        int sy = startY < endY ? 1 : -1;
        int err = dx - dy;
        int currentX = startX;
        int currentY = startY;

        while (true)
        {
            DrawBrushAt(currentX, currentY, color, brushSize);
            if (currentX == endX && currentY == endY) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                currentX += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                currentY += sy;
            }
        }
    }
    public void DrawCircle(int centerX, int centerY, int radius, string colorName, int brushSize)
    {
        if (!colorMap.TryGetValue(colorName, out Color color))
        {
            Debug.LogError($"Color desconocido: {colorName}");
            return;
        }

        int x = radius;
        int y = 0;
        int err = 0;

        while (x >= y)
        {
            DrawBrushAt(centerX + x, centerY + y, color, brushSize);
            DrawBrushAt(centerX + y, centerY + x, color, brushSize);
            DrawBrushAt(centerX - y, centerY + x, color, brushSize);
            DrawBrushAt(centerX - x, centerY + y, color, brushSize);
            DrawBrushAt(centerX - x, centerY - y, color, brushSize);
            DrawBrushAt(centerX - y, centerY - x, color, brushSize);
            DrawBrushAt(centerX + y, centerY - x, color, brushSize);
            DrawBrushAt(centerX + x, centerY - y, color, brushSize);

            y++;
            err += 1 + 2*y;
            if (2*(err - x) + 1 > 0)
            {
                x--;
                err += 1 - 2*x;
            }
        }
    }
    public void DrawRectangle(int startX, int startY, int dirX, int dirY, int distance, int width, int height, string colorName, int brushSize)
    {
        Debug.Log("Evaluando DrawRectangle");
        // Calcular centro del rectángulo
        int centerX = startX + dirX * distance;
        int centerY = startY + dirY * distance;
        
        // Dibujar contorno
        DrawRectangle(centerX, centerY, width, height, colorName, brushSize);
    }
    public void DrawRectangle(int centerX, int centerY, int width, int height, string colorName, int brushSize)
    {
    
    // Calcular esquinas del rectángulo
        int left = centerX - width / 2;
        int right = centerX + width / 2;
        int top = centerY - height / 2;
        int bottom = centerY + height / 2;
        Debug.Log("Dibujando Rectángulo");
        // Dibujar los 4 lados
        DrawLine(left, top, right, top, colorName, brushSize);     // Línea superior
        DrawLine(right, top, right, bottom, colorName, brushSize); // Línea derecha
        DrawLine(right, bottom, left, bottom, colorName, brushSize); // Línea inferior
        DrawLine(left, bottom, left, top, colorName, brushSize);   // Línea izquierda
    }

    private void DrawBrushAt(int centerX, int centerY, Color color, int brushSize)
    {
        int halfSize = brushSize / 2;

        for (int xOffset = -halfSize; xOffset <= halfSize; xOffset++)
        {
            for (int yOffset = -halfSize; yOffset <= halfSize; yOffset++)
            {
                int x = centerX + xOffset;
                int y = centerY + yOffset;

                if (x >= 0 && x < canvasSize && y >= 0 && y < canvasSize)
                {
                    pixels[x, y] = color;
                }
            }
        }
    }
}
