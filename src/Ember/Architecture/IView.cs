using Microsoft.Xna.Framework;

namespace Ember.Architecture;

public interface IView
{
    bool IsVisible { get; set; }
    void Update(GameTime gameTime);
    void Draw();
}
