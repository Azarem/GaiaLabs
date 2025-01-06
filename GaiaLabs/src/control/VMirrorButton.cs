using Godot;

public partial class VMirrorButton : CheckButton
{
    public static VMirrorButton Instance { get; private set; }

    public VMirrorButton()
    {
        Toggled += VMirrorButton_Toggled;
    }


    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    private void VMirrorButton_Toggled(bool toggledOn)
    {
        var set = ControlTest.TilesetCurrent;
        var ix = TilesetEditor.Instance.SelectedIndex;

        //Modify tileset
        var six = ix << 1;
        var sample = set[six] | (set[six + 1] << 8);
        sample &= 0x7FFF;
        sample |= toggledOn ? 0x8000 : 0;
        set[six] = (byte)sample;
        set[six + 1] = (byte)(sample >> 8);
    }
}

