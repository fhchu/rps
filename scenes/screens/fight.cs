using Godot;
using System;
using System.Collections.Generic;

public partial class fight : Control
{
    const string BIGROCK_NAME = "Big Rock";
    //Allows us to access and update UI elements from this file
    const string PLAYER_NODE_PATH = "LocalHBox/PlayerVBox";
    const string GAMELOG_PATH = "CanvasLayer/TextboxMargin/GameLog";
    const string UPGRADES_PATH = "CanvasLayer/UpgradesHBox";

    const string HANDS_UI_PATH = "/AttacksPanel/HandsHBox";
    const int NUM_PLAYERS = 2;
    const int NUM_HANDS = 3;
    public enum Hand { ROCK, PAPER, SCISSORS }
    public enum Operation { ADD, MULTIPLY, DIVIDE }

    private Player[] players;
    private static List<RoundResult> roundResults;
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
                players[0].thrownHand = Hand.ROCK;
                players[0].hasThrown = true;
            }
            if (Input.IsActionJustPressed("paper0"))
            {
                players[0].thrownHand = Hand.PAPER;
                players[0].hasThrown = true;
            }
            if (Input.IsActionJustPressed("scissors0"))
            {
                players[0].thrownHand = Hand.SCISSORS;
                players[0].hasThrown = true;
            }
            if (Input.IsActionJustPressed("rock1"))
            {
                players[1].thrownHand = Hand.ROCK;
                players[1].hasThrown = true;
            }
            if (Input.IsActionJustPressed("paper1"))
            {
                players[1].thrownHand = Hand.PAPER;
                players[1].hasThrown = true;
            }
            if (Input.IsActionJustPressed("scissors1"))
            {
                players[1].thrownHand = Hand.SCISSORS;
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
                    selectUpgrade(players[0], 0);
                }
                if (Input.IsActionJustPressed("paper0"))
                {
                    selectUpgrade(players[0], 1);
                    GD.Print("2");
                }
                if (Input.IsActionJustPressed("scissors0"))
                {
                    selectUpgrade(players[0], 2);
                    GD.Print("3");
                }
            }
            else
            {
                if (Input.IsActionJustPressed("rock1"))
                {
                    selectUpgrade(players[1], 0);
                    GD.Print("1");
                }
                if (Input.IsActionJustPressed("paper1"))
                {
                    selectUpgrade(players[1], 1);
                    GD.Print("2");
                }
                if (Input.IsActionJustPressed("scissors1"))
                {
                    selectUpgrade(players[1], 2);
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
            //TODO remove this, just testing
            player.powerUpsObtained[BIGROCK_NAME] = new BigRock(player);
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

            string healthBarControl = player.VBoxPath + "/HealthBar";
            ProgressBar healthBar = (ProgressBar)GetNode(healthBarControl);
            healthBar.MaxValue = phaseHealth;
            updateHealth(player, phaseHealth);
            updateDamageUI(player);
        }
        isCombatEnabled = true;
    }

    //calculate damage and update UI text and color with those values
    private void updateDamageUI(Player player)
    {
        string bigRock = "";
        if (player.powerUpsObtained[BIGROCK_NAME].isActive())
        {
            bigRock = "Big ";
        }
        player.calculateHandValues();

        Button handButton = (Button)GetNode(player.VBoxPath + HANDS_UI_PATH + "/" + "Rock" + "Button");
        handButton.Text = bigRock + "Rock" + ": " + player.realHandValues[0];
        handButton = (Button)GetNode(player.VBoxPath + HANDS_UI_PATH + "/" + "Paper" + "Button");
        handButton.Text = "Paper" + ": " + player.realHandValues[1];
        handButton = (Button)GetNode(player.VBoxPath + HANDS_UI_PATH + "/" + "Scissors" + "Button");
        handButton.Text = "Scissors" + ": " + player.realHandValues[2];
        //TODO make this green or red if it's bigger/smaller
        //if (value = player.baseHandValues)

    }

    //calculate winner and apply damage
    private void throwHands()
    {
        Player player0 = players[0];
        Player player1 = players[1];
        player0.hasThrown = false;
        player1.hasThrown = false;
        int winner = -1;
        Player winnerPlayer = player0;
        Player loserPlayer = player1;
        int[] damageArray = new int[2];
        //tie case
        if (player0.thrownHand == player1.thrownHand)
        {
            //hardcoding bigRock tieBreaks
            if (player0.thrownHand == Hand.ROCK)
            {
                bool BRActive0 = player0.powerUpsObtained.ContainsKey(BIGROCK_NAME) ? player0.powerUpsObtained[BIGROCK_NAME].isActive() : false;
                bool BRActive1 = player1.powerUpsObtained.ContainsKey(BIGROCK_NAME) ? player1.powerUpsObtained[BIGROCK_NAME].isActive() : false;

                if (BRActive0 && !BRActive1)
                {
                    winner = 0;
                }
                else if (BRActive1 && !BRActive0)
                {
                    winner = 1;
                    winnerPlayer = player1;
                    loserPlayer = player0;
                }
            }
        }
        else if ((player0.thrownHand == Hand.ROCK && player1.thrownHand == Hand.SCISSORS) ||
            (player0.thrownHand == Hand.PAPER && player1.thrownHand == Hand.ROCK) ||
            (player0.thrownHand == Hand.SCISSORS && player1.thrownHand == Hand.PAPER))
        {
            winner = 0;
        }
        else
        {
            winner = 1;
            winnerPlayer = player1;
            loserPlayer = player0;
        }
        if (winner != -1)
        {
            int damage = winnerPlayer.realHandValues[(int)winnerPlayer.thrownHand];
            //1-x will flip 1 and 0
            damageArray[1 - winner] = damage;
            updateHealth(loserPlayer, loserPlayer.currentHealth - damage);
        }

        roundResults.Add(new RoundResult(new Hand[] { player0.thrownHand, player1.thrownHand }, damageArray, winner));

        updateDamageUI(player0);
        updateDamageUI(player1);
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

    // the player knows their own max health. we are simply setting the health to this value.
    // returns true or false based on ?? TODO
    private bool updateHealth(Player player, int health)
    {
        string healthBarControl = player.VBoxPath + "/HealthBar";
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
        for (int i = 0; i < NUM_HANDS; i++)
        {
            Control marginContainer = (Control)upgrades.GetNode("MarginContainer" + i + "/Panel/VBoxContainer");
            Label titleLabel = (Label)marginContainer.GetNode("Title");
            Label descriptionLabel = (Label)marginContainer.GetNode("Description");
            PowerUp powerUp = player.powerUpLibrary[i];
            GD.Print(powerUp.GetType().ToString());
            titleLabel.Text = powerUp.Name;
            descriptionLabel.Text = powerUp.Description;

        }
        upgrades.Show();
    }

    //updates the player's hand power in backend 
    // TODO delete ?? and UI
    private void setBaseHandPower(Player player, Hand hand, int value)
    {
        player.baseHandValues[(int)hand] = value;
        //string handName = hand.ToString();
        //Button handButton = (Button)GetNode(player.VBoxPath + HANDS_UI_PATH + "/" + handName + "Button");
        //handButton.Text = handName + ": " + value;
    }

    //update all 3 hands at once
    private void setAllHands(Player player, int rockValue, int paperValue, int scissorsValue)
    {
        setBaseHandPower(player, Hand.ROCK, rockValue);
        setBaseHandPower(player, Hand.PAPER, paperValue);
        setBaseHandPower(player, Hand.SCISSORS, scissorsValue);
    }

    private void gameLogDisplay(string labelText)
    {
        gameLog.Show();
        Label gameLogLabel = (Label)gameLog.GetNode("Label");
        gameLogLabel.Text = labelText;
    }
    // hold the player data. player object does NOT handle their own UI
    public class Player
    {
        public int playerId;
        public string VBoxPath;
        public int maxHealth;
        public int currentHealth;
        //these are white (unconditional) hand values
        // rock = 0, paper = 1, scissors = 2
        public int[] baseHandValues;
        public int[] realHandValues;
        public List<PowerUp> powerUpLibrary;
        public List<int> upgradeThresholds;
        public Dictionary<string, PowerUp> powerUpsObtained;
        public Hand thrownHand;
        public bool hasThrown;
        public Player(string path, int id)
        {
            playerId = id;
            VBoxPath = path;
            baseHandValues = new int[NUM_HANDS];
            realHandValues = new int[NUM_HANDS];
            hasThrown = false;
            upgradeThresholds = new List<int>();
            powerUpLibrary = new List<PowerUp>();
            powerUpLibrary.Add(new BigRock(this));
            powerUpLibrary.Add(new MindChange(this));
            powerUpLibrary.Add(new InARow(this));
            powerUpsObtained = new Dictionary<string, PowerUp>();
        }

        public void calculateHandValues()
        {
            List<Multiplier> addSubList = new List<Multiplier>();
            //TODO if necessary
            // List<Multiplier> multDivList = new List<Multiplier>;

            foreach (KeyValuePair<String, PowerUp> power in powerUpsObtained)
            {
                addSubList.AddRange(power.Value.calculateDamage());
            }
            Array.Copy(baseHandValues, realHandValues, baseHandValues.Length);
            foreach (Multiplier multiplier in addSubList)
            {
                realHandValues[(int)multiplier.hand] += multiplier.value;
            }
        }
    }

    // stores a single round of "combat". a list of these is one game
    public class RoundResult
    {
        public Hand[] playerHands;
        //we're saving damage for the UI even if it's not used for mechanics
        // this is an array because each player can take different amounts of damage in one round
        public int[] damages;
        //0 for player1, 1 for player2, -1 for tie
        public int winner;
        public RoundResult(Hand[] playerHands, int[] damages, int winner)
        {
            this.playerHands = playerHands;
            this.damages = damages;
            this.winner = winner;
        }
    }

    // contains an operation and a value for damage calculation.
    // examples Addition, 2, Paper = +2 Paper damage
    public class Multiplier
    {
        public Operation operation;
        public int value;
        public Hand hand;

        public Multiplier(Operation operation, int value, Hand hand)
        {
            this.operation = operation;
            this.value = value;
            this.hand = hand;
        }
    }

    public abstract class PowerUp
    {

        public abstract string Name { get; }
        public abstract string Description { get; }
        //tells the backend/UI whether to list this powerup as active
        public abstract bool isActive();
        //
        public abstract List<Multiplier> calculateDamage();
    }

    public class BigRock : PowerUp
    {
        public override string Name { get { return BIGROCK_NAME; } }
        public override string Description { get { return "Your third Rock thrown in a row breaks ties"; } }

        int ownerId;
        public BigRock(Player player)
        {
            ownerId = player.playerId;
        }

        public override bool isActive()
        {
            int numRounds = roundResults.Count;
            if (numRounds >= 2)
            {
                int i = numRounds - 1;
                int rocks = 0;
                while (i >= 0 && roundResults[i].playerHands[ownerId] == Hand.ROCK)
                {
                    i--;
                    rocks++;
                }
                if ((rocks + 1) % 3 == 0)
                {
                    GD.Print("BigRockActive for player " + ownerId);
                    return true;
                }
            }
            return false;
        }
        // this powerup does not affect damage and needs to be hardcoded
        public override List<Multiplier> calculateDamage()
        {
            return new List<Multiplier>();
        }
    }

    public class InARow : PowerUp
    {
        public override string Name { get { return "Streak Bonus"; } }
        public override string Description { get { return "Every hand you throw in a row is worth +1 more"; } }
        int ownerId;
        public InARow(Player player)
        {
            ownerId = player.playerId;
        }
        public override bool isActive()
        {
            return true;
        }
        public override List<Multiplier> calculateDamage()
        {
            List<Multiplier> multipliers = new List<Multiplier>();
            if (roundResults.Count >= 1)
            {
                int count = 1;
                int i = roundResults.Count - 1;
                Hand lastHand = roundResults[i].playerHands[ownerId];
                i--;
                while (i >= 0 && roundResults[i].playerHands[ownerId] == lastHand)
                {
                    count++;
                }
                multipliers.Add(new Multiplier(Operation.ADD, count, lastHand));
            }
            return multipliers;
        }
    }

    public class MindChange : PowerUp
    {
        public override string Name { get { return "Noncommittal"; } }
        public override string Description { get { return "+1 if you throw a different hand than before"; } }
        int ownerId;
        public MindChange(Player player)
        {
            ownerId = player.playerId;
        }
        public override bool isActive()
        {
            return true;
        }
        public override List<Multiplier> calculateDamage()
        {
            List<Multiplier> multipliers = new List<Multiplier>();
            if (roundResults.Count >= 1)
            {
                int i = roundResults.Count - 1;
                List<Hand> handList = new List<Hand> { Hand.ROCK, Hand.PAPER, Hand.SCISSORS };
                Hand lastHand = roundResults[i].playerHands[ownerId];
                handList.Remove(lastHand);
                foreach (Hand hand in handList)
                {
                    multipliers.Add(new Multiplier(Operation.ADD, 1, hand));
                }
            }
            return multipliers;
        }
    }
}
