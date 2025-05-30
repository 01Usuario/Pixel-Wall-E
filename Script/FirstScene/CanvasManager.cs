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
    private Texture2D canvasTexture;

    public TMP_Text errorText;

    void Start(){
        InitializeCanvas(100);
        sizeInput.interactable=false;
        confirmButton.interactable=false;
        confirmButton.onClick.AddListener(OnConfirmResize);
        PaintTestPixel();
    }

   private void InitializeCanvas(int newSize) {
        canvasSize = newSize;
        canvasTexture = new Texture2D(canvasSize, canvasSize, TextureFormat.RGBA32, false);
        canvasTexture.filterMode = FilterMode.Point; // ¡Importante para píxeles nítidos!
        canvasDisplay.texture = canvasTexture;
        ClearCanvas();
    }

    // Borra el lienzo (píxeles blancos)
    public void ClearCanvas() {
        Color[] pixels = new Color[canvasSize * canvasSize];
        for (int i = 0; i < pixels.Length; i++) {
            pixels[i] = Color.white;
        }
        canvasTexture.SetPixels(pixels);
        canvasTexture.Apply();
    }
    // Llamado por el botón de redimensión
    public void OnResizeButtonClick() {
        sizeInput.interactable=true;
        confirmButton.interactable=true; 
        sizeInput.text = canvasSize.ToString(); // Mostrar tamaño actual
    }
     public void OnConfirmResize() {
        if (int.TryParse(sizeInput.text, out int newSize)) {
            newSize = Mathf.Clamp(newSize, 1, 512);
            InitializeCanvas(newSize);
            sizeInput.interactable=false; // Ocultar panel
            confirmButton.interactable=false; // Ocultar botón
        } else {
            errorText.text = "El tamaño introducido no es válido, por favor vuelva a introducir un tamaño válido";
        }
    }

    private void PaintTestPixel() {
        SetPixel(0, 0, Color.red);
        SetPixel(canvasSize-1, canvasSize-1, Color.blue);

    }
    public void SetPixelDirect(int x, int y, Color color)
    {
        if (x < 0 || y < 0 || x >=canvasSize || y >= canvasSize) return;
        canvasTexture.SetPixel(x, y, color);
    }

    // Método para pintar un píxel
    public void SetPixel(int x, int y, Color color)
    {
        if (x < 0 || y < 0 || x >= canvasSize || y >= canvasSize) return;
        canvasTexture.SetPixel(x, y, color);
        canvasTexture.Apply();
    }
}
