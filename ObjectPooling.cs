using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PoolObject
{
    [Tooltip("The key value")][SerializeField] private string objectName;
    [SerializeField] private GameObject objectPrefab;
    [Tooltip("The initial quantity and max objects in scene")] [SerializeField] private int maxObjectsInScene;
    [Tooltip("if active, increases the list when the quantity is greater than Max Objects In Scene")] [SerializeField] private bool dynamicSize;
    [HideInInspector] public List<GameObject> poolObjectList;

    public GameObject ObjectPrefab { get => objectPrefab;}
    public string ObjectName { get => objectName;}
    public bool DynamicSize { get => dynamicSize;}
    public int MaxObjectsInScene { get => maxObjectsInScene;}
    public void IncreaseMaxObjects()
    {
        maxObjectsInScene++;
    }
}
public class ObjectPooling : Singleton<ObjectPooling>
{

    public List<PoolObject> poolObjects = new List<PoolObject>();

    private void Start()
    {
        foreach (PoolObject poolObject in poolObjects)
        {
            poolObject.poolObjectList = new List<GameObject>();
            if (HasIPoolObject(poolObject))
            {
                for (int i = 0; i < poolObject.MaxObjectsInScene; i++)
                {
                    CreatePool(poolObject);
                }
            }
            else
            {
                Debug.LogWarning($"The {poolObject.ObjectPrefab.name} prefab must implement IPoolObject interface");
            }
        }
    }
    private bool HasIPoolObject(PoolObject poolObject)
    {
        return poolObject.ObjectPrefab.GetComponent<IPoolObject>() != null ? true : false;
    }
    private static void CreatePool(PoolObject poolObject)
    {
        GameObject poolObjectInstance = Instantiate(poolObject.ObjectPrefab);
        poolObjectInstance.name = poolObject.ObjectName;
        poolObjectInstance.SetActive(false);
        poolObject.poolObjectList.Add(poolObjectInstance);
    }

    public GameObject SpawnObjectPool(string poolObjectName, Transform transform, Quaternion rotation)
    {
        return GetSpawnObject(poolObjectName, transform.position, rotation);
    }

    public GameObject SpawnObjectPool(string poolObjectName, Vector3 position, Quaternion rotation)
    {
        return GetSpawnObject(poolObjectName, position, rotation);
    }

    private GameObject GetSpawnObject(string poolObjectName, Vector3 position, Quaternion rotation)
    {
        PoolObject poolObject = GetPoolObject(poolObjectName);
        if (poolObject != null)
        {
            for (int i = 0; i < poolObject.poolObjectList.Count; i++)
            {
                if (!poolObject.poolObjectList[i].activeInHierarchy)
                {
                    return SpawnFromPool(position, rotation, poolObject, i);
                }
            }
            if (poolObject.DynamicSize)
            {
                return CreateForPool(position, poolObject);
            }
            else
            {
                Debug.LogWarning($"Your pool of {poolObject.ObjectName} is empty, recommend enabling dynamic size");
                return null;
            }
        }
        return null;
    }
    private PoolObject GetPoolObject(string poolObjectName)
    {
        for (int i = 0; i < poolObjects.Count; i++)
        {
            if (poolObjects[i].ObjectName == poolObjectName)
                return poolObjects[i];
        }
        Debug.LogWarning("This object doesn't exist");
        return null;
    }

    private static GameObject CreateForPool(Vector3 position, PoolObject poolObject)
    {
        GameObject poolObjectInstance = SetupNewObjectForPool(position, poolObject);
        return poolObjectInstance;
    }

    private static GameObject SetupNewObjectForPool(Vector3 position, PoolObject poolObject)
    {
        GameObject poolObjectInstance = Instantiate(poolObject.ObjectPrefab, position, Quaternion.identity);
        poolObjectInstance.name = poolObject.ObjectName;
        poolObject.IncreaseMaxObjects();
        poolObject.poolObjectList.Add(poolObjectInstance);
        poolObjectInstance.GetComponent<IPoolObject>().OnSpawn();
        return poolObjectInstance;
    }

    private static GameObject SpawnFromPool(Vector3 position, Quaternion rotation, PoolObject poolObject, int index)
    {
        SetupPoolObject(poolObject, index);
        SetSpawnPositionAndRotaion(position, rotation, poolObject, index);
        return poolObject.poolObjectList[index];
    }

    private static void SetupPoolObject(PoolObject poolObject, int index)
    {
        poolObject.poolObjectList[index].SetActive(true);
        poolObject.poolObjectList[index].GetComponent<IPoolObject>().OnSpawn();
    }

    private static void SetSpawnPositionAndRotaion(Vector3 position, Quaternion rotation, PoolObject poolObject, int index)
    {
        poolObject.poolObjectList[index].transform.SetPositionAndRotation(position, rotation);
    }
}
