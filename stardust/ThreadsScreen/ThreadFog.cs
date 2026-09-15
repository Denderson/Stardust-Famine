using Menu;
using UnityEngine;

namespace Stardust.ThreadsScreen;

public class ThreadFog : PositionedMenuObject
{
    public FSprite sprite;

    public ThreadFog(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
    {
        Vector2 screenSize = menu.manager.rainWorld.options.ScreenSize;

        sprite = new FSprite("Futile_White", true)
        {
            shader = menu.manager.rainWorld.Shaders["ThreadFog"]
        };
        sprite.scaleX = screenSize.x / sprite.element.sourcePixelSize.x;
        sprite.scaleY = screenSize.y / sprite.element.sourcePixelSize.y;

        Container.AddChild(sprite);
        sprite.SetPosition(pos);

        Container.MoveToBack();
    }

    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        sprite.SetPosition(pos);
    }

    public override void RemoveSprites()
    {
        base.RemoveSprites();
        sprite.RemoveFromContainer();
    }
}