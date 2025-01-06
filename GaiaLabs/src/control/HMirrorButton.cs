using Godot;

public partial class HMirrorButton : CheckButton
{
    public static HMirrorButton Instance { get; private set; }

    public HMirrorButton()
    {
        Toggled += HMirrorButton_Toggled;
    }


    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }

    private void HMirrorButton_Toggled(bool toggledOn)
    {
        var set = ControlTest.TilesetCurrent;
        var ix = TilesetEditor.Instance.SelectedIndex;

        //Modify tileset
        var six = ix << 1;
        var sample = set[six] | (set[six + 1] << 8);
        sample &= 0xBFFF;
        sample |= toggledOn ? 0x4000 : 0;
        set[six] = (byte)sample;
        set[six + 1] = (byte)(sample >> 8);
    }
}
