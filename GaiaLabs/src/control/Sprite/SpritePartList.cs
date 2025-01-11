using GaiaLabs.src.control.Sprite;
using GaiaLib.Sprites;
using Godot;


public partial class SpritePartList : VBoxContainer
{
    public static SpritePartList Instance { get; private set; }

    private int _selectedIndex = -1;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var btn = _selectedIndex >= 0 ? GetChild(_selectedIndex) as Button : null;
            if (btn != null)
                btn.ButtonPressed = false;

            _selectedIndex = value;
            if (value < 0)
                return;

            btn = value >= 0 ? GetChild(value) as Button : null;
            if (btn != null)
                btn.ButtonPressed = true;
        }
    }

    private SpriteGroup _currentGroup;

    public SpriteGroup CurrentGroup
    {
        get => _currentGroup;
        set
        {
            foreach (var c in GetChildren())
            {
                RemoveChild(c);
                c.QueueFree();
            }

            _currentGroup = value;
            if (value == null)
                return;

            for (int ix = 0, len = value.Parts.Count; ix < len; ix++)
                CreateButton(ix);
        }
    }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    private void PartButtonPressed(Button btn, SpritePart part)
    {
        SelectedIndex = btn.GetIndex();
        SpritePropertyPanel.Instance.SelectedObject = part;
    }

    private void CreateButton(int ix)
    {
        var p = _currentGroup.Parts[ix];

        var btn = new Button() { Text = $"(x){p.XOffset:X2} (y){p.YOffset:X2} (t){p.TileIndex:X}", ToggleMode = true };

        btn.Pressed += new(() => { PartButtonPressed(btn, p); });

        AddChild(btn);
    }

    public void AddPart()
    {
        if (_currentGroup == null)
            return;

        SpritePart part;

        if (_selectedIndex >= 0 && _selectedIndex < _currentGroup.Parts.Count)
        {
            part = _currentGroup.Parts[_selectedIndex];
            part = new()
            {
                XOffset = part.XOffset,
                YOffset = part.YOffset,
                XOffsetMirror = part.XOffsetMirror,
                YOffsetMirror = part.YOffsetMirror,
                HMirror = part.HMirror,
                IsLarge = part.IsLarge,
                PaletteIndex = part.PaletteIndex,
                SomeOffset = part.SomeOffset,
                TileIndex = part.TileIndex,
                VMirror = part.VMirror
            };
        }
        else
            part = new();

        _currentGroup.Parts.Add(part);
        CreateButton(_currentGroup.Parts.Count - 1);
    }

    public void RemovePart()
    {
        if (_currentGroup == null || _selectedIndex < 0)
            return;

        var btn = GetChild(_selectedIndex);
        RemoveChild(btn);
        btn.QueueFree();

        _currentGroup.Parts.RemoveAt(_selectedIndex);

        _selectedIndex = -1;
    }



    public void Reset()
    {
        CurrentGroup = null;
    }
}

