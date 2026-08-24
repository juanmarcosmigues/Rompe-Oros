using UnityEngine;
using static CombatEntity;

public class Dummy : MonoBehaviour
{
    protected CombatEntity combat;
    protected Character character;

    private void Awake()
    {
        combat = GetComponent<CombatEntity>();
        character = GetComponent<Character>();
        combat.onRecieveDamageEvent += OnHurt;
    }

    void OnHurt (HitBox hb, string tag, HitResults result, int damage, float weight, Vector3 direction, Vector3 point)
    {
        Vector3 pushDir = hb.Owner.transform.position.x < transform.position.x ? Vector3.right : Vector3.left;
        pushDir += Vector3.up;
        pushDir.Normalize();

        character.Impulse(pushDir * 10f);
    }
}
