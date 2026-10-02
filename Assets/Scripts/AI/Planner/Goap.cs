using System.Collections.Generic;
using UnityEngine;

public class Goap
{
    private List<Action> availableActions;

    public Goap(List<Action> actions)
    {
        availableActions = actions;
    }

    void BuildGraphForward(Node parent, List<Node> leaves, List<Action> actions, Goal goal)
    {
        foreach (Action action in actions)
        {
            if (action.IsValid(parent.worldState))
            {
                WorldState newState = action.ApplyEffects(parent.worldState.Copy());
                Node node = CreateNode(parent, newState, action);

                if (newState.GoalAchived(goal.GetGoalState()))
                    leaves.Add(node);
                else
                    BuildGraphForward(node, leaves, CreateActionSubSet(actions, action), goal);
            }
        }
    }

    public List<Action> BuildPlanForward(WorldState initialState, Goal goal)
    {
        if (goal == null) 
            return null;

        Node root = new Node { worldState = initialState, cost = 0f };
        List<Node> leaves = new List<Node>();

        BuildGraphForward(root, leaves, availableActions, goal);

        if (leaves.Count == 0)
            return null;

        return ExtractPlan(SearchCheapestNode(leaves));
    }

    public Node SearchCheapestNode(List<Node> nodes)
    {
        Node cheapest = nodes[0]; 
        foreach(Node node in nodes)
        {
            if (node.cost < cheapest.cost)
                cheapest = node;
        }
        return cheapest; 
    }

    Node CreateNode(Node parent, WorldState worldState, Action action)
    {
        return new Node
        {
            parent = parent,
            worldState = worldState,
            action = action,
            cost = parent.cost + action.cost,
        };
    }

    List<Action> CreateActionSubSet(List<Action> actions, Action toRemove)
    {
        List<Action> subset = new List<Action>(actions);
        subset.Remove(toRemove);
        return subset;
    }

    List<Action> ExtractPlan(Node leaf)
    {
        List<Action> plan = new List<Action>();
        Node current = leaf;
        while (current.action != null)
        {
            plan.Insert(0, current.action);
            current = current.parent;
        }
        return plan;
    }
}
