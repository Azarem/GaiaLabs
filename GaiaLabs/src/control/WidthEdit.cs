using Godot;
using System.Linq;
using System.Text.RegularExpressions;

public partial class WidthEdit : LineEdit
{
    public static WidthEdit Instance { get; set; }

    public WidthEdit()
    {
        CaretBlink = true;
        TextChanged += WidthEdit_TextChanged;
        TextSubmitted += WidthEdit_TextSubmitted;
    }

    private void WidthEdit_TextSubmitted(string newText)
    {
        if (newText.Length > 0 && int.TryParse(newText, out var width))
            ControlTest.ChangeWidth(width);
    }

    private void WidthEdit_TextChanged(string newText)
    {
        var caret = CaretColumn;
        Text = Regex.Replace(newText, "[^0-9-]", "").ToUpper();
        CaretColumn = caret;
    }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    public void Reset()
    {
        Text = ControlTest.TilemapWidth.ToString();
    }

}
