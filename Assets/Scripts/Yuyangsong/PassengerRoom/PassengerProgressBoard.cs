using TMPro;
using UnityEngine;

public class PassengerProgressBoard : MonoBehaviour
{
    public ItemSpawnPoint wrench, knob, sword;
    public ItemSpawnPoint flashlight, spray;
    public DrawerOpen seat3CDrawer;
    public DoorOpen luggageDoor, emergencyBox;
    public ToolboxSequence toolbox;
    public TMP_Text[] displays;
    int previousKeys = -1, previousItems = -1, previousLocks = -1, previousPuzzles = -1;

    void Update()
    {
        int keys = Acquired(wrench) + Acquired(knob) + Acquired(sword);
        int items = Acquired(flashlight) + Acquired(spray);
        int seat = seat3CDrawer.IsOpen ? 1 : 0;
        int locks = seat + (luggageDoor.IsOpen ? 1 : 0) + (emergencyBox.IsOpen ? 1 : 0);
        int puzzles = seat + (toolbox.IsSolved ? 1 : 0);
        if (keys == previousKeys && items == previousItems &&
            locks == previousLocks && puzzles == previousPuzzles) return;
        previousKeys = keys;
        previousItems = items;
        previousLocks = locks;
        previousPuzzles = puzzles;
        string text = $"PASSENGER ROOM\nKEYS {keys} / 3\nITEMS {items} / 2\nLOCKS {locks} / 3\nPUZZLES {puzzles} / 2";
        if (keys == 3 && items == 2 && locks == 3 && puzzles == 2) text += "\nROOM COMPLETE";
        foreach (var display in displays) display.text = text;
    }

    int Acquired(ItemSpawnPoint point) => point.HasBeenAcquired ? 1 : 0;
}
