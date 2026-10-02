using UnityEngine;
using System.Collections.Generic;
using AI.InfluenceMap;
using AI.Planner;

public sealed class AIController : UnitController
{
    #region Fields

    private List<Action> _availableActions;
    private List<Goal>   _goals;

    private Goap       _goap;
    private WorldState _worldState;

    [Header("Influence Map")]
    [SerializeField] private ScoutingSubSystem _scoutingSubSystem;
    [SerializeField] private InfluenceMap _influenceMap;
    private AIContext _context;

    [Header("Factory Construction")]
    [SerializeField] private int targetFactoryCountPerType = 2;
    [SerializeField] private int reservedPointsForArmy = 10;
    [SerializeField] private float radiusConstruction = 40f;

    [Header("Unit Production")]
    [SerializeField] private SquadProduction _squadProduction; 

    private float _evalTimer;
    private const float EVAL_INTERVAL = 2.0f;
    private bool _gameOver;

    #endregion

    #region Unity Methods

    protected override void Awake()
    {
        base.Awake();
        _availableActions = new List<Action>(GetComponentsInChildren<Action>());
        _goals = new List<Goal>(GetComponentsInChildren<Goal>());
    }

    protected override void Start()
    {
        base.Start();
        _goap = new Goap(_availableActions);
        _worldState = new WorldState();
        _scoutingSubSystem.Setup(Team);

        _context = new AIContext
        {
            influenceMap       = _influenceMap,
            scoutingSubSystem  = _scoutingSubSystem,
            worldState         = _worldState,
            unitController     = this,
        };

        MainBase.OnDeadEvent += () => _gameOver = true;
        UnitController enemy = GameServices.GetControllerByTeam(GameServices.GetOpponent(Team));
        enemy.MainBase.OnDeadEvent += () => _gameOver = true;
    }

    protected override void Update()
    {
        base.Update();

        if (_gameOver) 
            return;

        _evalTimer += Time.deltaTime;
        if (_evalTimer >= EVAL_INTERVAL)
        {
            
            foreach (Goal goal in _goals)
                goal.RatePriority(_worldState, Team, _context);

            _goals.Sort((a, b) => b.priority.CompareTo(a.priority));

            ReallocateSquads();
            ContinuousProduction();
            DebugSquads();
            _evalTimer = 0f;
        }

        UpdateWorldState();
        UpdateSquadPlans();
        //TryExpandProduction();
    }

    #endregion

    #region Unit Production Methods

    public override void AddUnit(Unit unit)
    {
        unit.OnDeadEvent += () =>
        {
            Squad squad = unit.CurrentSquad;
            if (squad != null && squad.Count <= 1 && squad.assignedGoal != null)
                squad.assignedGoal.UnassignSquad(squad);
        };

        base.AddUnit(unit);
        AssignUnitToGoal(unit);
    }

    private void ContinuousProduction()
    {
        foreach (Factory factory in FactoryList)
        {
            if (factory.IsUnderConstruction)
                continue;

            int typeId = _squadProduction.GetMostNeededTypeId(factory, this);
            if (typeId < 0)
                continue;

            int unitIndex = factory.GetMenuIndexForTypeId(typeId);
            if (unitIndex >= 0)
                factory.RequestUnitBuild(unitIndex);
        }
    }

    #endregion

    #region Squad Allocation Methods

    /// <summary>
    /// Called when a unit is spawned. Finds the active goal with the largest deficit
    /// (target count - current count) and assigns the unit to it immediately,
    /// without waiting for the next ReallocateSquads cycle.
    /// </summary>
    private void AssignUnitToGoal(Unit unit)
    {
        List<Goal> activeGoals = GetActiveGoals();
        if (activeGoals.Count == 0)
            return;

        // The new unit is already in UnitList (base.AddUnit is called first)
        var targets = ComputeTargets(activeGoals, UnitList.Count);

        Goal bestGoal = null;
        int  biggestDeficit = 0;

        foreach (Goal goal in activeGoals)
        {
            int current = goal.GetAssignedSquad()?.Count ?? 0;
            int deficit = targets[goal] - current;
            if (deficit > biggestDeficit)
            {
                biggestDeficit = deficit;
                bestGoal = goal;
            }
        }

        if (bestGoal == null) 
            return;

        Squad squad = bestGoal.GetAssignedSquad();
        if (squad == null)
        {
            squad = new Squad();
            SquadList.Add(squad);
            squad.assignedGoal = bestGoal;
            bestGoal.AssignSquad(squad);

            List<Action> plan = _goap.BuildPlanForward(_worldState, bestGoal);
            if (plan != null) 
                squad.Plan = new Queue<Action>(plan);
        }

        squad.AddUnit(unit);
    }

    /// <summary>
    /// Redistributes all units based on current goal priorities (runs every EVAL_INTERVAL seconds).
    /// Never resets an active plan: only surplus units and free units are moved.
    /// Order: disband inactive squads → strip excess → collect unassigned → fill deficits → remove empty squads.
    /// </summary>
    private void ReallocateSquads()
    {
        List<Goal> activeGoals = GetActiveGoals();
        int totalUnits = UnitList.Count;

        // Disband squads whose goal is no longer active, collect freed units
        var unassigned = new List<Unit>();
        foreach (Squad squad in new List<Squad>(SquadList))
        {
            if (squad.assignedGoal == null || squad.assignedGoal.priority <= 0)
            {
                if (squad.assignedGoal != null) 
                    squad.assignedGoal.UnassignSquad(squad);
                
                squad.assignedGoal = null;
                
                foreach (Unit u in new List<Unit>(squad.Units)) 
                {
                    squad.RemoveUnit(u); 
                    unassigned.Add(u);
                }
                SquadList.Remove(squad);
            }
        }

        if (activeGoals.Count == 0)
            return;

        // Compute targets
        var targets = ComputeTargets(activeGoals, totalUnits);

        // Strip excess units from over-staffed squads into the unassigned pool
        foreach (Goal goal in activeGoals)
        {
            Squad squad = goal.GetAssignedSquad();
            if (squad == null) 
                continue;
            int excess = squad.Count - targets[goal];
            for (int i = 0; i < excess; i++)
            {
                Unit u = squad.Units[squad.Count - 1];
                squad.RemoveUnit(u);
                unassigned.Add(u);
            }
        }

        // Collect truly unassigned units (not in any squad)
        foreach (Unit u in UnitList)
            if (u.CurrentSquad == null && !unassigned.Contains(u))
                unassigned.Add(u);

        // Fill deficit goals from the unassigned pool
        foreach (Goal goal in activeGoals)
        {
            if (unassigned.Count == 0) 
                break;

            Squad squad = goal.GetAssignedSquad();
            if (squad == null)
            {
                squad = new Squad();
                SquadList.Add(squad);
                squad.assignedGoal = goal;
                goal.AssignSquad(squad);
            }

            int deficit = targets[goal] - squad.Count;
            for (int i = 0; i < deficit && unassigned.Count > 0; i++)
            {
                squad.AddUnit(unassigned[0]);
                unassigned.RemoveAt(0);
            }

            // Build plan only if squad has none (don't interrupt an ongoing plan)
            if (squad.Plan == null || squad.Plan.Count == 0)
            {
                List<Action> plan = _goap.BuildPlanForward(_worldState, goal);
                if (plan != null) 
                    squad.Plan = new Queue<Action>(plan);
            }
        }

        // Clean up empty squads
        foreach (Squad squad in new List<Squad>(SquadList))
        {
            if (squad.Count != 0) continue;
            if (squad.assignedGoal != null) squad.assignedGoal.UnassignSquad(squad);
            SquadList.Remove(squad);
        }
    }

    /// <summary>
    /// Computes the target unit count for each active goal, proportionally to its priority.
    /// Units are handed out one by one to the goal with the best score priority / (2 * assigned + 1)
    /// (Sainte-Laguë method), so the highest priority goal is always served first.
    /// MaxUnitCount is a cap, not a reservation: a capped goal stops receiving units and the
    /// rest go to the other goals. Units left over when every goal is capped stay unassigned.
    /// </summary>
    private Dictionary<Goal, int> ComputeTargets(List<Goal> activeGoals, int totalUnits)
    {
        var targets = new Dictionary<Goal, int>();
        foreach (Goal g in activeGoals)
            targets[g] = 0;

        for (int n = 0; n < totalUnits; n++)
        {
            Goal  bestGoal  = null;
            float bestScore = 0f;

            // activeGoals is sorted by priority (descending), so ties go to the higher priority goal
            foreach (Goal g in activeGoals)
            {
                if (g.MaxUnitCount > 0 && targets[g] >= g.MaxUnitCount)
                    continue;

                float score = g.priority / (2f * targets[g] + 1f);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestGoal  = g;
                }
            }

            if (bestGoal == null)
                break;

            targets[bestGoal]++;
        }

        return targets;
    }

    private List<Goal> GetActiveGoals()
    {
        var result = new List<Goal>();
        foreach (Goal g in _goals)
            if (g.priority > 0) 
                result.Add(g);
        return result;
    }

    #endregion

    #region Plan Execution Methods

    private void UpdateSquadPlans()
    {
        foreach (Squad squad in new List<Squad>(SquadList))
        {
            if (squad.Plan == null || squad.Plan.Count == 0) continue;

            Action current = squad.Plan.Peek();
            current.OnUpdate(squad, _context);

            if (current.Complete(squad, _context))
            {
                squad.Plan.Dequeue();
                if (squad.Plan.Count == 0)
                    Replan(squad);
            }
        }
    }

    private void Replan(Squad squad)
    {
        Goal goal = squad.assignedGoal;
        if (goal == null) return;

        goal.RatePriority(_worldState, Team, _context);
        if (goal.priority <= 0)
        {
            goal.UnassignSquad(squad);
            squad.assignedGoal = null;
            return;
        }

        List<Action> plan = _goap.BuildPlanForward(_worldState, goal);
        if (plan != null)
            squad.Plan = new Queue<Action>(plan);
    }

    #endregion

    #region World State Methods

    private void UpdateWorldState()
    {
        bool labLocated     = false;
        bool allLabsCaptured = true;
        foreach (TargetBuilding lab in GameServices.GetTargetBuildings())
        {
            if (lab.GetTeam() == Team) 
                continue;
            allLabsCaptured = false; 
            if (!_influenceMap.WasVisible(lab.transform.position, (int)Team)) 
                continue;
            labLocated = true;
        }
        _worldState.EnemyLabLocated       = labLocated;
        _worldState.AllEnemyLabsCaptured  = allLabsCaptured;

        bool labCaptured = false;
        foreach (TargetBuilding lab in GameServices.GetTargetBuildings())
        {
            if (lab.GetTeam() != Team) 
                continue;
            labCaptured = true;
            break;
        }
        _worldState.LabCaptured = labCaptured;

        bool labUnderThreat = false;
        foreach (TargetBuilding lab in GameServices.GetTargetBuildings())
        {
            if (lab.GetTeam() != Team)
                 continue;
            int inf = _influenceMap.GetSubMapInfluenceAtPosition<EnemyUnitInfluenceSubMap>(lab.transform.position);
            if (inf < 0) 
                labUnderThreat = true;
        }
        _worldState.LabUnderThreat = labUnderThreat;

        int baseInf = _influenceMap.GetSubMapInfluenceAtPosition<EnemyUnitInfluenceSubMap>(MainBase.transform.position);
        _worldState.BaseUnderThreat = baseInf < 0;

        UnitController enemy = GameServices.GetControllerByTeam(GameServices.GetOpponent(Team));
        _worldState.EnemyBaseLocated = _influenceMap.WasVisible(enemy.MainBase.transform.position, (int)Team);
        
        bool enemyFactoryLocated = false;
        foreach (Factory factory in enemy.GetFactoryList)
        {
            if (factory == enemy.MainBase) 
                continue;
            if (!factory.IsAlive) 
                continue;
            if (_influenceMap.WasVisible(factory.transform.position, (int)Team))
            {
                enemyFactoryLocated = true;
                break;
            }
        }
        _worldState.EnemyFactoryLocated = enemyFactoryLocated;
    }

    #endregion

    #region Debug Methods

    private void DebugSquads()
    {
        for (int i = 0; i < SquadList.Count; i++)
        {
            Squad  squad      = SquadList[i];
            string goalName   = squad.assignedGoal != null ? squad.assignedGoal.GetType().Name : "idle";
            string planAction = (squad.Plan != null && squad.Plan.Count > 0) ? squad.Plan.Peek().GetType().Name : "no plan";
            Debug.Log($"[AI {Team}] Squad {i} ({squad.Count} units) → {goalName} | action: {planAction}");
        }
    }

    #endregion
}