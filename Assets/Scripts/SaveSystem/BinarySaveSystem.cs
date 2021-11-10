using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class BinarySaveSystem : ISaveSystem
{
	private string _filePath = Application.persistentDataPath + "/gameSave.dat";

	private static BinaryFormatter _bFormatter = new BinaryFormatter();


	public void Save(StoredData storedData)
	{
		FileStream fileStream = new FileStream(_filePath, FileMode.OpenOrCreate);

		_bFormatter.Serialize(fileStream, storedData);

		fileStream.Close();

	}

	public StoredData Load() 
	{
		if (File.Exists(_filePath))
		{
			FileStream fileStream = new FileStream(_filePath, FileMode.Open);

			StoredData storedData = _bFormatter.Deserialize(fileStream) as StoredData;

			fileStream.Close();

			if (storedData == null)
				return new StoredData();
			else
				return storedData;
		}
		else
		{
			return new StoredData();
		}

	}
}
