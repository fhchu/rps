using Godot;
using System;

public partial class main_menu : MarginContainer
{
    //total number of rows in the menu. need to update this whenever a new option is added
    private int menuRowCount = 1;
    private int startRow = 0;
    private int exitRow = 2;
    private Label selector1;
    private Label selector2;
    private Label selector3;
    // Called when the node enters the scene tree for the first time.
    private int cursorLocation;

    public override void _Ready()
    {
        selector1 = GetNode<Label>("CenterContainer/VBoxContainer/SelectorsCenter/SelectorsVBox/StartHBox/SelectorLabel");
        selector2 = GetNode<Label>("CenterContainer/VBoxContainer/SelectorsCenter/SelectorsVBox/OptionsHBox/SelectorLabel");
        selector3 = GetNode<Label>("CenterContainer/VBoxContainer/SelectorsCenter/SelectorsVBox/ExitHBox/SelectorLabel");
        cursorLocation = 0;
        setSelection(cursorLocation);
    }

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

    private void setSelection(int updatedCursor)
    {
        var selectorSymbol = ">";
        var blankSymbol = "";
        selector1.Text = updatedCursor == 0 ? selectorSymbol : blankSymbol;
        selector2.Text = updatedCursor == 1 ? selectorSymbol : blankSymbol;
        selector3.Text = updatedCursor == 2 ? selectorSymbol : blankSymbol;

    }

    private void handleSelection(int updatedCursor)
    {
        if (updatedCursor == startRow)
        {
            PackedScene fightScene = (PackedScene)ResourceLoader.Load("res://scenes/screens/fight.tscn");
            GetParent().AddChild(fightScene.Instantiate());
            QueueFree();
        }
        else if (updatedCursor == exitRow)
        {
            GetTree().Quit();
        }
    }
}
