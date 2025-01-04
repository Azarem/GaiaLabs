using Godot;
using System;

public partial class PaletteSelector : Control
{
    public static PaletteSelector Instance { get; private set; }

    float _colorSize = 20f;

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set { _selectedIndex = value; QueueRedraw(); }
    }

    public PaletteSelector()
    {
        FocusMode = FocusModeEnum.Click;
        CustomMinimumSize = new(300, 200);
    }

    public override void _EnterTree()
    {
        FocusMode = FocusModeEnum.Click;
        Instance = this;
        base._EnterTree();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (!HasFocus())
            return;

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
                        var value = SelectedIndex = (int)Math.Min(pos.Y / _colorSize, 7);
                        ControlTest.ReloadGraphicSet(value);

                        var set = ControlTest.TilesetCurrent;
                        var ix = TilesetEditor.Instance.SelectedIndex;
                        var gfxIx = GfxSelector.Instance.SelectedIndex;

                        //Modify tileset
                        var six = ix << 1;
                        var sample = set[six] | (set[six + 1] << 8);
                        sample &= 0xE3FF;
                        sample |= value << 10;
                        set[six] = (byte)sample;
                        set[six + 1] = (byte)(sample >> 8);

                        var dstX = ((ix >> 2 & 0x7) << 1) + (ix & 1);
                        var dstY = (ix >> 5 << 1) + ((ix & 2) != 0 ? 1 : 0);

                        ControlTest.TilesetImage.BlitRect(ControlTest.GfxImage,
                            new((gfxIx & 0xF) << 3, (gfxIx & 0x1F0) >> 1, 8, 8),
                            new(dstX << 3, dstY << 3));

                        ControlTest.TilesetTexture.Update(ControlTest.TilesetImage);

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
        for (int i = 0; i < 8; i++)
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

        if (_selectedIndex >= 0 && _selectedIndex < 8)
        {
            var y = _selectedIndex * _colorSize;
            var w = _colorSize * 15 - 1;
            var h = _colorSize - 1;
            DrawDashedLine(new(0, y), new(w, y), new Color(1, 1, 1));
            DrawDashedLine(new(w, y), new(w, y + h), new Color(1, 1, 1));
            DrawDashedLine(new(w, y + h), new(0, y + h), new Color(1, 1, 1));
            DrawDashedLine(new(0, y + h), new(0, y), new Color(1, 1, 1));
        }
    }


}
