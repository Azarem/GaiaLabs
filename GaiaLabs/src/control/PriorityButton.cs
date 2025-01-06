using Godot;

public partial class PriorityButton : CheckButton
{
    public static PriorityButton Instance { get; private set; }

    public PriorityButton()
    {
        this.Toggled += PriorityButton_Toggled;
    }

    private void PriorityButton_Toggled(bool toggledOn)
    {
        var set = ControlTest.TilesetCurrent;
        var ix = TilesetEditor.Instance.SelectedIndex;

        //Modify tileset
        var six = ix << 1;
        var sample = set[six] | (set[six + 1] << 8);
        sample &= 0xDFFF;
        sample |= toggledOn ? 0x2000 : 0;
        set[six] = (byte)sample;
        set[six + 1] = (byte)(sample >> 8);
    }

    public override void _EnterTree()
    {
        Instance = this;
        base._EnterTree();
    }
}
