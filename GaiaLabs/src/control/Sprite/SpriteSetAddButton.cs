using Godot;


public partial class SpriteSetAddButton : Button
{
    public SpriteSetAddButton()
    {
        Pressed += SpriteSetAddButton_Pressed;
    }

    private void SpriteSetAddButton_Pressed()
    {
        SpriteSetList.Instance.AddSet();
    }
}

