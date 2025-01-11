using Godot;
using System;

public partial class PAnimSelector : Control
{
    public static PAnimSelector Instance { get; private set; }

    float _colorSize = 20f;

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set { _selectedIndex = value; QueueRedraw(); }
    }

    public PAnimSelector()
    {
        FocusMode = FocusModeEnum.Click;
        CustomMinimumSize = new(_colorSize * 15, _colorSize * 16);
    }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        //if (!HasFocus())
        //    return;

        if (@event is InputEventMouseButton mouse)
        {
            var pos = mouse.Position - GlobalPosition;

            if (pos.X < 0 || pos.Y < 0 || pos.X >= Size.X || pos.Y >= Size.Y)
                return;

            if (mouse.Pressed)
                switch (mouse.ButtonIndex)
                {
                    case MouseButton.Right:
                        break;

                    case MouseButton.Left:
                        SelectedIndex = (int)Math.Min(pos.Y / _colorSize, 15);
                        break;

                    case MouseButton.Middle:
                        break;

                }
            else
                switch (mouse.ButtonIndex)
                {
                    case MouseButton.Left:
                        break;
                    case MouseButton.Right:
                        break;
                    case MouseButton.Middle:
                        break;
                }
        }

    }

    public override void _Draw()
    {
        base._Draw();

        //var lineSize = pixelSize * 15;
        var pal = ControlTest.PaletteData;

        int cIx = 0;
        var lineY = _colorSize / 2;
        for (int i = 0; i < 16; i++)
        {
            cIx += 4; //Skip first (transparent) color
            var lineX = 0f;
            for (int z = 0; z < 15; z++)
            {
                var color = new Color(pal[cIx++] / 255f, pal[cIx++] / 255f, pal[cIx++] / 255f);
                cIx++;

                DrawLine(new(lineX, lineY), new(lineX + _colorSize, lineY), color, _colorSize);

                lineX += _colorSize;
            }
            lineY += _colorSize;
        }

        if (_selectedIndex >= 0 && _selectedIndex < 16)
        {
            var y = _selectedIndex * _colorSize;
            var w = _colorSize * 15;
            var h = _colorSize;
            DrawDashedLine(new(0, y), new(w, y), new Color(1, 1, 1));
            DrawDashedLine(new(w, y), new(w, y + h), new Color(1, 1, 1));
            DrawDashedLine(new(w, y + h), new(0, y + h), new Color(1, 1, 1));
            DrawDashedLine(new(0, y + h), new(0, y), new Color(1, 1, 1));
        }
    }
}

