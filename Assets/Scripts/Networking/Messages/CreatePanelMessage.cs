using UnityEngine;
using UnityEngine.Networking;
using System;
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

        Type t = _actionSet.GetType();
        if (t == typeof(SinglePanelActionSet)) {
            writer.Write((byte)0);
        } else if (t == typeof(ReplacementPanelActionSet)) {
            writer.Write((byte)1);
        } else if (t == typeof(CodePanelActionSet)) {
            writer.Write((byte)2);
        } else {
            throw new Exception("Unexpected Action Set Type " + t);
        }

        _actionSet.Serialize(writer);

        writer.Write(_x);
        writer.Write(_y);
        writer.Write(_prefabIndex);
    }

    public override void Deserialize(NetworkReader reader) {
        base.Deserialize(reader);

        byte setType = reader.ReadByte();
        switch (setType) {
            case (byte)0: _actionSet = new SinglePanelActionSet(); break;
            case (byte)1: _actionSet = new ReplacementPanelActionSet(); break;
            case (byte)2: _actionSet = new CodePanelActionSet(); break;
            default: throw new Exception("Unexpected Action Set ID " + setType);
        }
        
        _actionSet.Deserialize(reader);

        _x = reader.ReadInt32();
        _y = reader.ReadInt32();
        _prefabIndex = reader.ReadInt32();
    }
}
