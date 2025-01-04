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
    }
}
