using GaiaLib.Rom;
using Godot;
using System;

public partial class TilemapControl : Control
{
    private int _hoverTileX, _hoverTileY;
    private Vector2[] _hoverBox = new Vector2[5];
    //private Vector2 _drawSize;
    private float _tileSize;
    private float _zoom = 2.0f;
    private bool _isDragging = false;
    private Vector2 _lastPos;
    public static TilemapControl Instance;

    private Vector2 _drawPos, _drawSize;

    //private Vector2 GetDrawSize()
    //{
    //    var ratio = Size.X / Size.Y;
    //    return ratio > ControlTest.TilemapRatio
    //        ? new(Size.Y * ControlTest.TilemapRatio, Size.Y)
    //        : new(Size.X, Size.X / ControlTest.TilemapRatio);
    //}

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
        Reset();
    }

    public override void _Draw()
    {
        //_drawSize = GetDrawSize();
        _tileSize = _drawSize.X / (ControlTest.TilemapTileWidth << 4);
        DrawTextureRect(ControlTest.TilemapTexture, new(_drawPos, _drawSize), false);
        DrawPolyline(_hoverBox, Color.Color8(0, 255, 255));
    }

    private byte GetMapIndex(byte? newValue = null)
    {
        var w = ControlTest.IsEffect ? ControlTest.RomState.EffectTilemapW : ControlTest.RomState.MainTilemapW;
        var map = ControlTest.IsEffect ? ControlTest.RomState.EffectTilemap : ControlTest.RomState.MainTilemap;

        var mOffset = (_hoverTileY >> 4) * w + (_hoverTileX >> 4);
        var ix = (mOffset << 8) | ((_hoverTileY & 0x0F) << 4) | (_hoverTileX & 0x0F);

        var old = map[ix];

        if (newValue != null)
            map[ix] = newValue.Value;

        return old;
    }

    public override void _Input(InputEvent @event)
    {

        if (@event is InputEventMouse motion)
        {
            if (_isDragging)
            {
                Position += motion.Position - _lastPos;
            }
            else
            {
                var pos = motion.Position - this.GlobalPosition - _drawPos;
                if (pos.X < 0 || pos.Y < 0 || pos.X > _drawSize.X || pos.Y > _drawSize.Y)
                {
                    _hoverBox[0] = _hoverBox[1] = _hoverBox[2] = _hoverBox[3] = _hoverBox[4] = new(-1, -1);
                    _hoverTileX = -1;
                    _hoverTileY = -1;
                    return;
                }
                else
                {
                    _hoverTileX = (int)(pos.X / _tileSize);
                    _hoverTileY = (int)(pos.Y / _tileSize);

                    var posX = _hoverTileX * _tileSize;
                    var posY = _hoverTileY * _tileSize;

                    _hoverBox[0] = _hoverBox[4] = new(posX, posY);
                    _hoverBox[1] = new(posX + _tileSize, posY);
                    _hoverBox[2] = new(posX + _tileSize, posY + _tileSize);
                    _hoverBox[3] = new(posX, posY + _tileSize);
                }
            }
            _lastPos = motion.Position;
            QueueRedraw();
        }

        if (@event is InputEventMouseButton mouse)
        {
            if (mouse.Pressed)
                switch (mouse.ButtonIndex)
                {
                    case MouseButton.Right:
                        if (_hoverTileX >= 0 && _hoverTileY >= 0)
                        {
                            ControlTest.SelectedIndex = GetMapIndex();
                        }
                        break;

                    case MouseButton.Left:
                        if (_hoverTileX >= 0 && _hoverTileY >= 0)
                        {
                            GetMapIndex((byte)ControlTest.SelectedIndex);

                            ControlTest.TilemapImage.BlitRect(ControlTest.TilesetImage,
                                new((ControlTest.SelectedIndex & 0x07) << 4, (ControlTest.SelectedIndex & 0xF8) << 1, 16, 16),
                                new(_hoverTileX << 4, _hoverTileY << 4));

                            ControlTest.TilemapTexture.Update(ControlTest.TilemapImage);
                            QueueRedraw();
                        }
                        break;

                    case MouseButton.Middle:
                        _isDragging = true;
                        break;

                    case MouseButton.WheelUp:
                        _drawSize *= 1.25f;
                        _drawPos *= 1.25f;
                        QueueRedraw();
                        break;

                    case MouseButton.WheelDown:
                        _drawSize *= 0.75f;
                        _drawPos *= 0.75f;
                        QueueRedraw();
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
                        _isDragging = false;
                        break;
                }
        }

        if(@event is InputEventKey key)
        {
            return;
        }

        base._Input(@event);
    }

    public void Reset()
    {
        _drawSize = new(ControlTest._mapWidth * 4f, ControlTest._mapHeight * 4f);
        _drawPos = new(0, 0);
        QueueRedraw();
    }
}
