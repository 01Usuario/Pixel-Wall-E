using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    public TMP_InputField sizeInput;
    public Button resizeButton;

    public Button confirmButton;
    public RawImage canvasDisplay;
    public int canvasSize;
    public Texture2D canvasTexture;
    public Color[,] pixels;
    private Evaluator evaluator;

    public Evaluator Evaluator => evaluator;

    private readonly Dictionary<Color, string> colorMap = new()
    {
        [Color.red] = "Red",
        [Color.blue] = "Blue",
        [Color.green] = "Green",
        [Color.yellow] = "Yellow",
        [new Color(1f, 0.65f, 0f)] = "Orange",

        [new Color(0.5f, 0f, 0.5f)] = "Purple",
        [Color.black] = "Black",
        [Color.white] = "White",
        [new Color(0f, 0f, 0f, 0f)] = "Transparent"
    };
    public TMP_Text errorText;

    void Start()
    {
        InitializeCanvas(100);
        sizeInput.interactable = false;
        confirmButton.interactable = false;
        confirmButton.onClick.AddListener(OnConfirmResize);
    }

    private void InitializeCanvas(int newSize)
    {
        canvasSize = newSize;
        canvasTexture = new Texture2D(canvasSize, canvasSize, TextureFormat.RGBA32, false);
        canvasTexture.filterMode = FilterMode.Point; // ¡Importante para píxeles nítidos!
        canvasDisplay.texture = canvasTexture;
        ClearCanvas();
    }

    // Borra el lienzo (píxeles blancos)
    public void ClearCanvas()
    {
        Color[] pixels = new Color[canvasSize * canvasSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        canvasTexture.SetPixels(pixels);
        canvasTexture.Apply();
    }
    public void OnResizeButtonClick()
    {
        sizeInput.interactable = true;
        confirmButton.interactable = true;
        sizeInput.text = canvasSize.ToString(); // Mostrar tamaño actual
    }
    public void OnConfirmResize()
    {
        if (int.TryParse(sizeInput.text, out int newSize))
        {
            newSize = Mathf.Clamp(newSize, 1, 512);
            InitializeCanvas(newSize);
            sizeInput.interactable = false; // Ocultar panel
            confirmButton.interactable = false; // Ocultar botón
        }
        else
        {
            errorText.text = "El tamaño introducido no es válido, por favor vuelva a introducir un tamaño válido";
        }
    }
    public string GetPixelColor(int x, int y)
    {
        if (x < 0 || x >= canvasSize || y < 0 || y >= canvasSize)
            return "OutOfBounds";

        Color pixelColor = canvasTexture.GetPixel(x, y);

        foreach (var kvp in colorMap)
        {
            if (pixelColor == kvp.Key)
            {
                return kvp.Value;
            }
        }
        return "Unknown";
    }
    public Vector2 CanvasToWorldPosition(Vector2 canvasPosition)
    {
        // Convertir posición en píxeles a posición en el mundo
        float pixelSize = 1f / canvasSize;
        return new Vector2(
            (canvasPosition.x * pixelSize) - 0.5f + pixelSize/2f,
            (canvasPosition.y * pixelSize) - 0.5f + pixelSize/2f
        );
    }
}