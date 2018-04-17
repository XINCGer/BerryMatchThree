using UnityEngine;
using System.Collections;

public abstract class IBlock : MonoBehaviour {
    public Slot slot;

    public int level = 1;
    abstract public void BlockCrush(bool force);
    abstract public bool CanBeCrushedByNearSlot();
    abstract public void Initialize();
    abstract public bool CanItContainChip();
    abstract public int GetLevels();
}
