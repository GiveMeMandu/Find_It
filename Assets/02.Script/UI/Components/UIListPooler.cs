using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SnowRabbit
{
    public class UIListPooler : MonoBehaviour
    {
        public GameObject template;
        private List<GameObject> pooledObjects = new List<GameObject>();
        private List<GameObject> activeObjects = new List<GameObject>();

        void Awake()
        {
            template.SetActive(false);
        }

        /// <summary>
        /// 기존 Cell들을 모두 지우고 새로운 Cell들을 준비합니다.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<GameObject> PrepareCells(int count)
        {
            // Ensure the pooledObjects list is large enough
            while (pooledObjects.Count < count)
            {
                GameObject obj = Instantiate(template, transform);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
            // Deactivate all currently active objects
            foreach (var obj in activeObjects)
            {
                obj.SetActive(false);
            }

            activeObjects.Clear();
            pooledObjects.GetRange(0, count).ForEach(obj =>
            {
                obj.SetActive(true);
                activeObjects.Add(obj);
            });
            return activeObjects;
        }

        /// <summary>
        /// 기존 Cell을 모두 유지하고 새로운 Cell을 추가합니다.
        /// </summary>
        /// <returns></returns>
        public GameObject AddCell()
        {
            if (pooledObjects.Count < activeObjects.Count + 1)
            {
                GameObject obj = Instantiate(template, transform);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
            var newCell = pooledObjects[activeObjects.Count];
            newCell.SetActive(true);
            activeObjects.Add(newCell);
            return newCell;
        }

        public GameObject GetCell(int index)
        {
            if (index < 0 || index >= activeObjects.Count)
            {
                return null;
            }
            return activeObjects[index];
        }


        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
