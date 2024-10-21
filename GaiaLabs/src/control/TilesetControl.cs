using Godot;
using System;

public partial class TilesetControl : Control
{
    private Vector2[] _gridLines = new Vector2[16 * 16 * 4];
    private Vector2[] _selectionBox = new Vector2[5];

    public override void _Draw()
    {
        base._Draw();
        var size = Math.Min(Size.X, Size.Y);
        var gridSize = size / 16;

        int c = 0;
        float x2 = 0, y2 = 0;
        for (int y = 0; y < 16; y++, x2 = 0, y2 += gridSize)
            for (int x = 0; x < 16; x++, x2 += gridSize)
            {
                _gridLines[c++] = new(0, y2);
                _gridLines[c++] = new(size, y2);
                _gridLines[c++] = new(x2, 0);
                _gridLines[c++] = new(x2, size);
            }

        var ix = ControlTest.SelectedIndex;
        var posX = (ix & 0xF) * gridSize;
        var posY = (ix >> 4) * gridSize;
        _selectionBox[0] = _selectionBox[4] = new(posX, posY);
        _selectionBox[1] = new(posX + gridSize, posY);
        _selectionBox[2] = new(posX + gridSize, posY + gridSize);
        _selectionBox[3] = new(posX, posY + gridSize);


        DrawTextureRect(ControlTest.TilesetTexture, new(0, 0, size, size), false);

        DrawMultiline(_gridLines, Color.Color8(160, 160, 160, 80));
        DrawPolyline(_selectionBox, Color.Color8(255, 0, 255, 255));
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if(@event is InputEventMouseButton mouse)
        {
            var size = Math.Min(Size.X, Size.Y) / 16f;
            if (mouse.ButtonIndex == MouseButton.Left)
            {
                var tileX = (int)(mouse.Position.X / size);
                var tileY = (int)(mouse.Position.Y / size);

                ControlTest.SelectedIndex = (tileY << 4) + tileX;
                QueueRedraw();
            }
        }

    }
}
