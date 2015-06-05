using UnityEngine;
using System.Collections;

public abstract class PanelActionSetBase {
    protected int _setId;
    protected int _currentVariantIndex = -1;
    protected string _panelLabel;

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

    public string panelLabel {
        get {
            return _panelLabel;
        }
    }

    public abstract string getVariant(int variantIndex);
    public abstract int getVariantCount();

    public PanelActionSetBase() { }

    public PanelActionSetBase(string panelLabel) {
        _setId = Random.Range(int.MinValue, int.MaxValue);
        _panelLabel = panelLabel;
    }

    public virtual void Serialize(UnityEngine.Networking.NetworkWriter writer) {
        writer.Write(_setId);
        writer.Write(_currentVariantIndex);
        writer.Write(_panelLabel);
    }

    public virtual void Deserialize(UnityEngine.Networking.NetworkReader reader) {
        _setId = reader.ReadInt32();
        _currentVariantIndex = reader.ReadInt32();
        _panelLabel = reader.ReadString();
    }
}
