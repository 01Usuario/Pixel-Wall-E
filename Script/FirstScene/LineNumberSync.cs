using TMPro;
using UnityEngine;
using System.Linq;

public class LineNumberSync : MonoBehaviour
{
    public TMP_Text lineNumbers;
    public TMP_InputField codeInput;

    void Start()
    {
        codeInput.onValueChanged.AddListener(UpdateLineNumbers);
    }

    void UpdateLineNumbers(string text)
    {
        int lineCount = codeInput.text.Split('\n').Length;
        lineNumbers.text = string.Join("\n", Enumerable.Range(1, lineCount));
    }
}