using UnityEngine;

public class CombatController : MonoBehaviour
{
    MeeleFighter meeleFighter;

    void Awake()
    {
        meeleFighter = GetComponent<MeeleFighter>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            meeleFighter.TryToAttack();
        }
    }
}
