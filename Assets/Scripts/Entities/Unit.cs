using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;


public enum UnitOrder
{
    None,
    Move,
    Attack,
    Capture,
    Repair,
}


public class Unit : BaseEntity
{
    [SerializeField]
    UnitDataScriptable UnitData = null;

    Transform BulletSlot;
    float LastActionDate = 0f;
    Vector3 PositionTarget = Vector3.zero;
    
    NavMeshAgent NavMeshAgent;

    public TargetBuilding CaptureTarget = null;
    public BaseEntity NextTarget = null;

    public UnitOrder CurrentOrder = UnitOrder.None;

    public UnitDataScriptable GetUnitData { get { return UnitData; } }
    public int Cost { get { return UnitData.Cost; } }
    public int GetTypeId { get { return UnitData.TypeId; } }

    public NavMeshAgent GetNavMeshAgent() { return NavMeshAgent; }
    public Vector3 GetPositionTarget() { return PositionTarget; }


    public Squad CurrentSquad { get; set; }
    override public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        base.Init(_team);

        HP = UnitData.MaxHP;
        OnDeadEvent += Unit_OnDead;
    }
    void Unit_OnDead()
    {
        if (IsCapturing())
            StopCapture();

        if (GetUnitData.DeathFXPrefab)
        {
            GameObject fx = Instantiate(GetUnitData.DeathFXPrefab, transform);
            fx.transform.parent = null;
        }

        Destroy(gameObject);
    }
    #region MonoBehaviour methods
    override protected void Awake()
    {
        base.Awake();

        NavMeshAgent = GetComponent<NavMeshAgent>();
        BulletSlot = transform.Find("BulletSlot");

        // fill NavMeshAgent parameters
        NavMeshAgent.speed = GetUnitData.Speed;
        NavMeshAgent.angularSpeed = GetUnitData.AngularSpeed;
        NavMeshAgent.acceleration = GetUnitData.Acceleration;
    }
    override protected void Start()
    {
        // Needed for non factory spawned units (debug)
        if (!IsInitialized)
            Init(Team);

        base.Start();
    }
    override protected void Update()
    {
        CheckUnitsInRange();
	}
    #endregion

    #region IRepairable
    override public bool NeedsRepairing()
    {
        return HP < GetUnitData.MaxHP;
    }
    override public void Repair(int amount)
    {
        HP = Mathf.Min(HP + amount, GetUnitData.MaxHP);
        base.Repair(amount);
    }
    override public void FullRepair()
    {
        Repair(GetUnitData.MaxHP);
    }
    #endregion



    //Utils
    #region Utils
    public void CheckUnitsInRange()
    {
        var enemies = GameServices.GetControllerByTeam(GameServices.GetOpponent(GetTeam())).UnitList;
        foreach (Unit enemy in enemies)
        {
            if (!enemy.IsAlive || !CanAttack(enemy))
                continue;
            OrderAttack(enemy);
        }

        if (CurrentSquad != null)
        {
            foreach (Unit ally in CurrentSquad.Units)
            {
                if (ally == this || !ally.NeedsRepairing())
                    continue;
                if (!CanRepair(ally))
                    continue;

                OrderRepair(ally);
            }
        }
    }

    public bool HasReachedDestination()
    {
        if (NavMeshAgent == null || NavMeshAgent.pathPending)
            return false;

        return NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance;
    }

    public void StopCapture()
    {
        if (CaptureTarget == null)
            return;

        CaptureTarget.StopCapture(this);
        CaptureTarget = null;
    }

    public bool IsCapturing()
    {
        return CaptureTarget != null;
    }

    #endregion

    #region Order methods : Moving, Capturing, Targeting, Attacking, Repairing ...

    // Orders
    public void OrderMove(Vector3 pos)
    {
        NavMeshAgent.isStopped = false;
        NavMeshAgent.SetDestination(pos);    
    }

    public void OrderAttack(BaseEntity target)
    {
        NextTarget = target;
        CurrentOrder = UnitOrder.Attack;
    }

    public void OrderRepair(BaseEntity entity)
    {
        NextTarget = entity;
        CurrentOrder = UnitOrder.Repair;
    }

    public void OrderCapture(TargetBuilding target)
    {
        CaptureTarget = target;
        CurrentOrder = UnitOrder.Capture;
    }

    
    #endregion

    //Checks (used in the conditions)
    #region Checks

    public bool CanAttack(BaseEntity target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.AttackDistanceMax * GetUnitData.AttackDistanceMax)
            return false;

        return true;
    }
    public bool CanRepair(BaseEntity target)
    {
        if (GetUnitData.CanRepair == false || target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.RepairDistanceMax * GetUnitData.RepairDistanceMax)
            return false;

        return true;
    }

    public bool CanCapture(TargetBuilding target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.CaptureDistanceMax * GetUnitData.CaptureDistanceMax)
            return false;

        return true;
    }
    #endregion

    //Computes 
    #region Computes
    public void ComputeAttack()
    {
        BaseEntity ToAttack = NextTarget;
        if (CanAttack(ToAttack) == false)
            return;

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        transform.LookAt(ToAttack.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.AttackFrequency)
        {
            LastActionDate = Time.time;
            // visual only ?
            if (UnitData.BulletPrefab)
            {
                GameObject newBullet = Instantiate(UnitData.BulletPrefab, BulletSlot);
                newBullet.transform.parent = null;
                newBullet.GetComponent<Bullet>().ShootToward(ToAttack.transform.position - transform.position, this);
            }
            // apply damages
            int damages = Mathf.FloorToInt(UnitData.DPS * UnitData.AttackFrequency);
            ToAttack.AddDamage(damages);
        }
    }

    public void ComputeRepairing()
    {
        BaseEntity ToRepair = NextTarget;
        if (CanRepair(ToRepair) == false)
            return;

        if (NavMeshAgent)
            NavMeshAgent.isStopped = true;

        transform.LookAt(ToRepair.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.RepairFrequency)
        {
            LastActionDate = Time.time;

            // apply reparing
            int amount = Mathf.FloorToInt(UnitData.RPS * UnitData.RepairFrequency);
            ToRepair.Repair(amount);
        }
    }
    #endregion
}
