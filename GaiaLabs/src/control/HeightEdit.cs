using Godot;
using System.Linq;
using System.Text.RegularExpressions;

public partial class HeightEdit : LineEdit
{
    public static HeightEdit Instance { get; set; }

    public HeightEdit()
    {
        TextChanged += HeightEdit_TextChanged;
        TextSubmitted += HeightEdit_TextSubmitted;
    }

    private void HeightEdit_TextSubmitted(string newText)
    {
        if (newText.Length > 0 && int.TryParse(newText, out var height))
            ControlTest.ChangeHeight(height);
    }

    private void HeightEdit_TextChanged(string newText)
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
        Text = ControlTest.TilemapTileHeight.ToString();
    }
}
