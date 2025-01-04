using Godot;

public partial class TilesetEditor : Control
{
    public static TilesetEditor Instance { get; private set; }

    private int _hoverTileX, _hoverTileY;
    private Vector2[] _hoverBox = new Vector2[5];
    private Vector2[] _gridLines = new Vector2[8 * 32 * 2 + 4];
    private Vector2[] _selectionBox = new Vector2[5];
    //private Vector2 _drawSize;
    private float _tileSize, _halfSize;
    private float _zoom = 2.0f;
    private bool _isDragging = false;
    private Vector2 _lastPos;
    private Vector2 _drawPos;

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;
            var set = ControlTest.TilesetCurrent;
            var ix = value << 1;

            var tileData = set[ix++] | (set[ix] << 8);

            var tile = tileData & 0x01FF;
            var flag = (tileData & 0x0200) != 0;
            var priority = (tileData & 0x2000) != 0;
            var hMirror = (tileData & 0x4000) != 0;
            var vMirror = (tileData & 0x8000) != 0;
            var pOffset = (tileData >> 10) & 0x7;

            VMirrorButton.Instance.ButtonPressed = vMirror;
            HMirrorButton.Instance.ButtonPressed = hMirror;
            PriorityButton.Instance.ButtonPressed = priority;
            BlockButton.Instance.ButtonPressed = priority;
            GfxSelector.Instance.SelectedIndex = tile;
            PaletteSelector.Instance.SelectedIndex = pOffset;
            ControlTest.ReloadGraphicSet(pOffset);

            QueueRedraw();
        }
    }

    public TilesetEditor()
    {
        FocusMode = FocusModeEnum.Click;
    }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    public override void _Draw()
    {
        var size = Size;
        var left = _drawPos.X;
        var right = left + size.X;
        var top = _drawPos.Y;
        var bottom = top + size.Y;

        //_drawSize = GetDrawSize();
        _tileSize = size.X / 8;
        _halfSize = _tileSize / 2;
        DrawTextureRect(ControlTest.TilesetTexture, new(_drawPos, size), false);


        //Minor lines
        var c = 0;
        var pos = top + _halfSize;
        for (int ix = 0; ix < 32; ix++, pos += _tileSize)
        {
            _gridLines[c++] = new(left, pos);
            _gridLines[c++] = new(right, pos);
        }
        pos = left + _halfSize;
        for (int ix = 0; ix < 8; ix++, pos += _tileSize)
        {
            _gridLines[c++] = new(pos, top);
            _gridLines[c++] = new(pos, bottom);
        }
        _gridLines[c++] = new();
        _gridLines[c++] = new();
        _gridLines[c++] = new();
        _gridLines[c++] = new();
        DrawMultiline(_gridLines, Color.Color8(100, 120, 160, 180));

        //Major lines
        c = 0;
        pos = top;
        for (int y = 0; y < 33; y++, pos += _tileSize)
        {
            _gridLines[c++] = new(left, pos);
            _gridLines[c++] = new(right, pos);
        }

        pos = left;
        for (int x = 0; x < 9; x++, pos += _tileSize)
        {
            _gridLines[c++] = new(pos, top);
            _gridLines[c++] = new(pos, bottom);
        }

        DrawMultiline(_gridLines, Color.Color8(200, 200, 200, 180));

        //Selection outline
        var posX = ((_selectedIndex >> 2) & 0x7) * _tileSize + _drawPos.X;
        var posY = (_selectedIndex >> 5) * _tileSize + _drawPos.Y;
        posX += (_selectedIndex & 1) != 0 ? _halfSize : 0;
        posY += (_selectedIndex & 2) != 0 ? _halfSize : 0;
        _selectionBox[0] = _selectionBox[4] = new(posX, posY);
        _selectionBox[1] = new(posX + _halfSize, posY);
        _selectionBox[2] = new(posX + _halfSize, posY + _halfSize);
        _selectionBox[3] = new(posX, posY + _halfSize);
        DrawPolyline(_selectionBox, Color.Color8(255, 0, 255, 255));

        //var newBox = new[] {
        //    _hoverBox[0] + _drawPos,
        //    _hoverBox[1] + _drawPos,
        //    _hoverBox[2] + _drawPos,
        //    _hoverBox[3] + _drawPos,
        //    _hoverBox[4] + _drawPos
        //};
        //DrawPolyline(newBox, Color.Color8(0, 255, 255));
    }

    private int GetCurrentTileIndex()//ushort? newValue = null)
    {
        var six = (_hoverTileY >> 1 << 3) + (_hoverTileX >> 1);
        var quad = ((_hoverTileY & 1) << 1) | (_hoverTileX & 1);

        var ix = (six << 2) + quad;

        return ix;
        //var mOffset = (_hoverTileY >> 4) * ControlTest.TilemapWidth + (_hoverTileX >> 4);
        //var ix = (mOffset << 8) | ((_hoverTileY & 0x0F) << 4) | (_hoverTileX & 0x0F);

        //var old = ControlTest.TilemapCurrent[ix];

        //if (newValue != null)
        //    ControlTest.TilemapCurrent[ix] = newValue.Value;

        //return old;
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
                    var hoverTileX = (int)(pos.X / _halfSize);
                    var hoverTileY = (int)(pos.Y / _halfSize);

                    var isTileChange = hoverTileX != _hoverTileX || hoverTileY != _hoverTileY;

                    _hoverTileX = hoverTileX;
                    _hoverTileY = hoverTileY;

                    var posX = _hoverTileX * _halfSize;
                    var posY = _hoverTileY * _halfSize;

                    _hoverBox[0] = _hoverBox[4] = new(posX, posY);
                    _hoverBox[1] = new(posX + _halfSize, posY);
                    _hoverBox[2] = new(posX + _halfSize, posY + _halfSize);
                    _hoverBox[3] = new(posX, posY + _halfSize);

                    //if (_isPainting && isTileChange)
                    //WriteSelection();
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
                        }
                        break;

                    case MouseButton.Left:
                        if (_hoverTileX >= 0 && _hoverTileY >= 0)
                        {
                            SelectedIndex = GetCurrentTileIndex();
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
        Size = new(8 * 80, 32 * 80);
        Position = _drawPos = new();

        QueueRedraw();
    }
}

