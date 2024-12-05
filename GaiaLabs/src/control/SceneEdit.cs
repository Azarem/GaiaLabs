using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class SceneEdit : LineEdit
{
    private static readonly Key[] _acceptedKeys = new[] {
        Key.Kp1, Key.Key1,
        Key.Kp2, Key.Key2,
        Key.Kp3, Key.Key3,
        Key.Kp4, Key.Key4,
        Key.Kp5, Key.Key5,
        Key.Kp6, Key.Key6,
        Key.Kp7, Key.Key7,
        Key.Kp8, Key.Key8,
        Key.Kp9, Key.Key9,
        Key.Kp0, Key.Key0,
        Key.A, Key.B, Key.C, Key.D, Key.E, Key.F,
        Key.Delete,
        Key.Backspace
    };

    public SceneEdit()
    {

        this.CaretBlink = true;
        this.TextChanged += SceneEdit_TextChanged;
    }

    private void SceneEdit_TextChanged(string newText)
    {
        var caret = this.CaretColumn;
        Text = Regex.Replace(newText, "[^a-fA-F0-9]", "").ToUpper();
        CaretColumn = caret;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key)
        {
            if (_acceptedKeys.Contains(key.Keycode))
            {
                if (Text.Length > 1 && key.Keycode != Key.Delete && key.Keycode != Key.Backspace)
                    return;
            }
            else if (key.Keycode == Key.Enter)
            {
                if (Text.Length > 0)
                    ControlTest.LoadScene(int.Parse(Text, System.Globalization.NumberStyles.HexNumber));
            }
            else
                return;
        }

        base._Input(@event);
    }
}
