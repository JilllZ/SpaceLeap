using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CreatePanelMessage : CustomMessage {
    private PanelActionSetBase _actionSet;
    private int _x, _y;
    private int _prefabIndex;

    public PanelActionSetBase actionSet { get { return _actionSet; } }
    public int x { get { return _x; } }
    public int y { get { return _y; } }
    public int prefabIndex { get { return _prefabIndex; } }

    public CreatePanelMessage() { }

    public CreatePanelMessage(PanelActionSetBase actionSet, int x, int y, int prefabIndex) {
        _actionSet = actionSet;
        _x = x;
        _y = y;
        _prefabIndex = prefabIndex;
    }

    public override void Serialize(NetworkWriter writer) {
        base.Serialize(writer);
        _actionSet.Serialize(writer);
        writer.Write(_x);
        writer.Write(_y);
        writer.Write(_prefabIndex);
    }

    public override void Deserialize(NetworkReader reader) {
        base.Deserialize(reader);
        _actionSet.Deserialize(reader);
        _x = reader.ReadInt32();
        _y = reader.ReadInt32();
        _prefabIndex = reader.ReadInt32();
    }
}
