using Godot;
using System;

public partial class fight : Control
{
    //Allows us to access and update UI elements from this file
    const String PLAYER_NODE_PATH = "LocalHBox/PlayerVBox";
    const String ATTACKS_UI_PATH = "/AttacksPanel";
    const int NUM_PLAYERS = 2;

    Player[] players;
    int[] roundHealth = { 3, 7, 11 };
    int currentRound;

    Signal Player1Attack;
    Control GameLog;
    Control AttacksPanel;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GameLog = (Panel)GetNode("GameLog");
        GameLog.Hide();

        currentRound = 0;
        setUpPlayers();
        startRound(roundHealth[currentRound]);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("ui_right"))
        {
            // Move right.
        }
    }

    private void setUpPlayers()
    {
        players = new Player[NUM_PLAYERS];
        //TODO 2nd player
        for (int i = 0; i < 1; i++)
        {
            players[i] = new Player(PLAYER_NODE_PATH + i);
        }
    }

    private void startRound(int roundHealth)
    {
        //TODO 2nd player
        for (int i = 0; i < 1; i++)
        {
            Player player = players[i];
            Control AttacksPanel = (Panel)GetNode(player.VBoxPath + ATTACKS_UI_PATH);
            AttacksPanel.Hide();


            updateHealth(player, roundHealth);
        }
    }
    private void updateHealth(Player player, int health)
    {
        String healthBarControl = player.VBoxPath + "/HealthBar";
        ProgressBar healthBar = (ProgressBar)GetNode(healthBarControl);
        healthBar.Value = health;
        Label healthLabel = (Label)healthBar.GetChild(0);
        healthLabel.Text = player.currentHealth + "/ " + player.MaxHealth;
    }

    public class Player
    {
        public String VBoxPath;
        public int MaxHealth;
        public int currentHealth;
        public int rockPower;
        public int paperPower;
        public int scissorsPower;
        public int[] movesHistory;
        public PowerUp[] powerUps;

        public Player(String path)
        {
            VBoxPath = path;
        }
    }

    public class PowerUp
    {
        void process(Player player)
        {

        }
    }
}
