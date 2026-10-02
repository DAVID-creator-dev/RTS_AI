public class WorldState
{
    internal uint _flags;

    //structures
    internal const uint F_BASE_ALIVE = 1 << 0;
    internal const uint F_LAB_CAPTURED = 1 << 1;

    //Threats
    internal const uint F_LAB_UNDER_THREAT = 1 << 2;
    internal const uint F_FACTORY_UNDER_THREAT = 1 << 3;
    internal const uint F_BASE_UNDER_THREAT = 1 << 4;

    //Map
    internal const uint F_ENEMY_LAB_LOCATED = 1 << 5;
    internal const uint F_ENEMY_BASE_LOCATED = 1 << 6;
    internal const uint F_ENEMY_FACTORY_LOCATED = 1 << 7;

    //Win conditions
    internal const uint F_ENEMY_BASE_DESTROYED    = 1 << 9;
    internal const uint F_ENEMY_FACTORY_DESTROYED = 1 << 17;
    internal const uint F_ALL_ENEMY_LABS_CAPTURED = 1 << 18;

    //Defense outcomes
    internal const uint F_LAB_SECURED = 1 << 10;

    //Positioning (dedicated per destination - UnitsInPosition is ambiguous between multiple MoveTo actions)
    internal const uint F_IN_POSITION_ENEMY_BASE = 1 << 11;
    internal const uint F_IN_POSITION_ENEMY_LAB = 1 << 12;
    internal const uint F_IN_POSITION_OWN_BASE = 1 << 13;
    internal const uint F_IN_POSITION_OWN_FACTORY = 1 << 14;
    internal const uint F_IN_POSITION_OWN_LAB = 1 << 15;
    internal const uint F_IN_POSITION_ENEMY_FACTORY = 1 << 16;

    public bool BaseAlive
    {
        get => (_flags & F_BASE_ALIVE) != 0;
        set => _flags = value ? (_flags | F_BASE_ALIVE) : (_flags & ~F_BASE_ALIVE);
    }

    public bool LabCaptured
    {
        get => (_flags & F_LAB_CAPTURED) != 0;
        set => _flags = value ? (_flags | F_LAB_CAPTURED) : (_flags & ~F_LAB_CAPTURED);
    }

    public bool FactoryUnderThreat
    {
        get => (_flags & F_FACTORY_UNDER_THREAT) != 0;
        set => _flags = value ? (_flags | F_FACTORY_UNDER_THREAT) : (_flags & ~F_FACTORY_UNDER_THREAT);
    }

    public bool EnemyFactoryLocated
    {
        get => (_flags & F_ENEMY_FACTORY_LOCATED) != 0;
        set => _flags = value ? (_flags | F_ENEMY_FACTORY_LOCATED) : (_flags & ~F_ENEMY_FACTORY_LOCATED);
    }

    public bool LabUnderThreat
    {
        get => (_flags & F_LAB_UNDER_THREAT) != 0;
        set => _flags = value ? (_flags | F_LAB_UNDER_THREAT) : (_flags & ~F_LAB_UNDER_THREAT);
    }

    public bool BaseUnderThreat
    {
        get => (_flags & F_BASE_UNDER_THREAT) != 0;
        set => _flags = value ? (_flags | F_BASE_UNDER_THREAT) : (_flags & ~F_BASE_UNDER_THREAT);
    }

    public bool EnemyLabLocated
    {
        get => (_flags & F_ENEMY_LAB_LOCATED) != 0;
        set => _flags = value ? (_flags | F_ENEMY_LAB_LOCATED) : (_flags & ~F_ENEMY_LAB_LOCATED);
    }

    public bool EnemyBaseLocated
    {
        get => (_flags & F_ENEMY_BASE_LOCATED) != 0;
        set => _flags = value ? (_flags | F_ENEMY_BASE_LOCATED) : (_flags & ~F_ENEMY_BASE_LOCATED);
    }

    public bool EnemyBaseDestroyed
    {
        get => (_flags & F_ENEMY_BASE_DESTROYED) != 0;
        set => _flags = value ? (_flags | F_ENEMY_BASE_DESTROYED) : (_flags & ~F_ENEMY_BASE_DESTROYED);
    }

    public bool EnemyFactoryDestroyed
    {
        get => (_flags & F_ENEMY_FACTORY_DESTROYED) != 0;
        set => _flags = value ? (_flags | F_ENEMY_FACTORY_DESTROYED) : (_flags & ~F_ENEMY_FACTORY_DESTROYED);
    }

    public bool AllEnemyLabsCaptured
    {
        get => (_flags & F_ALL_ENEMY_LABS_CAPTURED) != 0;
        set => _flags = value ? (_flags | F_ALL_ENEMY_LABS_CAPTURED) : (_flags & ~F_ALL_ENEMY_LABS_CAPTURED);
    }

    public bool LabSecured
    {
        get => (_flags & F_LAB_SECURED) != 0;
        set => _flags = value ? (_flags | F_LAB_SECURED) : (_flags & ~F_LAB_SECURED);
    }

    public bool InPositionAtEnemyBase
    {
        get => (_flags & F_IN_POSITION_ENEMY_BASE) != 0;
        set => _flags = value ? (_flags | F_IN_POSITION_ENEMY_BASE) : (_flags & ~F_IN_POSITION_ENEMY_BASE);
    }

    public bool InPositionAtEnemyLab
    {
        get => (_flags & F_IN_POSITION_ENEMY_LAB) != 0;
        set => _flags = value ? (_flags | F_IN_POSITION_ENEMY_LAB) : (_flags & ~F_IN_POSITION_ENEMY_LAB);
    }

    public bool InPositionAtOwnBase
    {
        get => (_flags & F_IN_POSITION_OWN_BASE) != 0;
        set => _flags = value ? (_flags | F_IN_POSITION_OWN_BASE) : (_flags & ~F_IN_POSITION_OWN_BASE);
    }

    public bool InPositionAtOwnFactory
    {
        get => (_flags & F_IN_POSITION_OWN_FACTORY) != 0;
        set => _flags = value ? (_flags | F_IN_POSITION_OWN_FACTORY) : (_flags & ~F_IN_POSITION_OWN_FACTORY);
    }

    public bool InPositionAtOwnLab
    {
        get => (_flags & F_IN_POSITION_OWN_LAB) != 0;
        set => _flags = value ? (_flags | F_IN_POSITION_OWN_LAB) : (_flags & ~F_IN_POSITION_OWN_LAB);
    }

    public bool InPositionAtEnemyFactory
    {
        get => (_flags & F_IN_POSITION_ENEMY_FACTORY) != 0;
        set => _flags = value ? (_flags | F_IN_POSITION_ENEMY_FACTORY) : (_flags & ~F_IN_POSITION_ENEMY_FACTORY);
    }

    public WorldState Copy()
    { 
        return new WorldState() { _flags = _flags, }; 
    }
    public bool GoalAchived(WorldState goal) 
    { 
        return (_flags & goal._flags) == goal._flags; 
    }
}
