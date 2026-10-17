using UnityEngine;
using System.Collections;

public class GreedyGambler : MonoBehaviour
{
    
    // WHILE the agent has not reached the exit
    // Sense the environment
    //     Find nearby:
    // loot, enemies, obstacles, exit, and available spells
    //     IF the agent’s health is very low
    // move toward the exit 
    // CONTINUE
    //     FOR each available loot item:
    // Determine:
    // loot value, distance to loot, enemy danger, chance of successfully obtaining loot, risk/probable health if obtaining the loot 
    //     Calculate utility:
    // utility = (loot value * chance of success)
    //
    // Select the loot/action with the highest utility
    //     IF the selected action requires a chance-based spell:
    // use the spell 
    //     IF the spell succeeds:
    // continue toward the loot
    //     ELSE 
    // recalculate the utility of available actions
    // ELSE
    //     move toward the selected loot
    // IF the dungeon has been sufficiently explored 
    //     OR the agent has enough valuable loot/inventory full
    // move toward exit
    //
    //     END WHILE
   
   
    void Start()
    {
       
    }

    void Update()
    {
        
    }
    
}
