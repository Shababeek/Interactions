using System.Collections.Generic;

namespace Shababeek.Interactions.Animations
{
    public interface IPose
    {
        /// <summary>
        /// acess the finger of the pose by it's ID
        /// </summary>
        /// <param name="finger">finger ID 0:Thumb, 1: Index: 2: Middle: 3: Ring: 4: Pinky</param>
        float this[int finger]{set;}
        string Name { get;}

    }
}
