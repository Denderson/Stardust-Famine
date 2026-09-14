using Menu;
using UnityEngine;

namespace Stardust.ThreadsScreen;

public class ThreadSpiral : PositionedMenuObject
{
    public FSprite sprite;
    public float rotation;
    public float targetRotation;
    public float rotationSpeed = 4f;

    public ThreadSpiral(Menu.Menu menu, MenuObject owner, Vector2 pos, float size) : base(menu, owner, pos)
    {
        sprite = new FSprite("Futile_White", true)
        {
            scaleX = size,
            scaleY = size,
            shader = menu.manager.rainWorld.Shaders["ThreadSpiral"]
        };
        Container.AddChild(sprite);
        sprite.SetPosition(pos);
        rotation = 0f;
        targetRotation = 0f;
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.LeftArrow)) targetRotation -= 90f;
        if (Input.GetKeyDown(KeyCode.RightArrow)) targetRotation += 90f;

        rotation = Mathf.Lerp(rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        sprite.SetPosition(pos);
        sprite.rotation = rotation;
    }

    public override void RemoveSprites()
    {
        base.RemoveSprites();
        sprite.RemoveFromContainer();
    }
}