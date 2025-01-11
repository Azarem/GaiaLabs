using GaiaLabs.src.control.Sprite;
using GaiaLib.Sprites;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SpriteFrameList : VBoxContainer
{
    public static SpriteFrameList Instance;

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

    private List<SpriteFrame> _currentFrameset;
    public List<SpriteFrame> CurrentFrameset
    {
        get => _currentFrameset;
        set
        {
            foreach (var c in GetChildren())
            {
                RemoveChild(c);
                c.QueueFree();
            }

            _currentFrameset = value;
            if (value == null)
                return;

            for (int ix = 0, len = _currentFrameset.Count; ix < len; ix++)
                CreateButton(ix);
        }
    }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    private void CreateButton(int ix)
    {
        var frm = _currentFrameset[ix];
        var btn = new Button() { Text = $"{frm.Duration:X}:{frm.GroupIndex:X}", ToggleMode = true };
        btn.Pressed += new(() => { FrameButtonPressed(btn, frm); });
        AddChild(btn);
    }

    private void FrameButtonPressed(Button btn, SpriteFrame frm)
    {
        var gix = frm.GroupIndex;
        var grp = ControlTest.SpriteMap.Groups[gix];

        SelectedIndex = btn.GetIndex();
        SpriteGroupList.Instance.SelectedIndex = gix;
        SpritePartList.Instance.CurrentGroup = grp;
        SpritePropertyPanel.Instance.SelectedObject = new object[] { frm, grp };
        //SpriteGroupList.Instance.CurrentGroup = ControlTest.SpriteMap.Groups[set.GroupIndex];

    }

    public void AddFrame()
    {
        if (_currentFrameset == null)
            return;

        _currentFrameset.Add(new());
        CreateButton(_currentFrameset.Count - 1);
    }

    public void RemoveFrame()
    {
        if (_currentFrameset == null || _selectedIndex < 0) 
            return;

        var btn = GetChild(_selectedIndex);
        RemoveChild(btn);
        btn.QueueFree();

        _currentFrameset.RemoveAt(_selectedIndex);
        _selectedIndex = -1;
    }


    public void Reset()
    {
        CurrentFrameset = null;
    }
}
