using UnityEngine;

public class ShadowCharacter : Shadow
{
    [SerializeField] Character character;

    protected override void LateUpdate()
    {
        if (character != null && character.IsGrounded)
        {
            float size = 1;
            render.transform.localScale = Vector3.one * size;
            render.transform.position = source.position + ((-baseDistance + OFFSET) * Vector3.up);
            render.SetActive(true);

            return;
        }

        base.LateUpdate();
    }
}
