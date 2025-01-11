using GaiaLabs.src.control.Sprite;
using GaiaLib.Sprites;
using Godot;

public partial class SpriteGroupList : VBoxContainer
{
    public static SpriteGroupList Instance { get; private set; }


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


    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    public void AddGroup()
    {
        var grps = ControlTest.SpriteMap?.Groups;
        if (grps == null) return;

        grps.Add(new());
        CreateButton(grps.Count - 1);
    }

    public void RemoveGroup()
    {
        if (_selectedIndex < 0)
            return;

        var btn = GetChild(_selectedIndex);
        RemoveChild(btn);
        btn.QueueFree();

        ControlTest.SpriteMap.Groups.RemoveAt(_selectedIndex);

        _selectedIndex = -1;
        SpritePartList.Instance.CurrentGroup = null;
    }

    private void CreateButton(int ix)
    {
        var grp = ControlTest.SpriteMap.Groups[ix];
        var btn = new Button() { Text = $"Group{ix:X2}", ToggleMode = true };

        var bix = ix;
        btn.Pressed += new(() => { GroupButtonPressed(btn, grp); });

        AddChild(btn);

    }

    private void GroupButtonPressed(Button btn, SpriteGroup grp)
    {
        SelectedIndex = btn.GetIndex();
        SpriteSetList.Instance.SelectedIndex = -1;
        SpriteFrameList.Instance.CurrentFrameset = null;
        SpritePropertyPanel.Instance.SelectedObject = grp;
        SpritePartList.Instance.CurrentGroup = grp;
    }


    public void Reset()
    {
        _selectedIndex = -1;

        foreach (var c in GetChildren())
        {
            RemoveChild(c);
            c.QueueFree();
        }

        var grps = ControlTest.SpriteMap?.Groups;
        if (grps == null)
            return;

        for (int ix = 0, len = grps.Count; ix < len; ix++)
            CreateButton(ix);
    }
}

