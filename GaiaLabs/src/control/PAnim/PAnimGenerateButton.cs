using Godot;
using System;
using System.Text;

public partial class PAnimGenerateButton : Button
{
    public PAnimGenerateButton()
    {
        Pressed += PAnimGenerateButton_Pressed;
    }

    private void PAnimGenerateButton_Pressed()
    {
        if (!int.TryParse(PAnimMidpoint.Instance.Text, out var midpoint))
            return;

        if (!int.TryParse(PAnimWeight.Instance.Text, out var weight))
            return;

        if (!int.TryParse(PAnimTime.Instance.Text, out var frames) || frames <= 0)
            return;

        var palData = ControlTest.PaletteData;
        var srcColors = new Vector3[15];
        var dstColors = new Vector3[15];
        //var ctrColors = new Vector3[15];

        var palIx = PAnimSelector.Instance.SelectedIndex;
        for (int i = 0, p = (palIx << 6) + 4; i < 15; i++, p++)
            dstColors[i] = new Vector3(palData[p++], palData[p++], palData[p++]);

        float step = 1f / frames;

        StringBuilder builder = new();

        Vector2 p0 = new(0, 0);
        Vector2 p1 = new(0, weight / 100f);
        Vector2 p2 = new(1, 1);

        string prefix = DateTime.Now.ToString("MMddmmss");
        builder.AppendLine($"panim{prefix} [");

        for (int i = 0; i < frames; i++)
        {
            builder.AppendLine($"  palette_bundle < #01, &panim{prefix}_frame{i:X2}, #{(palIx << 4) + 1:X2}, #1D, #00 >");
        }

        builder.AppendLine("]");
        builder.AppendLine();

        float time = step;
        for (int i = 0; i < frames; i++, time += step)
        {
            Vector2 q0 = p0.Lerp(p1, time);
            Vector2 q1 = p1.Lerp(p2, time);
            Vector2 r0 = q0.Lerp(q1, time);

            var distance = r0.Y;

            builder.AppendLine($"panim{prefix}_frame{i:X2} [");
            for (int c = 0; c < 15; c++)
            {
                var color = srcColors[c].Lerp(dstColors[c], distance);
                int r = (int)Math.Round(color.X, 0, MidpointRounding.AwayFromZero);
                int g = (int)Math.Round(color.Y, 0, MidpointRounding.AwayFromZero);
                int b = (int)Math.Round(color.Z, 0, MidpointRounding.AwayFromZero);
                var pixel = ((b & 0xF8) << 7) | ((g & 0xF8) << 2) | ((r >> 3) & 0x1F);
                builder.AppendLine($"  #${pixel:X4}");
            }
            builder.AppendLine("]");
            builder.AppendLine();
        }


        PAnimOutput.Instance.Text = builder.ToString();

    }
}

