using Godot;

public partial class LayerSelector : CheckBox
{

    public LayerSelector()
    {
        this.Pressed += LayerSelector_Pressed;
    }

    private void LayerSelector_Pressed()
    {
        ControlTest.LoadScene(ControlTest.CurrentScene, ButtonPressed);
    }
}
