using Godot;

public partial class BlockButton : CheckButton
{
    public static BlockButton Instance { get; private set; }

    public BlockButton()
    {
        this.Toggled += BlockButton_Toggled;
    }

    private void BlockButton_Toggled(bool toggledOn)
    {
        var set = ControlTest.TilesetCurrent;
        var ix = TilesetEditor.Instance.SelectedIndex;

        //Modify tileset
        var six = ix << 1;
        var sample = set[six] | (set[six + 1] << 8);
        sample &= 0xFDFF;
        sample |= toggledOn ? 0x0200 : 0;
        set[six] = (byte)sample;
        set[six + 1] = (byte)(sample >> 8);
    }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }
}
