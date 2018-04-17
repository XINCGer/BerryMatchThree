using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Berry.Utils;

// Game logic class
public class SessionAssistant : MonoBehaviour {

	public static SessionAssistant main;
    public int blockCountTotal;
    public int jellyCountTotal;

    public bool squareCombination = true;
    public List<Combinations> combinations = new List<Combinations>();
    public List<ChipInfo> chipInfos = new List<ChipInfo>();
    public List<BlockInfo> blockInfos = new List<BlockInfo>();
    public List<Mix> mixes = new List<Mix>();

	List<Solution> solutions = new List<Solution>();
    
	public int lastMovementId;
	public int movesCount; // Number of remaining moves
	public int swapEvent; // After each successed swap this parameter grows by 1 
	public int[] countOfEachTargetCount = {0,0,0,0,0,0};// Array of counts of each color matches. Color ID is index.
	public float timeLeft; // Number of remaining time
	public int eventCount; // Event counter
	public int score = 0; // Current score
	public int[] colorMask = new int[6]; // Mask of random colors: color number - colorID
    public int targetSugarDropsCount;
    public int creatingSugarDropsCount;
	public bool isPlaying = false;
	public bool outOfLimit = false;
	public bool reachedTheTarget = false;
    public int creatingSugarTask = 0;
    public bool firstChipGeneration = false;
    public int matchCount = 0;

    public int stars;

    bool targetRoutineIsOver = false;
	bool limitationRoutineIsOver = false;
	
	bool wait = false;
	public static int scoreC = 10; // Score multiplier

    void Awake() {
        main = this;
        combinations.Sort((Combinations a, Combinations b) => {
            if (a.priority < b.priority)
                return -1;
            if (a.priority > b.priority)
                return 1;
            return 0;
        });
    }

    void Start() {
        DebugPanel.AddDelegate("Complete the level", () => {
            if (isPlaying) {
                reachedTheTarget = true;
                movesCount = 0;
                timeLeft = 0;
                score = LevelProfile.main.thirdStarScore;
            }
        });

        DebugPanel.AddDelegate("Fail the level", () => {
            if (isPlaying) {
                reachedTheTarget = false;
                limitationRoutineIsOver = true;
                movesCount = 0;
                timeLeft = 0;
            }
        });

        DebugPanel.AddDelegate("Add a bomb", () => {
            if (isPlaying) {
                List<string> powerups = chipInfos.Select(x => x.name).ToList();
                powerups.Remove("SimpleChip");
                if (powerups.Contains("Sugar"))
                    powerups.Remove("Sugar");
                if (powerups.Contains("Stone"))
                    powerups.Remove("Stone");
                FieldAssistant.main.AddPowerup(powerups[Random.Range(0, powerups.Count)]);
            }
        });
    }

    void Update () {
        DebugPanel.Log("Busy", "Session", Chip.busyList.Count);
        DebugPanel.Log("Can I Wait", "Session", CanIWait());
    }

    void OnApplicationPause(bool pauseStatus) {
        if (isPlaying)
            UIAssistant.main.ShowPage("Pause");

    }

    // Reset variables
    public static void Reset() {
        main.stars = 0;

        main.eventCount = 0;
        main.matchCount = 0;
        main.lastMovementId = 0;
        main.swapEvent = 0;
        main.score = 0;
        main.firstChipGeneration = true;

        main.isPlaying = false;
        main.movesCount = LevelProfile.main.limit;
        main.timeLeft = LevelProfile.main.limit;
        main.countOfEachTargetCount = new int[] { 0, 0, 0, 0, 0, 0};
        main.creatingSugarTask = 0;


        main.reachedTheTarget = false;
		main.outOfLimit = false;

		main.targetRoutineIsOver = false;
		main.limitationRoutineIsOver = false;

        main.iteraction = true;
	}

	// Add extra moves (booster effect)
	public void AddExtraMoves () {
		if (!isPlaying) return;
        if (ProfileAssistant.main.local_profile["move"] == 0) return;
        ProfileAssistant.main.local_profile["move"]--;
        ItemCounter.RefreshAll();
        movesCount += 5;
        Continue();
	}

    // Add extra time (booster effect)
    public void AddExtraTime () {
		if (!isPlaying) return;
        if (ProfileAssistant.main.local_profile["time"] == 0) return;
        ProfileAssistant.main.local_profile["time"]--;
        ItemCounter.RefreshAll();
		timeLeft += 15;
        Continue();
	}

    public void MixChips(Chip a, Chip b) {
        Mix mix = Mix.FindMix(a.chipType, b.chipType);
        if (mix == null)
            return;
        Chip target = null;
        Chip secondary = null;
        if (a.chipType == mix.pair.a) {
            target = a;
            secondary = b;
        }
        if (b.chipType == mix.pair.a) {
            target = b;
            secondary = a;
        }


        if (target == null) {
            Debug.LogError("It can't be mixed, because there is no target chip");
            return;
        }
        b.slot.chip = target;
        secondary.HideChip(false);

        if (target.jamType == "")
            target.jamType = secondary.jamType;

        target.SendMessage(mix.function, secondary);
    }

	// Resumption of gameplay
	public void Continue () {
        UIAssistant.main.ShowPage("Field");
		wait = false;
	}

	// Starting next level
	public void PlayNextLevel() {
		if (CPanel.uiAnimation > 0) return;
        StartCoroutine(PlayLevelRoutine(LevelProfile.main.level + 1));
	}

    IEnumerator PlayLevelRoutine(int level) {
        yield return StartCoroutine(QuitCoroutine());
        while (CPanel.uiAnimation > 0)
            yield return 0;
        int location = 0;
        for (int i = 0; i < LevelMap.main.locationLevelNumber.Length; i++)
            if (LevelMap.main.locationLevelNumber[i] < level)
                location = i;
            else
                break;
        LevelMap.main.locations[location].ApplyBackground();
        Level.LoadLevel(level);
    }

	// Restart the current level
	public void RestartLevel() {
		if (CPanel.uiAnimation > 0) return;
        StartCoroutine(PlayLevelRoutine(LevelProfile.main.level));
	}

	// Starting a new game session
	public void StartSession(FieldTarget sessionType, Limitation limitationType) {
		StopAllCoroutines (); // Ending of all current coroutines

        GameCamera.main.transform.position = new Vector3(0, 10, -10);
        GameCamera.cam.orthographicSize = 5;

        isPlaying = true;

        blockCountTotal = GameObject.FindObjectsOfType<Block>().Length;

		switch (limitationType) { // Start corresponding coroutine depending on the limiation mode
			case Limitation.Moves: StartCoroutine(MovesLimitation()); break;
			case Limitation.Time: StartCoroutine(TimeLimitation());break;
		}

		switch (sessionType) { // Start corresponding coroutine depending on the target level
            case FieldTarget.None: {
                    StartCoroutine(TargetSession(() => {
                        return true;
                    }));
                    break;
                }
            case FieldTarget.Jelly: {
                    jellyCountTotal = FindObjectsOfType<Jelly>().Length;
                    StartCoroutine(TargetSession(() => {
                        return FindObjectsOfType<Jelly>().Length == 0;
                    }));
                    break;
                }
            case FieldTarget.Block: {
                    StartCoroutine(TargetSession(() => {
                        return FindObjectsOfType<Block>().Length == 0;
                    }));
                    break;
                }
            case FieldTarget.Color: {
                    for (int i = 0; i < LevelProfile.main.countOfEachTargetCount.Length; i++)
                        countOfEachTargetCount[colorMask[i]] = LevelProfile.main.countOfEachTargetCount[i];
                    StartCoroutine(TargetSession(() => {
                        foreach (int c in countOfEachTargetCount)
                            if (c > 0)
                                return false;
                        return true;
                    }));
                    break;
                }
            case FieldTarget.SugarDrop: {
                    targetSugarDropsCount = 0;
                    creatingSugarDropsCount = LevelProfile.main.targetSugarDropsCount;
                    StartCoroutine(TargetSession(() => {
                        return targetSugarDropsCount >= LevelProfile.main.targetSugarDropsCount && GameObject.FindObjectsOfType<SugarChip>().Length == 0;
                    }));
                    break;
                }
            case FieldTarget.Jam: {
                    StartCoroutine(TargetSession(() => {
                        return Slot.all.Values.Count(x => Jam.GetType(x) != "") == Slot.all.Count;
                    }));
                    break;
                }
        }

		StartCoroutine (BaseSession()); // Base routine of game session
		StartCoroutine (ShowingHintRoutine()); // Coroutine display hints
		StartCoroutine (ShuffleRoutine()); // Coroutine of mixing chips at the lack moves
		StartCoroutine (FindingSolutionsRoutine()); // Coroutine of finding a solution and destruction of existing combinations of chips
		StartCoroutine (IllnessRoutine()); // Coroutine of Weeds logic

        GameCamera.main.ShowField();
        UIAssistant.main.ShowPage("Field");
    }

	IEnumerator BaseSession () {
        DebugPanel.Log("Status (Base)", "Session", "Began.");
		while (!limitationRoutineIsOver && !targetRoutineIsOver) {
			yield return 0;
        }

        DebugPanel.Log("Status (Base)", "Session", "Waiting is over.");

        // Checking the condition of losing
        if (!reachedTheTarget) {
            DebugPanel.Log("Status (Base)", "Session", "Session failed. Clearing.");
            yield return StartCoroutine(GameCamera.main.HideFieldRoutine());
			FieldAssistant.main.RemoveField();
			ShowLosePopup();
            DebugPanel.Log("Status (Base)", "Session", "Session failed. End.");
            yield break;
		}

        iteraction = false;

        DebugPanel.Log("Status (Base)", "Session", "Session completed. Waiting the cutscene.");

        DebugPanel.Log("Status (Base)", "Session", "Session completed. Target is reached.");

        yield return new WaitForSeconds(0.2f);
        UIAssistant.main.ShowPage("TargetIsReached");
        AudioAssistant.Shot("TargetIsReached");
        yield return StartCoroutine(Utils.WaitFor(() => CPanel.uiAnimation == 0, 0.4f));
        UIAssistant.main.ShowPage("Field");


        DebugPanel.Log("Status (Base)", "Session", "Session completed. Bonus matching.");

        // Conversion of the remaining moves into bombs and activating them
        yield return StartCoroutine(BurnLastMovesToPowerups());
		
		yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));

        DebugPanel.Log("Status (Base)", "Session", "Session completed. Clearing.");

        // Ending the session, showing win popup
        yield return StartCoroutine(GameCamera.main.HideFieldRoutine());
		FieldAssistant.main.RemoveField();
        StartCoroutine(YouWin());

        DebugPanel.Log("Status (Base)", "Session", "Session completed. End.");
    }

    IEnumerator TargetSession(System.Func<bool> success, System.Func<bool> failed = null) {
        reachedTheTarget = false;   
        yield return 0;
        DebugPanel.Log("Status (Target)", "Session", "Began.");

        int score_target = LevelProfile.main.target == FieldTarget.None ? LevelProfile.main.thirdStarScore : LevelProfile.main.firstStarScore;
        while (!outOfLimit && (score < score_target || !success.Invoke()) && (failed == null || !failed.Invoke())) {
            DebugPanel.Log("Status (Target)", "Session", "Playing.");
            yield return new WaitForSeconds(0.33f);
            if (GetResource() == 0 && score >= LevelProfile.main.firstStarScore && success.Invoke() && (failed == null || !failed.Invoke()))
                reachedTheTarget = true;
        }

        if (score >= LevelProfile.main.firstStarScore && success.Invoke() && (failed == null || !failed.Invoke()))
            reachedTheTarget = true;

        targetRoutineIsOver = true;

        DebugPanel.Log("Status (Target)", "Session", "Complete.");
    }

	#region Limitation Modes Logic	

	// Game session with limited time
	IEnumerator TimeLimitation() {
        DebugPanel.Log("Status (Limitation)", "Session", "Began.");

        outOfLimit = false;

		// Waiting until the rules of the game are carried out
		while (timeLeft > 0 && !targetRoutineIsOver) {
            DebugPanel.Log("Status (Limitation)", "Session", "Began.");
            if (Time.timeScale == 1)
                timeLeft -= 1f;
            timeLeft = Mathf.Max(timeLeft, 0);
            if (timeLeft <= 5)
                AudioAssistant.Shot("TimeWarrning");
            yield return new WaitForSeconds(1f);

            if (timeLeft <= 0) {
                DebugPanel.Log("Status (Limitation)", "Session", "Out of limit. Waiting for destoying.");
                do
                    yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
                while (FindObjectsOfType<Chip>().ToList().Find(x => x.destroying) != null);
                if (!reachedTheTarget) {
                    UIAssistant.main.ShowPage("NoMoreMoves");
                    AudioAssistant.Shot("NoMoreMoves");
                    wait = true;
                    // Pending the decision of the player - lose or purchase additional time
                    DebugPanel.Log("Status (Limitation)", "Session", "Out of limit. Extra resources offer.");
                    while (wait)
                        yield return new WaitForSeconds(0.5f);

                }
			}
		}

        DebugPanel.Log("Status (Limitation)", "Session", "Waiting is over.");

        yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));

		if (timeLeft <= 0) outOfLimit = true;

		limitationRoutineIsOver = true;

        DebugPanel.Log("Status (Limitation)", "Session", "End. Out of limit: " + outOfLimit);
    }

	// Game session with limited count of moves
	IEnumerator MovesLimitation() {
        DebugPanel.Log("Status (Limitation)", "Session", "Began.");
        outOfLimit = false;
		
		// Waiting until the rules of the game are carried out
        while (movesCount > 0) {
            DebugPanel.Log("Status (Limitation)", "Session", "Began.");
            yield return new WaitForSeconds(1f);
            if (movesCount <= 0) {
                DebugPanel.Log("Status (Limitation)", "Session", "Out of limit. Waiting for destoying.");
                do
				    yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
                while (FindObjectsOfType<Chip>().ToList().Find(x => x.destroying) != null);
                if (!reachedTheTarget) {
                    UIAssistant.main.ShowPage("NoMoreMoves");
                    AudioAssistant.Shot("NoMoreMoves");
                    wait = true;
                    // Pending the decision of the player - lose or purchase additional time
                    DebugPanel.Log("Status (Limitation)", "Session", "Out of limit. Extra resources offer.");
                    while (wait)
                        yield return new WaitForSeconds(0.5f);

                }
			}
        }

        DebugPanel.Log("Status (Limitation)", "Session", "Waiting is over.");

        yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
		
		outOfLimit = true;
		limitationRoutineIsOver = true;

        DebugPanel.Log("Status (Limitation)", "Session", "End. Out of limit: " + outOfLimit);
    }

    #endregion

    // Coroutine of searching solutions and the destruction of existing combinations
    IEnumerator FindingSolutionsRoutine () {
		List<Solution> solutions;
        int id = 0;

		while (true) {
            if (isPlaying) {

                yield return StartCoroutine(Utils.WaitFor(() => lastMovementId > id, 0.2f));

                id = lastMovementId;
                solutions = FindSolutions();
                if (solutions.Count > 0) {
                    matchCount++;
                    MatchSolutions(solutions);
                } else
                    yield return StartCoroutine(Utils.WaitFor(() => {
                        return Chip.busyList.Count == 0;
                    }, 0.1f));
            } else
                yield return 0;
		}
	}

	// Coroutine of conversion of the remaining moves into bombs and activating them
	IEnumerator BurnLastMovesToPowerups ()
	{
		yield return StartCoroutine(CollapseAllPowerups ());

		int newBombs = 0;
		switch (LevelProfile.main.limitation) {
			case Limitation.Moves: newBombs = movesCount; break;
			case Limitation.Time: newBombs = Mathf.CeilToInt(timeLeft / 3); break;
		}

		int count;
		while (newBombs > 0) {
			count = Mathf.Min(newBombs, 8);
			while (count > 0) {
				count --;
				newBombs --;
				movesCount --;
				timeLeft -= 3;
                timeLeft = Mathf.Max(timeLeft, 0);
				switch (Random.Range(0, 2)) {
				    case 0: FieldAssistant.main.AddPowerup("SimpleBomb"); break;
				    case 1: FieldAssistant.main.AddPowerup("CrossBomb"); break;
				}
				yield return new WaitForSeconds(0.1f);
			}
            yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
			yield return StartCoroutine(CollapseAllPowerups ());
		}
	}

	// Weeds logic
	IEnumerator IllnessRoutine () {
        Weed.lastWeedCrush = swapEvent;
		Weed.seed = 0;

        int last_swapEvent = swapEvent;

		yield return new WaitForSeconds(1f);

        while (Weed.all.Count > 0) {
            yield return StartCoroutine(Utils.WaitFor(() => swapEvent > last_swapEvent, 0.1f));
            last_swapEvent = swapEvent;
            yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.1f));
            if (Weed.lastWeedCrush < swapEvent) {
                Weed.seed += swapEvent - Weed.lastWeedCrush;
                Weed.lastWeedCrush = swapEvent;
            }
            Weed.Grow();
		}
	}

    // Ending the session at user
    public void Quit() {
		StopAllCoroutines ();
		StartCoroutine(QuitCoroutine());
	}

	// Coroutine of ending the session at user
	IEnumerator QuitCoroutine() {
        while (CPanel.uiAnimation > 0)
            yield return 0;

        isPlaying = false;
        
        if (GameCamera.main.playing) {
            UIAssistant.main.ShowPage("Field");

		    yield return StartCoroutine(GameCamera.main.HideFieldRoutine());
        
		    FieldAssistant.main.RemoveField();
            
            while (CPanel.uiAnimation > 0)
                yield return 0;
        }

        UIAssistant.main.ShowPage("Loading");

        while (CPanel.uiAnimation > 0)
            yield return 0;

        yield return new WaitForSeconds(0.5f);
        UIAssistant.main.ShowPage("LevelList");
	}

	// Coroutine of activation all bombs in playing field
	IEnumerator CollapseAllPowerups () {
		yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
		List<Chip> powerUp = FindPowerups ();
		while (powerUp.Count > 0) {
            powerUp = powerUp.FindAll(x => !x.destroying);
            if (powerUp.Count > 0) {
			    EventCounter();
                Chip pu = powerUp[Random.Range(0, powerUp.Count)];
                pu.jamType = Jam.GetType(pu.slot);
                pu.DestroyChip();
            }
			yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
			powerUp = FindPowerups ();
		}
		yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
	}

	// Finding bomb function
	List<Chip> FindPowerups ()
	{
		return FindObjectsOfType<IBomb>().Select(x => x.GetComponent<Chip>()).ToList();
    }

	// Showing lose popup
	void ShowLosePopup ()
	{
		AudioAssistant.Shot ("YouLose");
        isPlaying = false;
        GameCamera.main.HideField();
        UIAssistant.main.ShowPage("YouLose");
	}

	// Showing win popup
	IEnumerator YouWin ()
	{
		AudioAssistant.Shot ("YouWin");
        PlayerPrefs.SetInt("FirstPass", 1);
		isPlaying = false;

        ProfileAssistant.main.local_profile["life"]++;
   
        if (ProfileAssistant.main.local_profile.current_level == LevelProfile.main.level)
            if (Level.all.ContainsKey(ProfileAssistant.main.local_profile.current_level + 1))
                ProfileAssistant.main.local_profile.current_level++;
        
        ProfileAssistant.main.local_profile.SetScore(LevelProfile.main.level, score);

        GameCamera.main.HideField();
        
        yield return 0;

        while (CPanel.uiAnimation > 0)
            yield return 0;

        yield return 0;

        UIAssistant.main.ShowPage("YouWin");
        
        UserProfileUtils.WriteProfileOnDevice(ProfileAssistant.main.local_profile);
    }

    public void ShowYouWinPopup() {
        bool bestScore = false;

        if (ProfileAssistant.main.local_profile.GetScore(LevelProfile.main.level) < score) {
            ProfileAssistant.main.local_profile.SetScore(LevelProfile.main.level, score);
            bestScore = true;
        }

        UIAssistant.main.ShowPage(bestScore ? "YouWinBestScore" : "YouWin");
    }

	// Conditions for waiting player's actions
	public bool CanIWait (){
        return isPlaying && Chip.busyList.Count == 0;
	}

	void  AddSolution ( Solution s  ){
		solutions.Add(s);
	}

	// Event counter
	public void  EventCounter (){
		eventCount ++;
	}

    // Search function possible moves
    public List<Move> FindMoves() {
        List<Move> moves = new List<Move>();
        if (!FieldAssistant.main.gameObject.activeSelf)
            return moves;
        if (LevelProfile.main == null)
            return moves;

        Solution solution;
        int potential;

        Side[] asixes = new Side[2] { Side.Right, Side.Top };

        foreach (Side asix in asixes) {
            foreach (Slot slot in Slot.all.Values) {
                if (slot[asix] == null)
                    continue;
                if (slot.block != null || slot[asix].block != null)
                    continue;
                if (slot.chip == null || slot[asix].chip == null)
                    continue;
                if (slot.chip.id == slot[asix].chip.id)
                    continue;

                Move move = new Move();
                move.from = slot.coord;
                move.to = slot[asix].coord;
                AnalizSwap(move);

                Dictionary<Slot, Solution> solutions = new Dictionary<Slot, Solution>();

                Slot[] cslots = new Slot[2] { slot, slot[asix] };
                foreach (Slot cslot in cslots) {
                    solutions.Add(cslot, null);

                    potential = 0;
                    solution = MatchAnaliz(cslot);
                    if (solution != null) {
                        solutions[cslot] = solution;
                        potential = solution.potential;
                    }

                    solution = MatchSquareAnaliz(cslot);
                    if (solution != null && potential < solution.potential) {
                        solutions[cslot] = solution;
                        potential = solution.potential;
                    }

                    move.potencial += potential;
                }

                if (solutions[cslots[0]] != null && solutions[cslots[1]] != null)
                    move.solution = solutions[cslots[0]].potential > solutions[cslots[1]].potential ? solutions[cslots[0]] : solutions[cslots[1]];
                else
                    move.solution = solutions[cslots[0]] != null ? solutions[cslots[0]] : solutions[cslots[1]];

                AnalizSwap(move);

                if (Mix.ContainsThisMix(slot.chip.chipType, slot[asix].chip.chipType))
                    move.potencial += 100;
                if (move.potencial > 0)
                    moves.Add(move);
            }
        }

        return moves;
    }

    // change places 2 chips in accordance with the move for the analysis of the potential of this move
	void  AnalizSwap (Move move){
		Slot slot;
		Chip fChip = Slot.GetSlot(move.from).chip;
		Chip tChip = Slot.GetSlot(move.to).chip;
		if (!fChip || !tChip) return;
		slot = tChip.slot;
		fChip.slot.chip  = tChip;
		slot.chip = fChip;
	}

    void MatchSolutions(List<Solution> solutions) {
        if (!isPlaying) return;
        solutions.Sort(delegate(Solution x, Solution y) {
            if (x.potential == y.potential)
                return 0;
            else if (x.potential > y.potential)
                return -1;
            else
                return 1;
        });

        int width = LevelProfile.main.width;
        int height = LevelProfile.main.height;
        
        bool[,] mask = new bool[width,height];
        int2 key = new int2();
        Slot slot;

        for (key.x = 0; key.x < width; key.x++) 
            for (key.y = 0; key.y < height; key.y++) {
                mask[key.x, key.y] = false;
                if (Slot.all.ContainsKey(key)) {
                    slot = Slot.all[key];
                    if (slot.chip)
                        mask[key.x, key.y] = true;
                }
            }

        List<Solution> final_solutions = new List<Solution>();

        bool breaker;
        foreach (Solution s in solutions) {
            breaker = false;
            foreach (Chip c in s.chips) {
                if (!mask[c.slot.x, c.slot.y]) {
                    breaker = true;
                    break;
                }
            }
            if (breaker)
                continue;

            final_solutions.Add(s);

            foreach (Chip c in s.chips)
                mask[c.slot.x, c.slot.y] = false;
        }

        foreach (Solution solution in final_solutions) {
            EventCounter();
        
            int puID = -1;

            string targetJam = "";
            if (LevelProfile.main.target == FieldTarget.Jam) {
                if (solution.chips.Count(x => Jam.GetType(x.slot) == "Jam A") > 0)
                    targetJam = "Jam A";
            }
            
            if (solution.chips.Count(x => !x.IsMatcheble()) == 0) {
                foreach (Chip chip in solution.chips) {
                    if (chip.id == solution.id || chip.IsUniversalColor()) {
                        if (!chip.slot)
                            continue;

                        slot = chip.slot;

                        if (chip.movementID > puID)
                            puID = chip.movementID;
                        chip.SetScore(Mathf.Pow(2, solution.count - 3) / solution.count);
                        if (!slot.block)
                            FieldAssistant.main.BlockCrush(slot.coord, true);
                        chip.jamType = targetJam;
                        chip.DestroyChip();
                        if (slot.jelly)
                            slot.jelly.JellyCrush();
                    }
                }
            } else
                return;

            solution.chips.Sort(delegate(Chip a, Chip b) {
                return a.movementID > b.movementID ? -1 : a.movementID == b.movementID ? 0 : 1;
            });

            breaker = false;
            foreach (Combinations combination in combinations) {
                if (combination.square && !solution.q)
                    continue;
                if (!combination.square) {
                    if (combination.horizontal && !solution.h)
                        continue;
                    if (combination.vertical && !solution.v)
                        continue;
                    if (combination.minCount > solution.count)
                        continue;
                }

                foreach (Chip ch in solution.chips)
                    if (ch.chipType == "SimpleChip") {
                        FieldAssistant.main.GetNewBomb(ch.slot.coord, combination.chip, ch.slot.transform.position, solution.id);
                        breaker = true;
                        break;
                    }
                if (breaker)
                    break;
            }
        }
    }
	
	public int GetMovementID (){
		lastMovementId ++;
		return lastMovementId;
	}
	
	public int GetMovesCount (){
		return movesCount;
	}

    public float GetResource() {
        switch (LevelProfile.main.limitation) {
            case Limitation.Moves:
                return 1f * movesCount / LevelProfile.main.limit;
            case Limitation.Time:
                return 1f * timeLeft / LevelProfile.main.limit;
        }
        return 1f;
    }

	// Coroutine of call mixing chips in the absence of moves
	IEnumerator ShuffleRoutine () {
		int shuffleOrder = 0;
		float delay = 1;
		while (true) {
			yield return StartCoroutine(Utils.WaitFor(CanIWait, delay));
			if (eventCount > shuffleOrder && !targetRoutineIsOver && Chip.busyList.Count == 0) {
				shuffleOrder = eventCount;
				yield return StartCoroutine(Shuffle(false));
			}
		}
	}


    void RawShuffle(List<Slot> slots) {
        EventCounter();
        int targetID;
        for (int j = 0; j < slots.Count; j++) {
            targetID = Random.Range(0, j - 1);
            if (!slots[j].chip || !slots[targetID].chip)
                continue;
            if (slots[j].chip.chipType == "Sugar" || slots[targetID].chip.chipType == "Sugar")
                continue;
            Swap(slots[j].chip, slots[targetID].chip);
        }
    }


	// Coroutine of mixing chips
	public IEnumerator Shuffle (bool f) {
		bool force = f;

		List<Move> moves = FindMoves();
		if (moves.Count > 0 && !force)
			yield break;
		if (!isPlaying)
			yield break;

		isPlaying = false;

        List<Slot> slots = new List<Slot>(Slot.all.Values);
        
		Dictionary<Slot, Vector3> positions = new Dictionary<Slot, Vector3> ();
        foreach (Slot slot in slots)
			positions.Add (slot, slot.transform.position);

        float t = 0;
        while (t < 1) {
            t += Time.unscaledDeltaTime * 3;
            Slot.folder.transform.localScale = Vector3.one * Mathf.Lerp(1, 0.6f, EasingFunctions.easeInOutQuad(t));
            Slot.folder.transform.eulerAngles = Vector3.forward * Mathf.Lerp(0, Mathf.Sin(Time.unscaledTime * 40) * 3, EasingFunctions.easeInOutQuad(t));

            yield return 0;
        }


        if (f || moves.Count == 0) {
            f = false;
            RawShuffle(slots);
        }

        moves = FindMoves();
		List<Solution> solutions = FindSolutions ();

        int itrn = 0;
        int targetID;
        while (solutions.Count > 0 || moves.Count == 0) {
            if (itrn > 100) {
                ShowLosePopup();
                yield break;
            }
            if (solutions.Count > 0) {
                for (int s = 0; s < solutions.Count; s++) {
                    targetID = Random.Range(0, slots.Count - 1);
                    if (slots[targetID].chip && slots[targetID].chip.chipType != "Sugar" && slots[targetID].chip.id != solutions[s].id)
                        Swap(solutions[s].chips[Random.Range(0, solutions[s].chips.Count - 1)], slots[targetID].chip);
                }
            } else 
                RawShuffle(slots);

            moves = FindMoves();
            solutions = FindSolutions();
            itrn++;
            Slot.folder.transform.eulerAngles = Vector3.forward * Mathf.Sin(Time.unscaledTime * 40) * 3;

            yield return 0;
        }

        t = 0;
        AudioAssistant.Shot("Shuffle");
        while (t < 1) {
            t += Time.unscaledDeltaTime * 3;
            Slot.folder.transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1, EasingFunctions.easeInOutQuad(t));
            Slot.folder.transform.eulerAngles = Vector3.forward * Mathf.Lerp(Mathf.Sin(Time.unscaledTime * 40) * 3, 0, EasingFunctions.easeInOutQuad(t));
            yield return 0;
        }

        Slot.folder.transform.localScale = Vector3.one;
        Slot.folder.transform.eulerAngles = Vector3.zero;

		isPlaying = true;
	}

	// Function of searching possible solutions
	List<Solution> FindSolutions() {
		List<Solution> solutions = new List<Solution> ();
		Solution zsolution;
		foreach(Slot slot in Slot.all.Values) {
			zsolution = MatchAnaliz(slot);
			if (zsolution != null) solutions.Add(zsolution);
			zsolution = MatchSquareAnaliz(slot);
			if (zsolution != null) solutions.Add(zsolution);
		}
		return solutions;
	}

	// Coroutine of showing hints
	IEnumerator ShowingHintRoutine () {
		int hintOrder = 0;
		float delay = 5;

        yield return new WaitForSeconds(1f);

        while (!reachedTheTarget) {
            while (!isPlaying)
                yield return 0;
			yield return StartCoroutine(Utils.WaitFor(CanIWait, delay));
			if (eventCount > hintOrder) {
				hintOrder = eventCount;
				ShowHint();
			}
		}
	}

    // Analysis of chip for combination
    public Solution MatchAnaliz(Slot slot) {

        if (!slot.chip)
            return null;
        if (!slot.chip.IsMatcheble())
            return null;


        if (slot.chip.IsUniversalColor()) { // multicolor
            List<Solution> solutions = new List<Solution>();
            Solution z;
            Chip multicolorChip = slot.chip;
            for (int i = 0; i < 6; i++) {
                multicolorChip.id = i;
                z = MatchAnaliz(slot);
                if (z != null)
                    solutions.Add(z);
                z = MatchSquareAnaliz(slot);
                if (z != null)
                    solutions.Add(z);
            }
            multicolorChip.id = Chip.universalColorId;
            z = null;
            foreach (Solution sol in solutions)
                if (z == null || z.potential < sol.potential)
                    z = sol;
            return z;
        }

        Slot s;
        Dictionary<Side, List<Chip>> sides = new Dictionary<Side, List<Chip>>();
        int count;
        int2 key;
        foreach (Side side in Utils.straightSides) {
            count = 1;
            sides.Add(side, new List<Chip>());
            while (true) {
                key = slot.coord + Utils.SideOffset(side) * count;
                if (!Slot.all.ContainsKey(key))
                    break;
                s = Slot.all[key];
                if (!s.chip)
                    break;
                if (s.chip.id != slot.chip.id && !s.chip.IsUniversalColor())
                    break;
                if (!s.chip.IsMatcheble())
                    break;
                sides[side].Add(s.chip);
                count++;
            }
        }

        bool h = sides[Side.Right].Count + sides[Side.Left].Count >= 2;
        bool v = sides[Side.Top].Count + sides[Side.Bottom].Count >= 2;

        if (h || v) {
            Solution solution = new Solution();

            solution.h = h;
            solution.v = v;

            solution.chips = new List<Chip>();
            solution.chips.Add(slot.chip);

            if (h) {
                solution.chips.AddRange(sides[Side.Right]);
                solution.chips.AddRange(sides[Side.Left]);
            }
            if (v) {
                solution.chips.AddRange(sides[Side.Top]);
                solution.chips.AddRange(sides[Side.Bottom]);
            }

            solution.count = solution.chips.Count;

            solution.x = slot.x;
            solution.y = slot.y;
            solution.id = slot.chip.id;

            foreach (Chip c in solution.chips)
                solution.potential += c.GetPotencial();

            return solution;
        }
        return null;
    }

    public Solution MatchSquareAnaliz(Slot slot) {

        if (!main.squareCombination)
            return null;
        if (!slot.chip)
            return null;
        if (!slot.chip.IsMatcheble())
            return null;


        if (slot.chip.IsUniversalColor()) { // multicolor
            List<Solution> solutions = new List<Solution>();
            Solution z;
            Chip multicolorChip = slot.chip;
            for (int i = 0; i < 6; i++) {
                multicolorChip.id = i;
                z = MatchSquareAnaliz(slot);
                if (z != null)
                    solutions.Add(z);
            }
            multicolorChip.id = Chip.universalColorId;
            z = null;
            foreach (Solution sol in solutions)
                if (z == null || z.potential < sol.potential)
                    z = sol;
            return z;
        }

        List<Chip> square = new List<Chip>();
        List<Chip> buffer = new List<Chip>();
        Side sideR;
        int2 key;
        Slot s;


        buffer.Clear();
        foreach (Side side in Utils.straightSides) {
            for (int r = 0; r <= 2; r++) {
                sideR = Utils.RotateSide(side, r);
                key = slot.coord + Utils.SideOffset(sideR);
                if (Slot.all.ContainsKey(key)) {
                    s = Slot.all[key];
                    if (s.chip && (s.chip.id == slot.chip.id || s.chip.IsUniversalColor()) && s.chip.IsMatcheble())
                        buffer.Add(s.chip);
                    else
                        break;
                } else
                    break;
            }
            if (buffer.Count == 3) {
                foreach (Chip chip_b in buffer)
                    if (!square.Contains(chip_b))
                        square.Add(chip_b);
            }
            buffer.Clear();
        }


        bool q = square.Count >= 3;

        if (q) {
            Solution solution = new Solution();

            solution.q = q;

            solution.chips = new List<Chip>();
            solution.chips.Add(slot.chip);

            solution.chips.AddRange(square);

            solution.count = solution.chips.Count;

            solution.x = slot.x;
            solution.y = slot.y;
            solution.id = slot.chip.id;

            foreach (Chip c in solution.chips)
                solution.potential += c.GetPotencial();

            return solution;
        }
        return null;
    }

    #region Swapping
    // Temporary Variables
    bool swaping = false; // ÈTRUE when the animation plays swapping 2 chips
    public bool iteraction = false;

    // Function immediate swapping 2 chips
    public void Swap(Chip a, Chip b) {
        if (!a || !b)
            return;
        if (a == b)
            return;
        if (a.slot.block || b.slot.block)
            return;

        a.movementID = GetMovementID();
        b.movementID = GetMovementID();

        Slot slotA = a.slot;
        Slot slotB = b.slot;

        slotB.chip = a;
        slotA.chip = b;
    }

    // The function of swapping 2 chips by player
    public void SwapByPlayer(Chip a, Chip b, bool onlyForMatching, bool byAI = false) {
        StartCoroutine(SwapByPlayerRoutine(a, b, onlyForMatching, byAI)); // Starting corresponding coroutine
    }

    public void SwapByPlayer(Move move, bool onlyForMatching, bool byAI = false) {
        Chip a = Slot.all[move.from].chip;
        Chip b = Slot.all[move.to].chip;
        if (a && b)
            SwapByPlayer(a, b, onlyForMatching, byAI);
    }

    // Coroutine swapping 2 chips
    IEnumerator SwapByPlayerRoutine(Chip a, Chip b, bool onlyForMatching, bool byAI = false) {
        if (!isPlaying)
            yield break;
        if (!iteraction && !byAI)
            yield break;
        // cancellation terms
        if (swaping)
            yield break; // If the process is already running
        if (!a || !b)
            yield break; // If one of the chips is missing
        if (a.destroying || b.destroying)
            yield break;
        if (a.busy || b.busy) 
            yield break; // If one of the chips is busy
        if (a.slot.block || b.slot.block)
            yield break; // If one of the chips is blocked

        switch (LevelProfile.main.limitation) {
            case Limitation.Moves:
                if (movesCount <= 0)
                    yield break;
                break; // If not enough moves
            case Limitation.Time:
                if (main.timeLeft <= 0)
                    yield break;
                break; // If not enough time
        }

        Mix mix = mixes.Find(x => x.Compare(a.chipType, b.chipType));

        int move = 0; // Number of points movement which will be expend

        swaping = true;

        Vector3 posA = a.slot.transform.position;
        Vector3 posB = b.slot.transform.position;

        float progress = 0;

        Vector3 normal = a.slot.x == b.slot.x ? Vector3.right : Vector3.up;

        float time = 0;

        a.busy = true;
        b.busy = true;

        // Animation swapping 2 chips
        while (progress < ProjectParameters.main.swap_duration) {
            time = EasingFunctions.easeInOutQuad(progress / ProjectParameters.main.swap_duration);
            a.transform.position = Vector3.Lerp(posA, posB, time) + normal * Mathf.Sin(3.14f * time) * 0.2f;
            if (mix == null)
                b.transform.position = Vector3.Lerp(posB, posA, time) - normal * Mathf.Sin(3.14f * time) * 0.2f;

            progress += Time.deltaTime;

            yield return 0;
        }

        a.transform.position = posB;
        if (mix == null)
            b.transform.position = posA;

        a.movementID = main.GetMovementID();
        b.movementID = main.GetMovementID();

        if (mix != null) { // Scenario mix effect
            swaping = false;
            a.busy = false;
            b.busy = false;
            MixChips(a, b);
            yield return new WaitForSeconds(0.3f);
            movesCount--;
            swapEvent++;
            yield break;
        }

        // Scenario the effect of swapping two chips
        Slot slotA = a.slot;
        Slot slotB = b.slot;

        slotB.chip = a;
        slotA.chip = b;


        move++;

        // searching for solutions of matching
        int count = 0;
        Solution solution;

        solution = MatchAnaliz(slotA);
        if (solution != null)
            count += solution.count;

        solution = MatchSquareAnaliz(slotA);
        if (solution != null)
            count += solution.count;

        solution = MatchAnaliz(slotB);
        if (solution != null)
            count += solution.count;

        solution = MatchSquareAnaliz(slotB);
        if (solution != null)
            count += solution.count;

        // Scenario canceling of changing places of chips
        if (count == 0 && !onlyForMatching) {
            AudioAssistant.Shot("SwapFailed");
            while (progress > 0) {
                time = EasingFunctions.easeInOutQuad(progress / ProjectParameters.main.swap_duration);
                a.transform.position = Vector3.Lerp(posA, posB, time) - normal * Mathf.Sin(3.14f * time) * 0.2f;
                b.transform.position = Vector3.Lerp(posB, posA, time) + normal * Mathf.Sin(3.14f * time) * 0.2f;

                progress -= Time.deltaTime;

                yield return 0;
            }

            a.transform.position = posA;
            b.transform.position = posB;

            a.movementID = GetMovementID();
            b.movementID = GetMovementID();

            slotB.chip = b;
            slotA.chip = a;

            move--;
        } else {
            AudioAssistant.Shot("SwapSuccess");
            swapEvent++;
        }

        firstChipGeneration = false;

        if (!byAI)
            movesCount -= move;
        EventCounter();

        a.busy = false;
        b.busy = false;
        swaping = false;
    }
    #endregion

    // Showing random hint
    void  ShowHint (){
		if (!isPlaying) return;
		List<Move> moves = FindMoves();

        foreach (Move move in moves) {
            Debug.DrawLine(Slot.GetSlot(move.from).transform.position, Slot.GetSlot(move.to).transform.position, Color.red, 10);
        
        }


		if (moves.Count == 0) return;

		Move bestMove = moves[ Random.Range(0, moves.Count) ];

		if (bestMove.solution == null) return;

        foreach (Chip chip in bestMove.solution.chips)
            chip.Flashing(eventCount);
	}

    [System.Serializable]
    public class ChipInfo {
        public string name = "";
        public string contentName = "";
        public bool color = true;
        public string shirtName = "";
    }

    [System.Serializable]
    public class BlockInfo {
        public string name = "";
        public string contentName = "";
        public string shirtName = "";
        public int levelCount = 0;
        public bool chip = false;
    }

    [System.Serializable]
    public class Combinations {
        public int priority = 0;
        public string chip;
        public bool horizontal = true;
        public bool vertical = true;
        public bool square = false;
        public int minCount = 4;

    }

	// Class with information of solution
	public class Solution {
		//   T
		//   T
		// LLXRR  X - center of solution
		//   B
		//   B

		public int count; // count of chip combination (count = T + L + R + B + X)
		public int potential; // potential of solution
		public int id; // ID of chip color
        public List<Chip> chips = new List<Chip>();

		// center of solution
		public int x;
		public int y;

		public bool v; // is this solution is vertical?  (v = L + R + X >= 3)
		public bool h; // is this solution is horizontal? (h = T + B + X >= 3)
        public bool q;
        //public int posV; // number on right chips (posV = R)
        //public int negV; // number on left chips (negV = L)
        //public int posH; // number on top chips (posH = T)
        //public int negH; // number on bottom chips (negH = B)
	}

	// Class with information of move
	public class Move {
        //
        // A -> B
        //

        // position of start chip (A)
        public int2 from;
        // position of target chip (B)
        public int2 to;

        public Solution solution; // solution of this move
		public int potencial; // potential of this move
	}

    [System.Serializable]
    public class Mix {
        public Pair pair = new Pair("", "");

        public string function;

        public bool Compare(string _a, string _b) {
            return pair == new Pair(_a, _b);
        }

        public static bool ContainsThisMix(string _a, string _b) {
            return main.mixes.Find(x => x.Compare(_a, _b)) != null;
        }

        public static Mix FindMix(string _a, string _b) {
            return main.mixes.Find(x => x.Compare(_a, _b));
        }
    }
}
