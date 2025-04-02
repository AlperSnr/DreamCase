using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PoolingManager : MonoBehaviour
{
    public static PoolingManager instance;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
        }
        else
        {
            Debug.LogWarning("There is more than one PoolingManager instance.");
        }
    }

    public void CreatePool(string tag, GameObject prefab, int size)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag] = new Queue<GameObject>();

            for (int i = 0; i < size; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.transform.SetParent(transform);
                obj.SetActive(false);
                poolDictionary[tag].Enqueue(obj);
            }
        }
    }

    public GameObject GetFromPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist");
            return null;
        }

        if (poolDictionary[tag].Count <= 5)
        {
            Populate(tag);
        }

        GameObject obj = poolDictionary[tag].Dequeue();
        obj.SetActive(true);

        return obj;
    }

    public GameObject GetFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetFromPool(tag);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return;
        }

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }

    private void Populate(string tag)
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject obj = Instantiate(poolDictionary[tag].Peek());
            obj.transform.SetParent(transform);
            obj.SetActive(false);
            poolDictionary[tag].Enqueue(obj);
        }
    }
}
