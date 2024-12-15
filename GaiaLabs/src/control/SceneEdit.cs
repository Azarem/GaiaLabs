using Godot;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

public partial class SceneEdit : LineEdit
{
    public SceneEdit()
    {
        CaretBlink = true;
        TextChanged += SceneEdit_TextChanged;
        TextSubmitted += SceneEdit_TextSubmitted;
    }

    private void SceneEdit_TextSubmitted(string newText)
    {
        if (newText.Length > 0 && int.TryParse(newText, NumberStyles.HexNumber, null, out var scene))
            ControlTest.LoadScene(scene);
    }

    private void SceneEdit_TextChanged(string newText)
    {
        var caret = this.CaretColumn;
        Text = Regex.Replace(newText, "[^a-fA-F0-9]", "").ToUpper();
        CaretColumn = caret;
    }

}
