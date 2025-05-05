using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    PlayerMovement pm;
    HealthPlayer hp;
    string savePath;
    saveData data;

    private void Awake() {
        pm = FindObjectOfType<PlayerMovement>();
        hp = FindObjectOfType<HealthPlayer>();

        savePath = Application.persistentDataPath + "/save.dat";
        if(!File.Exists(savePath))
        {
            saveData newData = new saveData();
            newData.playerPosition = pm.GetPosition();
            newData.playerHealth = hp.GetHealth();
            SaveGame(newData);
        }
        data = LoadGame();
    }

    void SaveGame(saveData dataToSave)
    {
        string JsonData = JsonUtility.ToJson(dataToSave);
        File.WriteAllText(savePath, JsonData);
    }

    saveData LoadGame()
    {
        string loadedData = File.ReadAllText(savePath);
        saveData loadedDataObject = JsonUtility.FromJson<saveData>(loadedData);
        return loadedDataObject;
    }

    public void SaveGameButton()
    {
        Vector3 playerPos = pm.GetPosition();
        int pHealth = hp.GetHealth();

        data.playerPosition = playerPos;
        data.playerHealth = pHealth;

        if(data.enemies == null)
        {
            data.enemies = new List<EnemyData>();
        }
        else
        {
            data.enemies.Clear();
        }

        foreach(var enemy in FindObjectsOfType<EnemyAI>())
        {
            EnemyData enemyData = new EnemyData
            {
                position = enemy.transform.position,
                health = enemy.GetHealth()
            };
            data.enemies.Add(enemyData);
        }

        if(data.breakableObjects == null)
        {
            data.breakableObjects = new List<BreakableObjectData>();
        }
        else
        {
            data.breakableObjects.Clear();
        }

        foreach(var breakableObject in FindObjectsOfType<HitObjects>())
        {
            BreakableObjectData objectData = new BreakableObjectData
            {
                position = breakableObject.transform.position
            };
            data.breakableObjects.Add(objectData);
        }
        SaveGame(data);
    }

    public void LoadGameButton()
    {
        Vector3 playerPos = data.playerPosition;
        int health = data.playerHealth;

        pm.SetPosition(playerPos);
        hp.SetHealth(health);

        EnemyAI[] enemy = FindObjectsOfType<EnemyAI>();

        for(int i = 0; i > enemy.Length; i++)
        {
            enemy[i].SetHealth(data.enemies[i].health);
        }
    }
}

public class saveData
{
    public Vector3 playerPosition;
    public int playerHealth;
    public List<EnemyData> enemies = new List<EnemyData>();
    public List<BreakableObjectData> breakableObjects = new List<BreakableObjectData>();
}

[System.Serializable]
public class EnemyData
{
    public Vector3 position;
    public int health;
}

[System.Serializable]
public class BreakableObjectData
{
    public Vector3 position;
    public bool isBroken;
}
