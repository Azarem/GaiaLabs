using Godot;
using System;

public partial class TilesetControl : Control
{
    public static TilesetControl Instance;

    private Vector2[] _gridLines = new Vector2[16 * 16 * 4];
    private Vector2[] _selectionBox = new Vector2[5];
    private Vector2 _drawSize;
    private float _tileSize;

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    public override void _Draw()
    {
        base._Draw();

        var ratio = Size.X / Size.Y;
        _drawSize = ratio >= 0.25f ? new(Size.Y * 0.25f, Size.Y) : new(Size.X, Size.X / 0.25f);

        //var size = Math.Min(Size.X, Size.Y);
        _tileSize = _drawSize.X / 8f;

        int c = 0;
        float x2 = 0, y2 = 0;
        for (int y = 0; y < 32; y++, x2 = 0, y2 += _tileSize)
            for (int x = 0; x < 8; x++, x2 += _tileSize)
            {
                _gridLines[c++] = new(0, y2);
                _gridLines[c++] = new(_drawSize.X, y2);
                _gridLines[c++] = new(x2, 0);
                _gridLines[c++] = new(x2, _drawSize.Y);
            }

        var ix = ControlTest.SelectedIndex;
        var posX = (ix & 0x7) * _tileSize;
        var posY = (ix >> 3) * _tileSize;
        _selectionBox[0] = _selectionBox[4] = new(posX, posY);
        _selectionBox[1] = new(posX + _tileSize, posY);
        _selectionBox[2] = new(posX + _tileSize, posY + _tileSize);
        _selectionBox[3] = new(posX, posY + _tileSize);


        DrawTextureRect(ControlTest.TilesetTexture, new(0, 0, _drawSize), false);

        DrawMultiline(_gridLines, Color.Color8(160, 160, 160, 80));
        DrawPolyline(_selectionBox, Color.Color8(255, 0, 255, 255));
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseButton mouse)
        {
            var pos = mouse.Position - GlobalPosition;
            if (pos.X < 0 || pos.Y < 0 || pos.X >= Size.X || pos.Y >= Size.Y)
                return;

            if (mouse.ButtonIndex == MouseButton.Left)
            {
                var tileX = (int)(pos.X / _tileSize);
                var tileY = (int)(pos.Y / _tileSize);

                ControlTest.SelectedIndex = (tileY << 3) + tileX;
                QueueRedraw();
            }
        }

    }

    public void Reset()
    {
        QueueRedraw();
    }
}
