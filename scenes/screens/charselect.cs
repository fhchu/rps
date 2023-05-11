using Godot;
using System;

public partial class charselect : ColorRect
{
    private int cursorLocation;
    public int setSelection(int okay)
    {
        return 1;
    }
    public int menuRowCount;
    public void handleSelection(int okay)
    {
        return;
    }

    private int p1Cursor;
    private int p2Cursor;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        p1Cursor = 0;
        p2Cursor = 0;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_down") && cursorLocation < menuRowCount - 1)
        {
            cursorLocation++;
            setSelection(cursorLocation);
        }

        else if (Input.IsActionJustPressed("ui_up") && cursorLocation > 0)
        {
            cursorLocation--;
            setSelection(cursorLocation);
        }

        else if (Input.IsActionJustPressed("ui_accept"))
        {
            handleSelection(cursorLocation);
        };
    }
}
