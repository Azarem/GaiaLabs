
using Godot;

public partial class OffsetSelector : CheckButton
{

    public OffsetSelector()
    {
        Pressed += OffsetSelector_Pressed;
    }

    private void OffsetSelector_Pressed()
    {
        ControlTest.UseOffset = ButtonPressed;
        ControlTest.LoadScene(ControlTest.CurrentScene);
    }
}
