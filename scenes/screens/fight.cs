using Godot;
using System;
using System.Collections.Generic;

public partial class fight : Control
{
    //Allows us to access and update UI elements from this file
    const String PLAYER_NODE_PATH = "LocalHBox/PlayerVBox";
    const String GAMELOG_PATH = "CanvasLayer/TextboxMargin/GameLog";
    const String UPGRADES_PATH = "CanvasLayer/UpgradesHBox";

    const String HANDS_UI_PATH = "/AttacksPanel/HandsHBox";
    const int NUM_PLAYERS = 2;
    public enum Hand { Rock, Paper, Scissors }

    private Player[] players;
    private List<RoundResult> roundResults;
    // this is an array because we might want more phases in the future
    private int[] phaseHealth = { 20 };
    private int totalPhases = 1;
    private int currentPhase;
    private bool isCombatEnabled;
    private bool isUpgradePhase;
    private int upgradingPlayer;

    Panel gameLog;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        gameLog = (Panel)GetNode(GAMELOG_PATH);
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
        if (isCombatEnabled)
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
        else if (isUpgradePhase)
        {
            if (upgradingPlayer == 0)
            {
                if (Input.IsActionJustPressed("rock0"))
                {
                    GD.Print("1");
                }
                if (Input.IsActionJustPressed("paper0"))
                {
                    GD.Print("2");
                }
                if (Input.IsActionJustPressed("scissors0"))
                {
                    GD.Print("3");
                }
            }
            else
            {
                if (Input.IsActionJustPressed("rock1"))
                {
                    GD.Print("1");
                }
                if (Input.IsActionJustPressed("paper1"))
                {
                    GD.Print("2");
                }
                if (Input.IsActionJustPressed("scissors1"))
                {
                    GD.Print("3");
                }
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
            Player player = new Player(PLAYER_NODE_PATH + i, i);

            players[i] = player;
            // player does not handle setting of their own powers
            setAllHands(player, 1, 1, 1);
            // perhaps we can have different upgrade thresholds per player? 
            //player thresholds must be added in ascending order and cannot be higher than maxHealth (or we're buggin out!!)
            player.upgradeThresholds.Add(3);
            player.upgradeThresholds.Add(10);

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
        isCombatEnabled = true;
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
        roundResults.Add(new RoundResult());
        winner.calculateHandValues();
        loser.calculateHandValues();
    }

    private void selectUpgrade(Player player, int selection)
    {
        Control upgrades = (Control)GetNode(UPGRADES_PATH);
        isUpgradePhase = false;
        isCombatEnabled = true;

        upgrades.Hide();
    }

    private void endPhase(Player loser)
    {
        if (currentPhase == totalPhases)
        {
            //TODO move this to "end game" function if necessary
            gameLogDisplay("Game Over!");
            TextureRect loserPic = (TextureRect)GetNode(loser.VBoxPath).GetNode("TextureRect");
            loserPic.FlipV = true;
            isCombatEnabled = false;
        }
    }

    //presumably the player knows their own max health. we are simply setting the health to this value.
    // returns true or false based on ?? TODO
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
        else if (player.upgradeThresholds.Count > 0 && player.currentHealth <= player.maxHealth - player.upgradeThresholds[0])
        {
            startUpgradePhase(player);
        }

        return false;
    }


    private void startUpgradePhase(Player player)
    {
        isUpgradePhase = true;
        isCombatEnabled = false;
        upgradingPlayer = player.playerId;
        Control upgrades = (Control)GetNode(UPGRADES_PATH);
        for (int i = 0; i < 3; i++)
        {
            Control marginContainer = (Control)upgrades.GetNode("MarginContainer" + i + "/Panel/VBoxContainer");

        }
        upgrades.Show();
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
        public int playerId;
        public String VBoxPath;
        public int maxHealth;
        public int currentHealth;
        //these are white (unconditional) hand values
        // rock = 0, paper = 1, scissors = 2
        public int[] baseHandValues;
        public PowerUp[] powerUpLibrary;
        public List<int> upgradeThresholds;
        public List<PowerUp> powerUpsObtained;
        public Hand thrownHand;
        public bool hasThrown;
        public Player(String path, int id)
        {
            playerId = id;
            VBoxPath = path;
            baseHandValues = new int[3];
            hasThrown = false;
            upgradeThresholds = new List<int>();
            powerUpsObtained = new List<PowerUp>();
        }

        public void calculateHandValues()
        {
            int[] handModifiers = new int[3];
            foreach (PowerUp power in powerUpsObtained)
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

        RoundResult(Hand player0Hand, Hand player1Hand, )
    }

    public abstract class PowerUp
    {
        public String title;
        public String description;
        public abstract int[] calculateDamage(Player player);
    }

    public class BigRock : PowerUp
    {
        BigRock()
        {
            title = "Big Rock";
            description = "Every third Rock you throw breaks ties";
        }
        public override int[] calculateDamage(Player player)
        {
            if ()

        }
    }
}
