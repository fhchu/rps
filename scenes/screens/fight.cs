using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class fight : Control
{
    //Allows us to access and update UI elements from this file
    const string PLAYER_NODE_PATH = "LocalHBox/PlayerVBox";
    const string HEALTH_BAR_PATH = "/HealthBarMargin/BarVBox/HealthBar";
    const string EXP_BAR_PATH = "/HealthBarMargin/BarVBox/ExpBar";
    const string GAMELOG_PATH = "UpgradesCanvas/UpgradesVBox/TextboxMargin/GameLog";
    const string UPGRADES_PATH = "UpgradesCanvas/UpgradesVBox/UpgradesHBox";
    const string RPSGO_PATH = "UpgradesCanvas/RPSGoMargin/GameLog";
    const string HANDS_UI_PATH = "/HandsMargin/HandsHBox";
    const string SPRITE_UI_PATH = "/PlayerTexture";
    const string READY_UI_PATH = "/ReadyLabel";

    const int NUM_PLAYERS = 2;
    const int NUM_HANDS = 3;
    // we might want to have different/separate player health someday but not right now
    const int PLAYER_HEALTH = 25;
    const string BIGROCK_NAME = "Big Rock";

    public enum Hand { ROCK, PAPER, SCISSORS, NULL }
    public enum Operation { ADD, MULTIPLY, DIVIDE }

    private Player[] players;
    private static List<RoundResult> roundResults;
    // this is an array because we might want more phases in the future
    //private int[] phaseHealth = { 20 };
    //private int totalPhases = 1;
    //private int currentPhase;
    private bool isCombatEnabled;
    private bool isUpgradePhase;
    private int upgradingPlayer;
    private int upgradeSelection;
    private bool isAnimating;

    private bool animationsEnabled = true;

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
        //currentPhase = 0;
        setUpPlayers();
        roundResults = new List<RoundResult>();
        startGame(PLAYER_HEALTH);
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
                sayReady(false, false);
                calculateDamage();
            }
            else
            {
                sayReady(players[0].hasThrown, players[1].hasThrown);
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
            displayUpgradeOutline();
        }
    }

    // build players before game starts.
    // we setup HP and exp bars
    private void setUpPlayers()
    {
        players = new Player[NUM_PLAYERS];
        for (int i = 0; i < NUM_PLAYERS; i++)
        {
            // perhaps we can have different exp required per player? 
            // level exp must be added in ascending order (or we're buggin out!!)
            List<int> expLevels = new List<int>() { 5, 8, 13 };

            List<PowerUp> powerUpLibrary = new List<PowerUp>();

            powerUpLibrary.Add(new TieBonus());
            powerUpLibrary.Add(new ChangeBonus());
            powerUpLibrary.Add(new InARow());
            powerUpLibrary.Add(new BigRock());
            powerUpLibrary.Add(new WinMoreScissors());
            powerUpLibrary.Add(new PaperSwap());
            powerUpLibrary.Add(new mimicHand());
            powerUpLibrary.Add(new exodia());
            powerUpLibrary.Add(new thirdSwitch());

            Player player = new Player(i, PLAYER_NODE_PATH + i, expLevels, powerUpLibrary);

            players[i] = player;
            // player does not handle setting of their own powers
            setAllHands(player, 1, 1, 1);
        }
    }

    //update all 3 hands at once
    private void setAllHands(Player player, int rockValue, int paperValue, int scissorsValue)
    {
        setBaseHandPower(player, Hand.ROCK, rockValue);
        setBaseHandPower(player, Hand.PAPER, paperValue);
        setBaseHandPower(player, Hand.SCISSORS, scissorsValue);
    }

    //updates the player's base hand power in backend 
    private void setBaseHandPower(Player player, Hand hand, int value)
    {
        player.baseHandValues[(int)hand] = value;
    }

    // sets up fighting once upgrades have been taken
    private void startGame(int gameHealth)
    {
        //currentPhase += 1;
        for (int i = 0; i < NUM_PLAYERS; i++)
        {
            Player player = players[i];
            player.maxHealth = gameHealth;

            string healthBarControl = player.VBoxPath + HEALTH_BAR_PATH;
            ProgressBar healthBar = GetNode<ProgressBar>(healthBarControl);
            healthBar.MaxValue = gameHealth;

            updateHealth(player, gameHealth);

            player.maxExp = player.expLevels[player.level];
            updateExp(player, 0, true);
            updateDamageUI(player);
        }
        enableCombat();
    }

    // the player knows their own max health. we are simply setting the health to this value.
    // returns true if combat should immediately continue, else false
    private bool updateHealth(Player player, int health)
    {
        string healthBarControl = player.VBoxPath + HEALTH_BAR_PATH;
        ProgressBar healthBar = GetNode<ProgressBar>(healthBarControl);
        player.currentHealth = health;
        healthBar.Value = health;
        Label healthLabel = (Label)healthBar.GetChild(0);
        healthLabel.Text = player.currentHealth.ToString();

        if (player.currentHealth <= 0)
        {
            endGame(player);
            return false;
        }

        return true;
    }

    // we update exp ui and start upgrade phase if necessary
    // returns true if we started upgrade phase, else false
    private bool updateExp(Player player, int exp, bool canLevel)
    {
        string expControlPath = player.VBoxPath + EXP_BAR_PATH;
        ProgressBar expBar = GetNode<ProgressBar>(expControlPath);
        Label expLabel = (Label)expBar.GetChild(0);

        if (player.isMaxLevel())
        {
            expBar.Value = 0;
            expBar.MaxValue = 1;
            expLabel.Text = "MAX";
            return false;
        }

        expBar.Value = exp;
        expBar.MaxValue = player.maxExp;
        expLabel.Text = exp + "/" + player.maxExp;

        player.currentExp = exp;

        if (canLevel && player.currentExp >= player.maxExp)
        {
            displayUpgrades(player);

            return true;
        }

        return false;
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
        handButton.Text = player.realHandValues[0] + "\n" + bigRock + "Rock";

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
        handButton.Text = player.realHandValues[1] + "\n" + "Paper";

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
        handButton.Text = player.realHandValues[2] + "\n" + "Scissors";
        //TODO make this green or red if it's bigger/smaller
    }



    // activate combat polling for input and play RPSGO animation
    private void enableCombat()
    {
        isCombatEnabled = true;
        sayRPS();
    }

    private async void sayRPS()
    {
        if (!animationsEnabled)
        {
            return;
        }
        isAnimating = true;
        Label label = refereePanel.GetNode<Label>("Label");
        refereePanel.Show();
        label.Text = "Rock";
        await ToSignal(GetTree().CreateTimer(0.35f), SceneTreeTimer.SignalName.Timeout);
        label.Text = "Paper";
        await ToSignal(GetTree().CreateTimer(0.35f), SceneTreeTimer.SignalName.Timeout);
        label.Text = "Scissors";
        await ToSignal(GetTree().CreateTimer(0.35f), SceneTreeTimer.SignalName.Timeout);
        refereePanel.Hide();
        isAnimating = false;
    }

    private void sayReady(bool player0Ready, bool player1Ready)
    {

        Label player0label = GetNode<Label>(PLAYER_NODE_PATH + 0 + SPRITE_UI_PATH + READY_UI_PATH);
        Label player1label = GetNode<Label>(PLAYER_NODE_PATH + 1 + SPRITE_UI_PATH + READY_UI_PATH);

        player0label.Hide();
        player1label.Hide();

        if (player0Ready)
        {
            player0label.Show();

        }
        if (player1Ready)
        {
            player1label.Show();

        }
    }
    //calculate winner and apply damage
    private async void calculateDamage()
    {
        isCombatEnabled = false;
        refereePanel.Hide();
        Player player0 = players[0];
        Player player1 = players[1];
        player0.hasThrown = false;
        player1.hasThrown = false;
        int winner = -1;
        Player winnerPlayer = player0;
        Player loserPlayer = player1;
        // this is an array because maybe both players can take damage
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
        await sayRoundWinner(winner);
        int baseExp = 1;
        if (winner != -1)
        {
            int damage = winnerPlayer.realHandValues[(int)winnerPlayer.thrownHand];
            //1-x will flip 1 and 0
            damageArray[1 - winner] = damage;


            // enable combat if we're not dead
            if (updateHealth(loserPlayer, loserPlayer.currentHealth - damage))
            {
                // if the loser upgrade phase didn't start, we try the winner upgrade phase now. 
                // otherwise it will check after the loser upgrade phase completes.
                bool didUpgrade = updateExp(loserPlayer, loserPlayer.currentExp + damage + baseExp, true);
                didUpgrade = didUpgrade || updateExp(winnerPlayer, winnerPlayer.currentExp + baseExp, didUpgrade);

                if (!didUpgrade)
                {
                    enableCombat();
                }
            }
        }
        else
        {
            updateExp(loserPlayer, Math.Min(loserPlayer.currentExp + baseExp, loserPlayer.maxExp), false);
            updateExp(winnerPlayer, Math.Min(winnerPlayer.currentExp + baseExp, winnerPlayer.maxExp), false);
            enableCombat();
        }
        roundResults.Add(new RoundResult(new Hand[] { player0.thrownHand, player1.thrownHand }, damageArray, winner));

        updateDamageUI(player0);
        updateDamageUI(player1);

    }

    // explain who won the last round
    private async Task sayRoundWinner(int winner)
    {
        if (!animationsEnabled)
        {
            return;
        }
        bool isTie = false;
        if (winner == -1)
        {
            isTie = true;
            winner = 0;
        }
        int loser = 1 - winner;
        TextureRect winnerPicture = GetNode<TextureRect>(PLAYER_NODE_PATH + winner + SPRITE_UI_PATH);
        Label winnerLabel = winnerPicture.GetChild<Label>(0);

        TextureRect loserPicture = GetNode<TextureRect>(PLAYER_NODE_PATH + loser + SPRITE_UI_PATH);
        Label loserLabel = loserPicture.GetChild<Label>(0);

        float waitTime = 0.5f;
        if (isTie)
        {
            winnerLabel.Text = "Tie";
            loserLabel.Text = "Tie";
            winnerLabel.Show();
            loserLabel.Show();
            await ToSignal(GetTree().CreateTimer(waitTime), SceneTreeTimer.SignalName.Timeout);
            winnerLabel.Hide();
            loserLabel.Hide();
        }
        else
        {
            var happyPicture = GD.Load<Texture2D>("res://assets/player sprites/catHappy.png");
            var defaultPicture = GD.Load<Texture2D>("res://assets/player sprites/catDefault.png");
            var sadPicture = GD.Load<Texture2D>("res://assets/player sprites/catSad.png");

            winnerPicture.Texture = happyPicture;
            loserPicture.Texture = sadPicture;
            winnerLabel.Text = "Win!";
            winnerLabel.Show();
            await ToSignal(GetTree().CreateTimer(waitTime), SceneTreeTimer.SignalName.Timeout);
            winnerLabel.Hide();
            winnerPicture.Texture = defaultPicture;
            loserPicture.Texture = defaultPicture;
        }
    }

    // show the upgrade UI. does not handle upgrade logic
    private void displayUpgrades(Player player)
    {
        isCombatEnabled = false;
        isUpgradePhase = true;
        upgradesControl.Show();
        upgradeSelection = -1;
        upgradingPlayer = player.playerId;
        Control upgrades = GetNode<Control>(UPGRADES_PATH);
        for (int i = 0; i < NUM_HANDS; i++)
        {
            Control marginContainer = (Control)upgrades.GetNode("MarginContainer" + i + "/VBoxContainer");
            Label titleLabel = (Label)marginContainer.GetNode("Panel0/Title");
            Label descriptionLabel = (Label)marginContainer.GetNode("Panel1/Description");
            PowerUp powerUp = player.powerUpLibrary[i + 3 * player.level];
            titleLabel.Text = powerUp.Name;
            descriptionLabel.Text = powerUp.Description;

        }
        int display = upgradingPlayer + 1;
        gameLogDisplay("Player " + display + " Double press rock/paper/scissors to Evolve");
        gameLog.Show();

        // change the color of the box ?
        //StyleBox playerStyleBox = GetTree().Root.GetThemeStylebox("player" + player.playerId + "stylebox", "StyleBoxFlat");
        //GD.Print(playerStyleBox);
        //gameLog.AddThemeStyleboxOverride("normal", playerStyleBox);
        upgrades.Show();
    }

    // display color highlight for chosen upgrade
    private void displayUpgradeOutline()
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

    // handle the logic of picking an upgrade
    private void selectUpgrade(Player player, int selection)
    {
        isUpgradePhase = false;

        PowerUp selectedPowerUp = player.powerUpLibrary[selection + 3 * player.level];
        player.level++;
        player.powerUpsObtained.Add(selectedPowerUp.Name, selectedPowerUp);
        upgradesControl.Hide();
        refereePanel.Hide();
        gameLog.Hide();

        updateDamageUI(player);
        int newExp = player.currentExp - player.maxExp;
        if (!player.isMaxLevel())
        {
            player.maxExp = player.expLevels[player.level];
        }
        bool reUpgrade = updateExp(player, newExp, true);

        // check if opponent can upgrade. if we didn't upgrade and neither do they, we fight
        Player opponent = players[1 - player.playerId];
        if (!reUpgrade && !updateExp(opponent, opponent.currentExp, true))
        {
            enableCombat();
        }
    }

    // when one player has 0 hp, the game is over
    private void endGame(Player loser)
    {
        gameLogDisplay("Game Over!");
        TextureRect loserPic = GetNode<TextureRect>(PLAYER_NODE_PATH + loser.playerId + SPRITE_UI_PATH);
        var sadPicture = GD.Load<Texture2D>("res://assets/player sprites/catSad.png");
        loserPic.Texture = sadPicture;
        loserPic.FlipV = true;
        isCombatEnabled = false;
    }

    // display text on the thing on the
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
        public int level;

        public int currentExp;
        public int maxExp;
        //these are white (unconditional) hand values
        // rock = 0, paper = 1, scissors = 2
        public int[] baseHandValues;
        public int[] realHandValues;
        public List<PowerUp> powerUpLibrary;
        public List<int> expLevels;
        public List<int> levelUpRounds;
        public Dictionary<string, PowerUp> powerUpsObtained;
        public Hand thrownHand;
        public bool hasThrown;

        public Player(int id, string path, List<int> expLevels, List<PowerUp> powerUpLibrary)
        {
            VBoxPath = path;
            playerId = id;
            this.expLevels = expLevels;
            this.powerUpLibrary = powerUpLibrary;

            level = 0;
            baseHandValues = new int[NUM_HANDS];
            realHandValues = new int[NUM_HANDS];
            thrownHand = Hand.NULL;
            hasThrown = false;
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

        public bool isMaxLevel()
        {
            return level >= expLevels.Count;
        }

        public bool isMaxExp()
        {
            return currentExp >= maxExp;
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
        public abstract int RoundObtained { set; }

        //tells the backend/UI whether to list this powerup as active
        public abstract bool isActive(Player player);
        //calculate the damage this powerup provides to the player owning it
        public abstract List<Multiplier> calculateDamage(Player player);
    }

    public class InARow : PowerUp
    {
        public override string Name { get { return "Streak Bonus"; } }
        public override string Description { get { return "+1 for each same hand you throw in a row"; } }
        private int roundObtained;
        public override int RoundObtained { set { roundObtained = value; } }
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
        private int roundObtained;
        public override int RoundObtained { set { roundObtained = value; } }
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
        public override string Description { get { return "+1 to all Hands for each Tie in a row before you Win"; } }
        private int roundObtained;
        public override int RoundObtained { set { roundObtained = value; } }
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

    // old bigrock implementation. saving for later
    /*
    public class BigRock : PowerUp
    {
        public int roundObtained;
        public override string Name { get { return BIGROCK_NAME; } }
        public override string Description { get { return "Every Rock thrown in a row is worth +2 more. Big Rocks break Ties"; } }

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
        public override List<Multiplier> calculateDamage(Player player)
        {
            List<Multiplier> powerup = new List<Multiplier>();
            if (isActive(player))
            {
                powerup.Add(new Multiplier(Operation.ADD, 3, Hand.ROCK));
            }
            return powerup;
        }
    } */

    public class BigRock : PowerUp
    {
        public override string Name { get { return BIGROCK_NAME; } }
        public override string Description { get { return "Every Rock thrown in a row is worth +2 more. 3 or more Rocks in a row break ties"; } }
        private int rocksCounted;
        private int roundObtained;
        public override int RoundObtained { set { roundObtained = value; } }

        // this is specifically for the tiebreaker effect
        public override bool isActive(Player player)
        {
            int ownerId = player.playerId;
            int numRounds = roundResults.Count;

            countRocks(ownerId);
            if (rocksCounted >= 2)
            {
                return true;
            }

            return false;
        }
        public override List<Multiplier> calculateDamage(Player player)
        {
            List<Multiplier> powerup = new List<Multiplier>();

            countRocks(player.playerId);
            powerup.Add(new Multiplier(Operation.ADD, 2 * rocksCounted, Hand.ROCK));
            return powerup;
        }

        private void countRocks(int ownerId)
        {
            int numRounds = roundResults.Count;

            int i = numRounds - 1;
            int rocks = 0;
            while (roundResults[i].playerHands[ownerId] == Hand.ROCK)
            {
                i--;
                rocks++;
            }
            rocksCounted = rocks;
        }
    }

    public class WinMoreScissors : PowerUp
    {
        public override string Name { get { return "When it Glides"; } }
        public override string Description { get { return "+2 to Scissors permanently every time you Win with Scissors"; } }
        private int roundObtained;
        public override int RoundObtained { set { roundObtained = value; } }

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
        public override string Description { get { return "+2 to all Hands if your last hand was Paper"; } }
        private int roundObtained;
        public override int RoundObtained { set { roundObtained = value; } }
        public override bool isActive(Player player)
        {
            return roundResults[roundResults.Count - 1].playerHands[player.playerId] == Hand.PAPER;
        }
        public override List<Multiplier> calculateDamage(Player player)
        {
            List<Multiplier> multipliers = new List<Multiplier>();
            if (isActive(player))
            {
                multipliers.Add(new Multiplier(Operation.ADD, 2, Hand.ROCK));
                multipliers.Add(new Multiplier(Operation.ADD, 2, Hand.PAPER));
                multipliers.Add(new Multiplier(Operation.ADD, 2, Hand.SCISSORS));
            }
            return multipliers;
        }
    }

    public class mimicHand : PowerUp
    {
        public override string Name { get { return "Copycat"; } }
        public override string Description { get { return "+2 to the Hand your opponent last played"; } }
        private int roundObtained;
        public override int RoundObtained { set { roundObtained = value; } }
        public override bool isActive(Player player)
        {
            return true;
        }
        public override List<Multiplier> calculateDamage(Player player)
        {
            List<Multiplier> multipliers = new List<Multiplier>();
            if (isActive(player))
            {
                Hand lastHand = roundResults[roundResults.Count - 1].playerHands[1 - player.playerId];
                multipliers.Add(new Multiplier(Operation.ADD, 2, lastHand));

            }
            return multipliers;
        }
    }

    public class exodia : PowerUp
    {
        public override string Name { get { return "Out of Hand"; } }
        public override string Description { get { return "+3 to all Hands if your last 3 hands are unique"; } }
        private int roundObtained;
        public override int RoundObtained { set { roundObtained = value; } }
        public override bool isActive(Player player)
        {
            int numRounds = roundResults.Count;
            if (numRounds >= 3)
            {
                int[] hands = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    hands[(int)roundResults[numRounds - i - 1].playerHands[player.playerId]]++;
                }
                for (int i = 0; i < 3; i++)
                {
                    if (hands[i] != 1)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        public override List<Multiplier> calculateDamage(Player player)
        {
            List<Multiplier> multipliers = new List<Multiplier>();
            if (isActive(player))
            {
                if (isActive(player))
                {
                    multipliers.Add(new Multiplier(Operation.ADD, 3, Hand.ROCK));
                    multipliers.Add(new Multiplier(Operation.ADD, 3, Hand.PAPER));
                    multipliers.Add(new Multiplier(Operation.ADD, 3, Hand.SCISSORS));
                }

            }
            return multipliers;
        }
    }

    public class thirdSwitch : PowerUp
    {
        public override string Name { get { return "Combo Breaker"; } }
        public override string Description { get { return "+3 if you switch hands after playing the same hand twice"; } }
        private int roundObtained;
        public override int RoundObtained { set { roundObtained = value; } }
        public override bool isActive(Player player)
        {
            int numRounds = roundResults.Count;
            if (numRounds >= 2)
            {
                int playerId = player.playerId;
                Hand lastHand = roundResults[numRounds - 1].playerHands[playerId];
                Hand lastLasthand = roundResults[numRounds - 2].playerHands[playerId];
                if (lastHand == lastLasthand)
                {
                    return true;
                }
            }
            return false;
        }
        public override List<Multiplier> calculateDamage(Player player)
        {
            List<Multiplier> multipliers = new List<Multiplier>();
            if (isActive(player))
            {
                Hand lastHand = roundResults[roundResults.Count - 1].playerHands[player.playerId];
                if (lastHand == Hand.ROCK)
                {
                    multipliers.Add(new Multiplier(Operation.ADD, 3, Hand.PAPER));
                    multipliers.Add(new Multiplier(Operation.ADD, 3, Hand.SCISSORS));
                }
                if (lastHand == Hand.PAPER)
                {
                    multipliers.Add(new Multiplier(Operation.ADD, 3, Hand.ROCK));
                    multipliers.Add(new Multiplier(Operation.ADD, 3, Hand.SCISSORS));
                }
                if (lastHand == Hand.SCISSORS)
                {
                    multipliers.Add(new Multiplier(Operation.ADD, 3, Hand.ROCK));
                    multipliers.Add(new Multiplier(Operation.ADD, 3, Hand.PAPER));
                }
            }
            return multipliers;
        }
    }
}