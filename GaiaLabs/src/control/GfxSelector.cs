using Godot;
using System;

public partial class GfxSelector : Control
{
    public static GfxSelector Instance { get; private set; }

    //private int _currentPalette;
    //public int CurrentPalette
    //{
    //    get => _currentPalette;
    //    set
    //    {
    //        _currentPalette = value;
    //    }
    //}

    private int _hoverTileX, _hoverTileY;
    private Vector2[] _hoverBox = new Vector2[5];
    private Vector2[] _gridLines = new Vector2[16 * 16 * 4];
    private Vector2[] _selectionBox = new Vector2[5];
    //private Vector2 _drawSize;
    private float _tileSize;
    private float _zoom = 2.0f;
    private bool _isDragging = false;
    private Vector2 _lastPos;
    private Vector2 _drawPos;
    public Color _selectionColor = new(1, 0, 1);

    public int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;
            QueueRedraw();
        }
    }


    public GfxSelector()
    {
        FocusMode = FocusModeEnum.Click;
    }

    public override void _EnterTree()
    {
        FocusMode = FocusModeEnum.Click;
        Instance = this;
        base._EnterTree();
        Reset();
    }

    public override void _Draw()
    {
        base._Draw();

        var size = Size;
        //_drawSize = GetDrawSize();
        _tileSize = size.X / 16;
        DrawTextureRect(ControlTest.GfxTexture, new(_drawPos, size), false);

        if (_selectedIndex >= 0)
        {
            var x = (_selectedIndex & 0xF) * _tileSize + _drawPos.X;
            var y = (_selectedIndex >> 4) * _tileSize + _drawPos.Y;
            DrawDashedLine(new(x, y), new(x + _tileSize, y), _selectionColor, 2);
            DrawDashedLine(new(x + _tileSize, y), new(x + _tileSize, y + _tileSize), _selectionColor, 2);
            DrawDashedLine(new(x + _tileSize, y + _tileSize), new(x, y + _tileSize), _selectionColor, 2);
            DrawDashedLine(new(x, y + _tileSize), new(x, y), _selectionColor, 2);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!HasFocus())
            return;

        if (@event is InputEventMouse motion)
        {
            if (_isDragging)
            {
                _drawPos += motion.Position - _lastPos;
                QueueRedraw();
            }
            else
            {
                var pos = motion.Position - this.GlobalPosition - _drawPos;
                if (pos.X < 0 || pos.Y < 0 || pos.X >= Size.X || pos.Y >= Size.Y)
                {
                    _hoverBox[0] = _hoverBox[1] = _hoverBox[2] = _hoverBox[3] = _hoverBox[4] = new(-1, -1);
                    _hoverTileX = -1;
                    _hoverTileY = -1;
                    return;
                }
                else
                {
                    var hoverTileX = (int)(pos.X / _tileSize);
                    var hoverTileY = (int)(pos.Y / _tileSize);

                    var isTileChange = hoverTileX != _hoverTileX || hoverTileY != _hoverTileY;

                    _hoverTileX = hoverTileX;
                    _hoverTileY = hoverTileY;

                    var posX = _hoverTileX * _tileSize;
                    var posY = _hoverTileY * _tileSize;

                    _hoverBox[0] = _hoverBox[4] = new(posX, posY);
                    _hoverBox[1] = new(posX + _tileSize, posY);
                    _hoverBox[2] = new(posX + _tileSize, posY + _tileSize);
                    _hoverBox[3] = new(posX, posY + _tileSize);

                    //if (_isPainting && isTileChange)
                    //WriteSelection();
                }
            }

            _lastPos = motion.Position;
        }

        if (@event is InputEventMouseButton mouse)
        {
            if (mouse.Pressed)
                switch (mouse.ButtonIndex)
                {
                    case MouseButton.Right:
                        if (_hoverTileX >= 0 && _hoverTileY >= 0)
                        {
                        }
                        break;

                    case MouseButton.Left:
                        if (_hoverTileX >= 0 && _hoverTileY >= 0)
                        {
                            var value = SelectedIndex = ((_hoverTileY << 4) + _hoverTileX) & 0x1FF;
                            var set = ControlTest.TilesetCurrent;
                            var ix = TilesetEditor.Instance.SelectedIndex;


                            //Modify tileset
                            var six = ix << 1;
                            var sample = set[six] | (set[six + 1] << 8);
                            sample &= 0xFE00;
                            sample |= value;
                            set[six] = (byte)sample;
                            set[six + 1] = (byte)(sample >> 8);




                            //Update tileset texture
                            //var bmp = ControlTest.TilesetBitmap;
                            //var gfx = ControlTest.GfxBitmap;

                            //var srcIx = ((value & 0x1F0) << 3 + 2) + ((value & 0xF) << 3);
                            //var dstX = ((ix >> 2 & 0x7) << 1) + (ix & 1);
                            //var dstY = (ix >> 5 << 1) + ((ix & 2) != 0 ? 1 : 0);
                            //var dstIx = ((dstY << (3 + 3 + 4)) + (dstX << 3)) << 2;

                            //var vMirror = (sample & 0x8000) != 0;
                            //var hMirror = (sample & 0x4000) != 0;

                            //int row = vMirror ? 8 : -1;
                            //Func<bool> checkRow = vMirror ? (() => row-- > 0) : (() => ++row < 8);
                            //while (checkRow())
                            //{
                            //    var srcPos = srcIx + (row << 3 + 3);
                            //    srcPos <<= 2;

                            //    int col = hMirror ? 8 : -1;
                            //    Func<bool> checkCol = hMirror ? (() => col-- > 0) : (() => ++col < 8);
                            //    while (checkCol())
                            //    {
                            //        var zOffset = srcPos + (col << 2);
                            //        bmp[dstIx++] = gfx[zOffset++];
                            //        bmp[dstIx++] = gfx[zOffset++];
                            //        bmp[dstIx++] = gfx[zOffset++];
                            //        bmp[dstIx++] = gfx[zOffset];
                            //    }

                            //    dstIx += (8 * 16 - 8) * 4;
                            //}

                            //ControlTest.TilesetImage.SetData(128, 512, false, Image.Format.Rgba8, bmp);

                            //var srcIx = ((value & 0x1F0) << 3 + 2) + ((value & 0xF) << 3);
                            var dstX = ((ix >> 2 & 0x7) << 1) + (ix & 1);
                            var dstY = (ix >> 5 << 1) + ((ix & 2) != 0 ? 1 : 0);
                            //var dstIx = ((dstY << (3 + 3 + 4)) + (dstX << 3)) << 2;

                            ControlTest.TilesetImage.BlitRect(ControlTest.GfxImage,
                                new((value & 0xF) << 3, (value & 0x1F0) >> 1, 8, 8),
                                new(dstX << 3, dstY << 3));

                            ControlTest.TilesetTexture.Update(ControlTest.TilesetImage);
                        }
                        break;

                    case MouseButton.Middle:
                        _isDragging = true;
                        break;

                    case MouseButton.WheelUp:
                        Size *= 1.25f;
                        //_drawPos *= 1.25f;
                        QueueRedraw();
                        break;

                    case MouseButton.WheelDown:
                        Size *= 0.75f;
                        //_drawPos *= 0.75f;
                        QueueRedraw();
                        break;
                }
            else
                switch (mouse.ButtonIndex)
                {
                    case MouseButton.Left:
                        //_isPainting = false;
                        break;
                    case MouseButton.Right:
                        break;
                    case MouseButton.Middle:
                        _isDragging = false;
                        break;
                }
        }

        base._Input(@event);
    }

    public void Reset()
    {
        Size = new(16 * 48, 32 * 48);
        Position = _drawPos = new();

        QueueRedraw();
    }

}
