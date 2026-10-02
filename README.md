# RTS AI — Documentation technique

Projet de stratégie temps réel dans lequel deux équipes s'affrontent pour détruire la base adverse. L'une des deux équipes est entièrement pilotée par une intelligence artificielle structurée en trois couches décisionnelles.

## Architecture générale

L'IA repose sur une séparation stricte entre la décision stratégique (quoi faire), la planification (dans quel ordre), et l'exécution tactique (comment chaque unité se comporte au contact). Chaque couche opère à sa propre fréquence et communique avec la suivante.

```
 Utility System (stratégie)
 "quel objectif poursuivre ?"
        │
        │  goal queue triée par priorité
        ▼
 GOAP Planner (planification)
 "quelles actions enchaîner ?"
        │
        │  plan = [Action, Action, ...]
        ▼
 FSM tactique (exécution)
 "comment chaque unité agit frame par frame"
```

## Couche 1 — Utility System

La première couche détermine quels objectifs l'IA doit poursuivre à un instant donné. Chaque goal est un `MonoBehaviour` exposant une méthode `RatePriority` qui retourne un entier entre 0 et 10.

### Métriques d'entrée

Les fonctions de scoring s'appuient sur plusieurs sources de données :

- Le `WorldState` courant (bitmask de flags booléens).
- Le `GameState` qui expose des métriques dérivées du jeu : ratio de HP moyen de l'armée, ratio de build points, compteurs de labs par propriétaire, position de la base ennemie.
- L'`InfluenceMap` qui fournit la présence militaire autour d'un point donné (valeur positive = dominance alliée, négative = dominance ennemie).

### Scoring par AnimationCurve

Aucun seuil n'est hardcodé dans le code. Chaque métrique est normalisée en 0–1 puis évaluée par une `AnimationCurve` configurable dans l'Inspector Unity. Les scores issus de plusieurs courbes sont additionnés puis mis à l'échelle sur 0–10.

### Goal Queue

Tous les goals dont la priorité est non nulle entrent dans une queue triée par score décroissant. La queue est recalculée à intervalle fixe pour s'adapter à l'évolution du monde.

L'objectif dans l'implémentation de cette Utility System était d'avoir une première couche de décision ayant accés à un maximum de données de la partie et pouvant mener à des decisions variés et coherentes, sans avoir à ajouter des conditions hardcodés et répetitives. Le scoring des goal est d'autant plus important ici sachant qu'on ne se contente pas de conserver uniquement le prioritaire comme dans un GOAP classique.

## Couche 2 — GOAP Planner

Lorsqu'un goal arrive en tête de queue et que les troupes nécessaires à sa réalisation sont disponibles, le planner GOAP génère un plan d'actions pour la squad assignée.

Le planner va dicter les actions prises à moyenne échelle (squad), elle permet de distribuer efficacement un plan clair d'action, c'était selon nous le meilleur choix pour un jeu orienté stratégie, ou les agents ia doivent avoir une idée nette des actions à entreprendre pour gagner la partie.

### WorldState

L'état du monde est encodé dans un unique bitmask `uint32` de flags booléens.

La vérification de satisfaction d'un goal se fait par un AND de masques :

```csharp
public bool GoalAchieved(WorldState goal)
{
    return (_flags & goal._flags) == goal._flags;
}
```

### Algorithme de recherche

Le planner utilise une recherche en profondeur forward-chaining. Il énumère tous les plans valides et sélectionne le moins coûteux. L'absence d'heuristique et de mémoïsation est viable car le jeu d'actions est volontairement restreint.

### Actions GOAP

Chaque action est un `MonoBehaviour` qui déclare un coût, des préconditions (flags requis), des effets (flags produits).

## Couche 3 — FSM tactique

Chaque unité dispose de sa propre machine à états finie qui gère son comportement au niveau micro, indépendamment du plan GOAP en cours. La FSM réagit aux événements locaux sans remonter à la couche stratégique.

La FSM fonctionne avec des States, des Conditions et des Transitions, la fsm globale va mettre à jour les états de l'unité si les conditions d'une des transitions d'un état à l'autre sont remplies, permettant ainsi de passer d'un état à un autre état à l'aide de conditions prédéfinies.

- FSM
- FSMState
- FSMCondition
- FSMTransition

Chaque Unité possède une variable CurrentOrder et des fonctions assignées à ces états, CurrentOrder est un enum UnitOrder possèdant les valeurs "None, Move, Attack, Repair, Capture" et l'unité possède respectivement les functions OrderMove,OrderAttack ect...
```
public enum UnitOrder
{
    None,
    Move,
    Attack,
    Capture,
    Repair,
}
```
Ensuite la FSM va réagir à cette ordre et le consommer si jamais elle trouve les conditions nécessaires pour passer d'un état à l'autre.
On a fait ça pour faire en sorte que l'unité puisse être gérée depuis n'importe quelle source, que ce soit une IA ou un joueur.

On a fait le choix d'utiliser une FSM car elle permet de gérer simplement les actions plus triviales et à l'échelle des agents individuellement, le fait de pouvoir traquer l'état courant des ia en continu est aussi intéressant pour nous.

## Systèmes transverses

### Influence Map

L'`InfluenceMap` est une grille 2D où chaque cellule à une influence : positive pour une dominance alliée, négative pour une dominance ennemie. Le `MonoBehaviour` `InfluenceMap` ne calcule rien lui-même, il fusionnent plusieurs `InfluenceSubMap` des `ScriptableObject`, chacune responsable d'une seule source d'information :

- `UnitInfluenceSubMap` traque les unités visibles sur le terrain et projette leur influence sur un rayon modifiable autour d'elles, avec une intensité qui décroît avec la distance. Lorsqu'une unité sort du champ de vision, son influence n'est pas retirée d'un coup mais décroît progressivement dans le temps (`decayStartTime`, `decayTime`), plutôt que de "l'oublier" instantanément.
- `EnemyUnitInfluenceSubMap` hérite de la précédente en ne gardant que les unités adverses, elle est lue directement par certains goals (ex. `DefendBase`) plutôt que fusionnée dans la carte finale.
- `BuildingInfluenceSubMap` applique la même logique aux bâtiments (`BuildingInfluence`), un bâtiment projette une zone d'influence constante tant qu'il existe.
- `FogOfWarInfluenceSubMap` ne produit pas d'influence : elle fait le pont avec le `FogOfWarSystem` et fournit les états `IsVisible`/`WasVisible` utilisés par les autres sous-cartes.
- `InformationStalenessSubmap` ne stocke pas de l'influence mais un numéro de cycle, ce qui permet de savoir depuis combien de mises à jour une cellule n'a pas été rafraîchie (utilisé notamment par le scouting).

À chaque cycle, `InfluenceMap.MergeGrids` additionne cellule par cellule les sous-cartes (en excluant fog, staleness, buildings et la sous-carte ennemie dédiée) pour ne garder dans la carte fusionnée que la présence militaire alliée/ennemie, c'est cette valeur qui est exposée à l'Utility System. Les sous-cartes sont mises à jour sur plusieurs frames (`updateFrequency`, `SetupStaggeredUpdate`) pour répartir le coût CPU plutôt que de tout recalculer sur la même frame.

L'IA lit ces données via `AIContext.influenceMap`, soit `GetInfluenceAtPosition` pour la dominance globale à un point donné, soit `GetSubMapInfluenceAtPosition<T>` pour interroger une sous-carte précise (par exemple la présence ennemie autour de la base pour prioriser le goal "Defend Base").

### Formation Manager

Les squads utilisent un `FormationManager` qui calcule une grille rectangulaire orientée dans la direction du mouvement. La direction est recalculée depuis le centre réel du groupe (`GetCenter`) à chaque ordre. Les slots sont distribués selon le `TypeId` des unités . Le pathfinding individuel est délégué aux `NavMeshAgent` de Unity.