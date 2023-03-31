using Godot;
using System;
using System.Collections.Generic;

public partial class fight : Control
{
    //Allows us to access and update UI elements from this file
    const String PLAYER_NODE_PATH = "LocalHBox/PlayerVBox";
    const String HANDS_UI_PATH = "/AttacksPanel/HandsHBox";
    const int NUM_PLAYERS = 2;
    public enum Hand { Rock, Paper, Scissors }

    Player[] players;
    int[] roundHealth = { 3, 7, 11 };
    int totalRounds;
    int currentRound;
    bool combatIsOn;

    Panel gameLog;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        gameLog = (Panel)GetNode("GameLog");
        //gameLog.Hide();

        currentRound = 0;
        totalRounds = 3;
        setUpPlayers();
        startRound(roundHealth[currentRound]);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (combatIsOn)
        {
            if (Input.IsActionJustPressed("rock0"))
            {
                players[0].thrownHand = Hand.Rock;
                players[0].hasThrown = true;
            }
            if (Input.IsActionJustPressed("paper0"))
            {
                players[0].thrownHand = Hand.Paper;
                players[0].hasThrown = true;
            }
            if (Input.IsActionJustPressed("scissors0"))
            {
                players[0].thrownHand = Hand.Scissors;
                players[0].hasThrown = true;
            }
            if (Input.IsActionJustPressed("rock1"))
            {
                players[1].thrownHand = Hand.Rock;
                players[1].hasThrown = true;
            }
            if (Input.IsActionJustPressed("paper1"))
            {
                players[1].thrownHand = Hand.Paper;
                players[1].hasThrown = true;
            }
            if (Input.IsActionJustPressed("scissors1"))
            {
                players[1].thrownHand = Hand.Scissors;
                players[1].hasThrown = true;
            }
            if (players[1].hasThrown && players[0].hasThrown)
            {
                throwHands();
            }
        }
    }

    // build players before round 0
    private void setUpPlayers()
    {
        players = new Player[NUM_PLAYERS];
        for (int i = 0; i < NUM_PLAYERS; i++)
        {
            Player player = new Player(PLAYER_NODE_PATH + i);

            players[i] = player;
            updateHands(player, 1, 1, 1);

        }
    }

    // sets up fighting once upgrades have been taken
    private void startRound(int roundHealth)
    {
        currentRound += 1;
        for (int i = 0; i < NUM_PLAYERS; i++)
        {
            Player player = players[i];
            player.maxHealth = roundHealth;

            String healthBarControl = player.VBoxPath + "/HealthBar";
            ProgressBar healthBar = (ProgressBar)GetNode(healthBarControl);
            healthBar.MaxValue = roundHealth;
            updateHealth(player, roundHealth);
        }
        waitForHands();
    }

    private void waitForHands()
    {
        combatIsOn = true;
    }
    private void throwHands()
    {
        Player player0 = players[0];
        Player player1 = players[1];
        player0.hasThrown = false;
        player1.hasThrown = false;
        Player winner;
        Player loser;
        //tie case
        if (player0.thrownHand == player1.thrownHand)
        {
            return;
        }
        else if ((player0.thrownHand == Hand.Rock && player1.thrownHand == Hand.Scissors) ||
            (player0.thrownHand == Hand.Paper && player1.thrownHand == Hand.Rock) ||
            (player0.thrownHand == Hand.Scissors && player1.thrownHand == Hand.Paper))
        {
            winner = player0;
            loser = player1;
        }

        else
        {
            winner = player1;
            loser = player0;
        }
        if (updateHealth(loser, loser.currentHealth - winner.handValues[(int)winner.thrownHand]))
        {

        }
        winner.calculateHandValues();
        loser.calculateHandValues();
    }

    private void endRound(Player loser)
    {
        // if (currentRound == totalRounds)
        //{
        //TODO move this to "end game" function if necessary
        gameLogDisplay("Game Over!");
        TextureRect loserPic = (TextureRect)GetNode(loser.VBoxPath).GetNode("TextureRect");
        loserPic.FlipV = true;
        combatIsOn = false;
        //}
    }


    //presumably the player knows their own max health. we are simply setting the health to this value.
    // returns 
    private bool updateHealth(Player player, int health)
    {
        String healthBarControl = player.VBoxPath + "/HealthBar";
        ProgressBar healthBar = (ProgressBar)GetNode(healthBarControl);
        player.currentHealth = health;
        healthBar.Value = health;
        Label healthLabel = (Label)healthBar.GetChild(0);
        healthLabel.Text = player.currentHealth + "/ " + player.maxHealth;

        if (player.currentHealth <= 0)
        {
            endRound(player);
        }
    }

    //updates the player's hand power
    private void updateHandPower(Player player, Hand hand, int value)
    {
        String handName = hand.ToString();
        player.handValues[(int)hand] = value;
        Button handButton = (Button)GetNode(player.VBoxPath + HANDS_UI_PATH + "/" + handName + "Button");
        handButton.Text = handName + ": " + value;
        //TODO if it's updated, change the color to green
    }

    //update all 3 hands at once
    private void updateHands(Player player, int rockValue, int paperValue, int scissorsValue)
    {
        updateHandPower(player, Hand.Rock, rockValue);
        updateHandPower(player, Hand.Paper, paperValue);
        updateHandPower(player, Hand.Scissors, scissorsValue);
    }

    private void gameLogDisplay(String labelText)
    {
        gameLog.Show();
        Label gameLogLabel = (Label)gameLog.GetNode("Label");
        gameLogLabel.Text = labelText;
    }
    // hold the player data. player object does NOT handle their own UI
    public class Player
    {
        public String VBoxPath;
        public int maxHealth;
        public int currentHealth;
        public int[] handValues;
        public List<Hand> handHistory;
        public PowerUp[] powerUps;
        public Hand thrownHand;
        public bool hasThrown;
        public Player(String path)
        {
            VBoxPath = path;
            handValues = new int[3];
            handHistory = new List<Hand>();
            hasThrown = false;
        }

        public void calculateHandValues()
        {

        }
    }


    public class PowerUp
    {
        void process(Player player)
        {

        }
    }
}
