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

    private Player[] players;
    private List<RoundResult> roundResults;
    // this is an array because we might want more rounds in the future
    private int[] phaseHealth = { 20 };
    private int totalPhases = 1;
    private int currentPhase;
    private bool combatIsOn;

    Panel gameLog;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        gameLog = (Panel)GetNode("GameLog");
        gameLog.Hide();

        // we might want to have multiple phases one day
        currentPhase = 0;
        setUpPlayers();
        roundResults = new List<RoundResult>();
        startPhase(phaseHealth[currentPhase]);
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

    // build players before game starts.
    // we set HP 
    private void setUpPlayers()
    {
        players = new Player[NUM_PLAYERS];
        for (int i = 0; i < NUM_PLAYERS; i++)
        {
            Player player = new Player(PLAYER_NODE_PATH + i);

            players[i] = player;
            // player does not handle setting of their own powers
            setAllHands(player, 1, 1, 1);
            // perhaps we can have different upgrade thresholds per player? 
            player.upgradeThresholds = new int[] { 3, 10 };
        }
    }

    // sets up fighting once upgrades have been taken
    private void startPhase(int phaseHealth)
    {
        currentPhase += 1;
        for (int i = 0; i < NUM_PLAYERS; i++)
        {
            Player player = players[i];
            player.maxHealth = phaseHealth;

            String healthBarControl = player.VBoxPath + "/HealthBar";
            ProgressBar healthBar = (ProgressBar)GetNode(healthBarControl);
            healthBar.MaxValue = phaseHealth;
            updateHealth(player, phaseHealth);
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
        if (updateHealth(loser, loser.currentHealth - winner.baseHandValues[(int)winner.thrownHand]))
        {

        }
        winner.calculateHandValues();
        loser.calculateHandValues();
    }

    private void endPhase(Player loser)
    {
        if (currentPhase == totalPhases)
        {
            //TODO move this to "end game" function if necessary
            gameLogDisplay("Game Over!");
            TextureRect loserPic = (TextureRect)GetNode(loser.VBoxPath).GetNode("TextureRect");
            loserPic.FlipV = true;
            combatIsOn = false;
        }
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
            endPhase(player);
        }

        return false;
    }

    //updates the player's hand power in backend and UI
    private void setBaseHandPower(Player player, Hand hand, int value)
    {
        String handName = hand.ToString();
        player.baseHandValues[(int)hand] = value;
        Button handButton = (Button)GetNode(player.VBoxPath + HANDS_UI_PATH + "/" + handName + "Button");
        handButton.Text = handName + ": " + value;
    }


    //update all 3 hands at once
    private void setAllHands(Player player, int rockValue, int paperValue, int scissorsValue)
    {
        setBaseHandPower(player, Hand.Rock, rockValue);
        setBaseHandPower(player, Hand.Paper, paperValue);
        setBaseHandPower(player, Hand.Scissors, scissorsValue);
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
        //these are white (unconditional) hand values
        public int[] baseHandValues;
        public int[] upgradeThresholds;
        public List<PowerUp> powerUps;
        public Hand thrownHand;
        public bool hasThrown;
        public Player(String path)
        {
            VBoxPath = path;
            baseHandValues = new int[3];
            hasThrown = false;
            powerUps = new List<PowerUp>();
        }

        public void calculateHandValues()
        {
            int[] handModifiers = new int[3];
            foreach (PowerUp power in powerUps)
            {
                power.calculateDamage();
            }

        }
    }

    // stores a single round of "combat". a list of these is one game
    public class RoundResult
    {
        //in the future if necessary, we can calculate and store wins/ties instead of just hand history
        public Hand[] playerHands;
        //we're saving damage for the UI even if it's not used for mechanics
        // this is an array because each player can take different amounts of damage in one round
        public int[] damage;
    }
    public class PowerUp
    {
        public int[] calculateDamage()
        {
            return new int[3];
        }
    }
}
