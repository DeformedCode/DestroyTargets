using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveSystem
{
    public StoredData Load();

    public void Save(StoredData storedData);

}
