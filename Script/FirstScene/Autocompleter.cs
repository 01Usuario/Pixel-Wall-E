using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

using System.Linq;

public class Autocompleter : MonoBehaviour
{
    public TMP_InputField codeInput;
    public RectTransform suggestionPanel;
    public TMP_Text suggestionItemTemplate;
    public float maxPanelHeight = 200f;
    
    private readonly string[] keywords = {
        "Spawn", "Color", "Size", "DrawLine", "DrawCircle", "DrawRectangle",
        "Fill", "GoTo", "IsCanvasColor", "IsBrushSize", "GetActualX", "GetActualY",
        "GetCanvasSize", "GetColorCount", "IsBrushColor"
    };

    private Dictionary<string, string> keywordSignatures = new Dictionary<string, string>()
    {
        {"Spawn", "Spawn( , )"},
        {"Color", "Color(\" \")"},
        {"Size", "Size(number)"},
        {"DrawLine", "DrawLine( , , )"},
        {"DrawCircle", "DrawCircle( ,  ,  )"},
        {"DrawRectangle", "DrawRectangle( ,  ,  ,  ,  )"},
        {"Fill", "Fill()"},
        {"GoTo", "GoTo[ ]( )"},
        {"IsCanvasColor", "IsCanvasColor(\" \", , )"},
        {"IsBrushSize", "IsBrushSize( )"},
        {"GetActualX", "GetActualX()"},
        {"GetActualY", "GetActualY()"},
        {"GetCanvasSize", "GetCanvasSize()"},
        {"GetColorCount", "GetColorCount(\" \", , , , )"},
        {"IsBrushColor", "IsBrushColor(\" \")"}
    };

    private List<GameObject> suggestionItems = new List<GameObject>();
    private bool isShowingSuggestions = false;
    private int currentWordStart = 0;
    private int currentWordEnd = 0;

    void Start()
    {
        suggestionItemTemplate.gameObject.SetActive(false);
        suggestionPanel.gameObject.SetActive(false);
        
        SetupLayoutComponents();
        codeInput.onValueChanged.AddListener(OnCodeChanged);
    }

    void SetupLayoutComponents()
    {
        VerticalLayoutGroup layout = suggestionPanel.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = suggestionPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(1, 1, 1, 1);
            layout.spacing = 0.5f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        ContentSizeFitter fitter = suggestionPanel.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = suggestionPanel.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    void Update()
    {
        if (isShowingSuggestions && (Input.GetKeyDown(KeyCode.Return) && suggestionItems.Count > 0))
        {
            SelectSuggestion(suggestionItems[0].GetComponent<TMP_Text>().text);
        }
        
        if (isShowingSuggestions && Input.GetMouseButtonDown(0) 
            && !RectTransformUtility.RectangleContainsScreenPoint(suggestionPanel, Input.mousePosition))
        {
            HideSuggestions();
        }
    }

    private void OnCodeChanged(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            HideSuggestions();
            return;
        }

        string currentWord = GetCurrentWord();
        if (string.IsNullOrEmpty(currentWord))
        {
            HideSuggestions();
            return;
        }

        var matches = keywords
            .Where(k => k.ToLower().Contains(currentWord.ToLower()))
            .OrderBy(k => k.StartsWith(currentWord) ? 0 : 1) 
            .ThenBy(k => k.Length)
            .Take(5) 
            .ToList();

        if (matches.Count == 0) 
        {
            HideSuggestions();
        }
        else 
        {
            ShowSuggestions(matches);
            PositionPanelNearCaret();
        }
    }

    private void PositionPanelNearCaret()
{
    Vector2 panelPos = codeInput.transform.position;
    
    panelPos.x = Mathf.Clamp(panelPos.x, 0, Screen.width - suggestionPanel.sizeDelta.x);
    panelPos.y = Mathf.Clamp(panelPos.y, suggestionPanel.sizeDelta.y, Screen.height);
    
    suggestionPanel.position = panelPos;
}

    private string GetCurrentWord()
    {
        int caretPos = codeInput.caretPosition;
        string text = codeInput.text;

        if (text.Length == 0 || caretPos == 0)
        {
            currentWordStart = currentWordEnd = 0;
            return "";
        }

        currentWordStart = caretPos - 1;
        while (currentWordStart >= 0 && !IsWordBoundary(text[currentWordStart]))
            currentWordStart--;
        currentWordStart++;

        currentWordEnd = caretPos;
        while (currentWordEnd < text.Length && !IsWordBoundary(text[currentWordEnd]))
            currentWordEnd++;

        if (currentWordStart >= currentWordEnd)
            return "";

        return text.Substring(currentWordStart, currentWordEnd - currentWordStart);
    }

    private bool IsWordBoundary(char c)
    {
        return char.IsWhiteSpace(c) || c == '(' || c == ')' || c == '[' || c == ']' || c == ',';
    }
    
    private void ShowSuggestions(List<string> suggestions)
    {
        foreach (var item in suggestionItems)
            Destroy(item);
        suggestionItems.Clear();

        foreach (string suggestion in suggestions)
        {
            if (!keywordSignatures.TryGetValue(suggestion, out var signature))
                signature = suggestion;

            TMP_Text newItem = Instantiate(suggestionItemTemplate, suggestionPanel);
            newItem.text = signature;
            newItem.gameObject.SetActive(true);

            if (!newItem.TryGetComponent<Button>(out var btn))
                btn = newItem.gameObject.AddComponent<Button>();

            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.8f, 0.8f, 0.8f);
            colors.highlightedColor = new Color(0.6f, 0.6f, 1f);
            btn.colors = colors;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectSuggestion(signature));
            suggestionItems.Add(newItem.gameObject);
        }

        float panelHeight = Mathf.Min(maxPanelHeight, 
            suggestionItems.Count * (suggestionItemTemplate.rectTransform.sizeDelta.y + 1));
        suggestionPanel.sizeDelta = new Vector2(suggestionPanel.sizeDelta.x, panelHeight);

        suggestionPanel.gameObject.SetActive(true);
        isShowingSuggestions = true;
    }

    private void SelectSuggestion(string suggestion)
    {
        string text = codeInput.text;
        
        string newText = text.Substring(0, currentWordStart) + suggestion;
        
        if (currentWordEnd < text.Length)
            newText += text.Substring(currentWordEnd);
        
        codeInput.text = newText;
        
        codeInput.caretPosition = currentWordStart + suggestion.Length;
        
        HideSuggestions();
    }

    private void HideSuggestions()
    {
        suggestionPanel.gameObject.SetActive(false);
        isShowingSuggestions = false;
    }
}