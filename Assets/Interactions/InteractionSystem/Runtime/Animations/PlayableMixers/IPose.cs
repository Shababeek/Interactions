using System.Collections.Generic;

namespace Kandooz.InteractionSystem.Animations
{
    public interface IPose
    {
        float this[int indexer]{set;}
        string Name { get;}

    }
}
