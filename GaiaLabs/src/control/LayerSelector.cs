using Godot;

public partial class LayerSelector : CheckBox
{

    public LayerSelector()
    {
        this.Pressed += LayerSelector_Pressed;
    }

    private void LayerSelector_Pressed()
    {
        ControlTest.IsEffect = ButtonPressed;
        ControlTest.LoadScene(ControlTest.CurrentScene);
    }
}
