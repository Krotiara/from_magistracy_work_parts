using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class IndexHolder : MonoBehaviour
    {
        public string type;
        public string index;
        public string photoPath;

        public override bool Equals(object obj)
        {
            var holder = obj as IndexHolder;
            return holder != null &&
                   base.Equals(obj) &&
                   type == holder.type &&
                   index == holder.index &&
                   photoPath == holder.photoPath;
        }
    }
}
