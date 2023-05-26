using Godot;
using System;

public partial class charselect : ColorRect
{
    Label p1Label;
    Label p2Label;
    Control p1VBox;
    Control p2VBox;
    Container playerSelect;
    static int X = 0;
    static int Y = 1;
    static int PLAYER_1 = 0;
    static int PLAYER_2 = 1;
    static int ROW_COUNT = 2;
    static int COLUMN_COUNT = 1;
    enum Direction { UP, DOWN, LEFT, RIGHT };
    int p1Cursor;
    int p2Cursor;

    bool hasP1Selected;
    bool hasP2Selected;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        p1Label = GetNode<Label>("MarginContainer/HBoxContainer/MarginContainer/Panel/VBoxContainer/CenterContainer/GridContainer/Panel0/Player1Label");
        p2Label = GetNode<Label>("MarginContainer/HBoxContainer/MarginContainer/Panel/VBoxContainer/CenterContainer/GridContainer/Panel0/Player2Label");
        p1VBox = GetNode<Control>("MarginContainer/HBoxContainer/Player1VBox");
        p2VBox = GetNode<Control>("MarginContainer/HBoxContainer/Player2VBox");

        playerSelect = GetNode<Container>("MarginContainer/HBoxContainer/MarginContainer/Panel/VBoxContainer/CenterContainer/GridContainer");
        p1Cursor = 0;
        p2Cursor = 0;
        hasP1Selected = false;
        hasP2Selected = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_up_0"))
        {
            moveCursor(PLAYER_1, Direction.UP);
        }
        else if (Input.IsActionJustPressed("ui_down_0"))
        {
            moveCursor(PLAYER_1, Direction.DOWN);
        }
        else if (Input.IsActionJustPressed("ui_left_0"))
        {
            moveCursor(PLAYER_1, Direction.LEFT);
        }
        else if (Input.IsActionJustPressed("ui_right_0"))
        {
            moveCursor(PLAYER_1, Direction.RIGHT);
        }
        else if (Input.IsActionJustPressed("ui_accept_0"))
        {
            selectCharacter(PLAYER_1);
        }
        else if (Input.IsActionJustPressed("ui_up_1"))
        {
            moveCursor(PLAYER_2, Direction.UP);
        }
        else if (Input.IsActionJustPressed("ui_down_1"))
        {
            moveCursor(PLAYER_2, Direction.DOWN);
        }
        else if (Input.IsActionJustPressed("ui_left_1"))
        {
            moveCursor(PLAYER_2, Direction.LEFT);
        }
        else if (Input.IsActionJustPressed("ui_right_1"))
        {
            moveCursor(PLAYER_2, Direction.RIGHT);
        }
        else if (Input.IsActionJustPressed("ui_accept_1"))
        {
            selectCharacter(PLAYER_2);
        };
    }

    void moveCursor(int player, Direction direction)
    {
        Label label = player == PLAYER_1 ? p1Label : p2Label;

        if (direction == Direction.UP)
        {
            if (p1Cursor == 0)
            {
                p1Cursor = ROW_COUNT;
            }
            else
            {
                p1Cursor--;
            }
            label.Position = getPosition(p1Cursor);
        }
        if (direction == Direction.DOWN)
        {
            if (p1Cursor == ROW_COUNT)
            {
                p1Cursor = 0;
            }
            else
            {
                p1Cursor++;
            }
            label.Position = getPosition(p1Cursor);
        }


    }

    void selectCharacter(int player)
    {
        Control playerVbox;
        if (player == PLAYER_1)
        {
            hasP1Selected = !hasP1Selected;
            playerVbox = p1VBox;
        }
        else
        {
            hasP2Selected = !hasP2Selected;
            playerVbox = p2VBox;
        }

        if (hasP1Selected && hasP2Selected)
        {
            PackedScene fightScene = (PackedScene)ResourceLoader.Load("res://scenes/screens/fight.tscn");

        }
    }

    Vector2 getPosition(int location)
    {
        Vector2 position = playerSelect.GetChild<Control>(location).Position;
        GD.Print(position);

        return position;
    }
}
