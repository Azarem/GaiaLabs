using GaiaLabs.src.control.Sprite;
using GaiaLib.Sprites;
using Godot;
using System.Collections.Generic;


public partial class SpriteSetList : VBoxContainer
{
    public static SpriteSetList Instance;

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

    private Control _contextMenu;
    private Window _window;

    public SpriteSetList()
    {
    }

    public override void _Ready()
    {
        base._Ready();

        _window = new Window() { Owner = GetWindow() };


        _contextMenu = new VBoxContainer() { };
        //_contextMenu.GuiInput += _contextMenu_GuiInput;
        var newBtn = new MenuButton() { Text = "New" };
        var delBtn = new MenuButton() { Text = "Delete" };

        _contextMenu.AddChild(newBtn);
        _contextMenu.AddChild(delBtn);

        _window.AddChild(_contextMenu);

        ControlTest.Instance.AddChild(_window);

        //GetWindow().AddChild(_window);
        //GetParent().GetParent().AddChild(_contextMenu);
        //AddChild(_contextMenu);
    }

    //private void _contextMenu_GuiInput(InputEvent @event)
    //{
    //    if (@event is InputEventMouse mouse)
    //    {
    //        var pos = mouse.GlobalPosition - _contextMenu.GlobalPosition;
    //        if (pos.X < 0 || pos.Y < 0 || pos.X >= _contextMenu.Size.X || pos.Y >= _contextMenu.Size.Y)
    //        {
    //            Visible = false;
    //            return;
    //        }
    //    }
    //}

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouse mouse)
        {
            var parent = GetParent() as Control;
            var pos = mouse.GlobalPosition - parent.GlobalPosition;
            if (pos.X < 0 || pos.Y < 0 || pos.X >= parent.Size.X || pos.Y >= parent.Size.Y)
                return;

            if (mouse is InputEventMouseButton button)
            {
                if (button.ButtonIndex == MouseButton.Right)
                {
                    DrawRect(new(pos, new(200, 100)), new Color(0, 0, 0), true);
                    //var wnd = TilesetWindow.Instance;
                    //wnd.Position = new((int)mouse.GlobalPosition.X, (int)mouse.GlobalPosition.Y);
                    //wnd.Show();
                    //_contextMenu.Popup();
                    //_contextMenu.Position = mouse.GlobalPosition;
                    // _contextMenu.Visible = true;
                }
            }
        }
    }

    public void Reset()
    {
        _selectedIndex = -1;
        foreach (var c in GetChildren())
        {
            RemoveChild(c);
            c.QueueFree();
        }

        var sets = ControlTest.SpriteMap?.FrameSets;
        if (sets == null)
            return;

        for (int ix = 0, len = sets.Count; ix < len; ix++)
            CreateButton(ix);
    }

    private void SetButtonPressed(Button btn, List<SpriteFrame> set)
    {
        var bix = btn.GetIndex();
        SelectedIndex = bix;
        SpriteFrameList.Instance.CurrentFrameset = set;
        SpritePartList.Instance.CurrentGroup = null;
        SpriteGroupList.Instance.SelectedIndex = -1;
        SpritePropertyPanel.Instance.SelectedObject = null;
    }

    private void CreateButton(int ix)
    {
        var set = ControlTest.SpriteMap.FrameSets[ix];
        var btn = new Button() { Text = $"Set{ix:X2} ({set.Count:X})", ToggleMode = true };

        btn.Pressed += new(() => { SetButtonPressed(btn, set); });
        AddChild(btn);
    }


    public void AddSet()
    {
        var sets = ControlTest.SpriteMap?.FrameSets;
        if (sets == null) return;

        sets.Add(new());
        CreateButton(sets.Count - 1);
    }


    public void RemoveSet()
    {
        if (_selectedIndex < 0)
            return;

        var node = GetChild(_selectedIndex);
        RemoveChild(node);
        node.QueueFree();

        ControlTest.SpriteMap.FrameSets.RemoveAt(_selectedIndex);

        _selectedIndex = -1;
    }
}

