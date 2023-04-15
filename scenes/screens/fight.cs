using Godot;
using System;
using System.Collections.Generic;

public partial class fight : Control
{
    //Allows us to access and update UI elements from this file
    const string PLAYER_NODE_PATH = "LocalHBox/PlayerVBox";
    const string GAMELOG_PATH = "UpgradesCanvas/UpgradesVBox/TextboxMargin/GameLog";
    const string UPGRADES_PATH = "UpgradesCanvas/UpgradesVBox/UpgradesHBox";
    const string RPSGO_PATH = "UpgradesCanvas/RPSGoMargin/GameLog";
    const string HANDS_UI_PATH = "/AttacksPanel/HandsHBox";
    const int NUM_PLAYERS = 2;
    const int NUM_HANDS = 3;
    const string BIGROCK_NAME = "Big Rock";

    public enum Hand { ROCK, PAPER, SCISSORS, NULL }
    public enum Operation { ADD, MULTIPLY, DIVIDE }

    private Player[] players;
    private static List<RoundResult> roundResults;
    // this is an array because we might want more phases in the future
    private int[] phaseHealth = { 20 };
    private int totalPhases = 1;
    private int currentPhase;
    private bool isCombatEnabled;
    private bool isUpgradePhase;
    private int upgradeSelection;
    private int upgradingPlayer;
    private bool isAnimating;

    Panel gameLog;
    Control upgradesControl;
    Panel refereePanel;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        refereePanel = GetNode<Panel>(RPSGO_PATH);
        upgradesControl = GetNode<Control>(UPGRADES_PATH);
        gameLog = GetNode<Panel>(GAMELOG_PATH);
        refereePanel.Hide();
        upgradesControl.Hide();
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
            if (players[1].hasThrown && players[0].hasThrown && !isAnimating)
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
                    if (upgradeSelection == 0)
                    {
                        selectUpgrade(players[0], 0);
                    }
                    else
                    {
                        upgradeSelection = 0;
                    }
                }
                if (Input.IsActionJustPressed("paper0"))
                {
                    if (upgradeSelection == 1)
                    {
                        selectUpgrade(players[0], 1);
                    }
                    else
                    {
                        upgradeSelection = 1;
                    }
                }
                if (Input.IsActionJustPressed("scissors0"))
                {
                    if (upgradeSelection == 2)
                    {
                        selectUpgrade(players[0], 2);
                    }
                    else
                    {
                        upgradeSelection = 2;
                    }
                }
            }
            else
            {
                if (Input.IsActionJustPressed("rock1"))
                {
                    if (upgradeSelection == 0)
                    {
                        selectUpgrade(players[1], 0);
                    }
                    else
                    {
                        upgradeSelection = 0;
                    }
                }
                if (Input.IsActionJustPressed("paper1"))
                {
                    if (upgradeSelection == 1)
                    {
                        selectUpgrade(players[1], 1);
                    }
                    else
                    {
                        upgradeSelection = 1;
                    }
                }

                if (Input.IsActionJustPressed("scissors1"))
                {
                    if (upgradeSelection == 2)
                    {
                        selectUpgrade(players[1], 2);
                    }
                    else
                    {
                        upgradeSelection = 2;
                    }
                }
            }
            displayUpgradeBorder();
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

            string healthBarControl = player.VBoxPath + "/HealthBar";
            ProgressBar healthBar = GetNode<ProgressBar>(healthBarControl);
            healthBar.MaxValue = phaseHealth;
            updateHealth(player, phaseHealth);
            updateDamageUI(player);
        }
        enableCombat();
    }

    private void enableCombat()
    {
        isCombatEnabled = true;
        //sayRPS();
    }

    private void sayRPS()
    {
        isAnimating = true;
        Label label = refereePanel.GetNode<Label>("Label");
        refereePanel.Show();
        label.Text = "Rock Paper Scissors";
        isAnimating = false;
    }

    //calculate damage and update UI text and color with those values
    private void updateDamageUI(Player player)
    {
        string bigRock = "";
        if (player.powerUpsObtained.ContainsKey(BIGROCK_NAME) && player.powerUpsObtained[BIGROCK_NAME].isActive(player))
        {
            bigRock = "Big ";
        }
        player.calculateHandValues();

        Button handButton = GetNode<Button>(player.VBoxPath + HANDS_UI_PATH + "/" + "Rock" + "Button");
        Panel panel = handButton.GetNode<Panel>("Panel");
        if (player.thrownHand == Hand.ROCK)
        {
            panel.Show();
        }
        else
        {
            panel.Hide();
        }
        handButton.Text = bigRock + "Rock" + ": " + player.realHandValues[0];

        handButton = GetNode<Button>(player.VBoxPath + HANDS_UI_PATH + "/" + "Paper" + "Button");
        panel = handButton.GetNode<Panel>("Panel");
        if (player.thrownHand == Hand.PAPER)
        {
            panel.Show();
        }
        else
        {
            panel.Hide();
        }
        handButton.Text = "Paper" + ": " + player.realHandValues[1];

        handButton = GetNode<Button>(player.VBoxPath + HANDS_UI_PATH + "/" + "Scissors" + "Button");
        panel = handButton.GetNode<Panel>("Panel");
        if (player.thrownHand == Hand.SCISSORS)
        {
            panel.Show();
        }
        else
        {
            panel.Hide();
        }
        handButton.Text = "Scissors" + ": " + player.realHandValues[2];
        //TODO make this green or red if it's bigger/smaller
        //if (value = player.baseHandValues)

    }

    //calculate winner and apply damage
    private void throwHands()
    {
        refereePanel.Hide();
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
                bool BRActive0 = player0.powerUpsObtained.ContainsKey(BIGROCK_NAME) ? player0.powerUpsObtained[BIGROCK_NAME].isActive(player0) : false;
                bool BRActive1 = player1.powerUpsObtained.ContainsKey(BIGROCK_NAME) ? player1.powerUpsObtained[BIGROCK_NAME].isActive(player1) : false;

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

    private void displayUpgradeBorder()
    {
        Panel panel0 = upgradesControl.GetNode<Panel>("MarginContainer0/OutlinePanel");
        Panel panel1 = upgradesControl.GetNode<Panel>("MarginContainer1/OutlinePanel");
        Panel panel2 = upgradesControl.GetNode<Panel>("MarginContainer2/OutlinePanel");
        panel0.Hide();
        panel1.Hide();
        panel2.Hide();

        if (upgradeSelection >= 0)
        {

            if (upgradeSelection == 0)
            {
                panel0.Show();
            }
            if (upgradeSelection == 1)
            {
                panel1.Show();
            }
            if (upgradeSelection == 2)
            {
                panel2.Show();
            }
        }
    }
    private void selectUpgrade(Player player, int selection)
    {
        isUpgradePhase = false;

        PowerUp selectedPowerUp = player.powerUpLibrary[selection + 3 * player.currentUpgradePhase];
        player.currentUpgradePhase++;
        player.powerUpsObtained.Add(selectedPowerUp.Name, selectedPowerUp);
        upgradesControl.Hide();
        refereePanel.Hide();
        gameLog.Hide();

        updateDamageUI(player);
        enableCombat();
    }

    private void endPhase(Player loser)
    {
        if (currentPhase == totalPhases)
        {
            //TODO move this to "end game" function if necessary
            gameLogDisplay("Game Over!");
            TextureRect loserPic = GetNode<TextureRect>(loser.VBoxPath + "/TextureRect");
            loserPic.FlipV = true;
            isCombatEnabled = false;
        }
    }

    // the player knows their own max health. we are simply setting the health to this value.
    // returns true or false based on ?? TODO
    private bool updateHealth(Player player, int health)
    {
        string healthBarControl = player.VBoxPath + "/HealthBar";
        ProgressBar healthBar = GetNode<ProgressBar>(healthBarControl);
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
            player.upgradeThresholds.RemoveAt(0);
            startUpgradePhase(player);
        }

        return false;
    }


    private void startUpgradePhase(Player player)
    {
        upgradesControl.Show();
        isUpgradePhase = true;
        upgradeSelection = -1;
        isCombatEnabled = false;
        upgradingPlayer = player.playerId;
        Control upgrades = GetNode<Control>(UPGRADES_PATH);
        for (int i = 0; i < NUM_HANDS; i++)
        {
            Control marginContainer = (Control)upgrades.GetNode("MarginContainer" + i + "/VBoxContainer");
            Label titleLabel = (Label)marginContainer.GetNode("Panel0/Title");
            Label descriptionLabel = (Label)marginContainer.GetNode("Panel1/Description");
            PowerUp powerUp = player.powerUpLibrary[i + 3 * player.currentUpgradePhase];
            titleLabel.Text = powerUp.Name;
            descriptionLabel.Text = powerUp.Description;

        }
        int display = upgradingPlayer + 1;
        gameLogDisplay("Player " + display + " Double press rock/paper/scissors to Evolve");
        gameLog.Show();

        StyleBox playerStyleBox = GetTree().Root.GetThemeStylebox("player" + player.playerId + "stylebox", "StyleBoxFlat");
        GD.Print(playerStyleBox);
        gameLog.AddThemeStyleboxOverride("normal", playerStyleBox);
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
        public int currentUpgradePhase;

        public Player(string path, int id)
        {
            currentUpgradePhase = 0;
            playerId = id;
            VBoxPath = path;
            baseHandValues = new int[NUM_HANDS];
            realHandValues = new int[NUM_HANDS];
            thrownHand = Hand.NULL;
            hasThrown = false;
            upgradeThresholds = new List<int>();
            powerUpLibrary = new List<PowerUp>();
            powerUpLibrary.Add(new TieBonus());
            powerUpLibrary.Add(new ChangeBonus());
            powerUpLibrary.Add(new InARow());
            powerUpLibrary.Add(new BigRock());
            powerUpLibrary.Add(new WinMoreScissors());
            powerUpLibrary.Add(new PaperSwap());
            powerUpsObtained = new Dictionary<string, PowerUp>();
        }

        public void calculateHandValues()
        {
            List<Multiplier> addSubList = new List<Multiplier>();
            //TODO if necessary
            // List<Multiplier> multDivList = new List<Multiplier>;

            foreach (KeyValuePair<String, PowerUp> power in powerUpsObtained)
            {
                addSubList.AddRange(power.Value.calculateDamage(this));
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
        public abstract bool isActive(Player player);
        //calculate the damage this powerup provides to the player owning it
        public abstract List<Multiplier> calculateDamage(Player player);
    }

    public class BigRock : PowerUp
    {
        public int roundObtained;
        public override string Name { get { return BIGROCK_NAME; } }
        public override string Description { get { return "Every third Rock thrown in a row is worth +3 and breaks Ties"; } }

        public BigRock()
        {
            roundObtained = 0;
        }
        public override bool isActive(Player player)
        {
            int ownerId = player.playerId;
            int numRounds = roundResults.Count;
            if (numRounds >= 2)
            {
                int i = numRounds - 1;
                int rocks = 0;
                while (i > roundObtained && roundResults[i].playerHands[ownerId] == Hand.ROCK)
                {
                    i--;
                    rocks++;
                }
                if ((rocks + 1) % 3 == 0)
                {
                    return true;
                }
            }
            return false;
        }
        // this powerup does not affect damage and needs to be hardcoded in the combat section
        public override List<Multiplier> calculateDamage(Player player)
        {
            List<Multiplier> powerup = new List<Multiplier>();
            if (isActive(player))
            {
                powerup.Add(new Multiplier(Operation.ADD, 3, Hand.ROCK));
            }
            return powerup;
        }
    }

    public class InARow : PowerUp
    {
        public override string Name { get { return "Streak Bonus"; } }
        public override string Description { get { return "+1 for each same hand you throw in a row"; } }
        public override bool isActive(Player player)
        {
            return true;
        }
        public override List<Multiplier> calculateDamage(Player player)
        {
            int ownerId = player.playerId;
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
                    i--;

                }
                multipliers.Add(new Multiplier(Operation.ADD, count, lastHand));
            }
            return multipliers;
        }
    }

    public class ChangeBonus : PowerUp
    {
        public override string Name { get { return "Unpredictable"; } }
        public override string Description { get { return "+1 if you throw a different hand than before"; } }
        public override bool isActive(Player player)
        {
            return true;
        }
        public override List<Multiplier> calculateDamage(Player player)
        {
            int ownerId = player.playerId;
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

    public class TieBonus : PowerUp
    {
        public override string Name { get { return "Silver Lining"; } }
        public override string Description { get { return "+2 for each Tie in a row before you Win"; } }
        public override bool isActive(Player player)
        {
            return roundResults[roundResults.Count - 1].winner == -1;
        }
        public override List<Multiplier> calculateDamage(Player player)
        {
            List<Multiplier> multipliers = new List<Multiplier>();
            int tieCount = 0;
            for (int i = roundResults.Count - 1; i >= 0; i--)
            {
                if (roundResults[i].winner == -1)
                {
                    tieCount++;
                }
                else
                {
                    break;
                }
            }
            multipliers.Add(new Multiplier(Operation.ADD, tieCount, Hand.ROCK));
            multipliers.Add(new Multiplier(Operation.ADD, tieCount, Hand.PAPER));
            multipliers.Add(new Multiplier(Operation.ADD, tieCount, Hand.SCISSORS));

            return multipliers;
        }
    }
    public class WinMoreScissors : PowerUp
    {
        public int roundObtained;
        public override string Name { get { return "When it Glides"; } }
        public override string Description { get { return "+1 to Scissors every time you Win with Scissors"; } }

        public WinMoreScissors()
        {
            roundObtained = 0;
        }

        public override bool isActive(Player player)
        {
            return true;
        }
        public override List<Multiplier> calculateDamage(Player player)
        {
            int playerId = player.playerId;
            int winCount = 0;
            for (int i = roundResults.Count - 1; i > roundObtained; i--)
            {
                RoundResult round = roundResults[i];
                if (round.winner == playerId && round.playerHands[playerId] == Hand.SCISSORS)
                {
                    winCount++;
                }
            }
            return new List<Multiplier>() { new Multiplier(Operation.ADD, winCount, Hand.SCISSORS) };
        }

    }

    public class PaperSwap : PowerUp
    {
        public override string Name { get { return "Origami"; } }
        public override string Description { get { return "+1 if your last hand was Paper"; } }
        public override bool isActive(Player player)
        {
            return roundResults[roundResults.Count - 1].playerHands[player.playerId] == Hand.PAPER;
        }
        public override List<Multiplier> calculateDamage(Player player)
        {
            List<Multiplier> multipliers = new List<Multiplier>();
            if (isActive(player))
            {
                multipliers.Add(new Multiplier(Operation.ADD, 1, Hand.ROCK));
                multipliers.Add(new Multiplier(Operation.ADD, 1, Hand.PAPER));
                multipliers.Add(new Multiplier(Operation.ADD, 1, Hand.SCISSORS));
            }
            return multipliers;
        }
    }
}