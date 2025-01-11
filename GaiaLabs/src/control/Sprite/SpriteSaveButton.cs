using Godot;


public partial class SpriteSaveButton : Button
{
    public SpriteSaveButton()
    {
        Pressed += SpriteSaveButton_Pressed;
    }

    private void SpriteSaveButton_Pressed()
    {
        ControlTest.SaveSprites();
    }
}

