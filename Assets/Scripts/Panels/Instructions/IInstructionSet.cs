using UnityEngine;
using System.Collections;

public interface IInstructionSet {
    string getVariant(int variantIndex);
    int getVariantCount();
}
