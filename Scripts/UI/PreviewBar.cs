using Godot;
using System;

public partial class PreviewBar : Control
{


    [Export]
    public ProgressBar FinalHealthBar;

    [Export]
    public ProgressBar PreviewHealthBar;





    public void SetMaxBars(int max)
    {
        FinalHealthBar.MaxValue = max;
        PreviewHealthBar.MaxValue = max;
    }

    public void SetBars(int finalVal, int previewVal)
    {
        FinalHealthBar.Value = finalVal;
        PreviewHealthBar.Value = previewVal;
    }


}
