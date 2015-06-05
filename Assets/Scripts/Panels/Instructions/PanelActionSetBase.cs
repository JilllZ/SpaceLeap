using UnityEngine;
using System.Collections;

public abstract class PanelActionSetBase : CustomMessage {
    protected int _setId;
    protected int _currentVariantIndex = -1;

    public int setId {
        get {
            return _setId;
        }
    }

    public virtual int currentVariantIndex {
        get {
            return _currentVariantIndex;
        }
        set {
            _currentVariantIndex = value;
        }
    }

    public abstract string getVariant(int variantIndex);
    public abstract int getVariantCount();

    public void initialize() {
        _setId = Random.Range(int.MinValue, int.MaxValue);
        sendToServer();
    }

    public override void Serialize(UnityEngine.Networking.NetworkWriter writer) {
        base.Serialize(writer);
        writer.Write(_setId);
        writer.Write(_currentVariantIndex);
    }

    public override void Deserialize(UnityEngine.Networking.NetworkReader reader) {
        base.Deserialize(reader);
        _setId = reader.ReadInt32();
        _currentVariantIndex = reader.ReadInt32();
    }
}
