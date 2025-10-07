using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace SmallRPG
{
    public class TextManager : MonoBehaviour
    {
        private GameObject linePrefab;
        private GameObject scrollViewContent;
        private ScrollRect scrollRect;

        static TextManager _instance;
        public static TextManager GetInstance()
        {
            if(_instance == null)
            {
            _instance = new GameObject("_TextManager").AddComponent<TextManager>();
            _instance.Initialise();
            }

            return _instance;
        }

        void Initialise()
        {
            linePrefab = (GameObject)Resources.Load("Prefabs/TextLine", typeof(GameObject));
            scrollViewContent = GameObject.FindGameObjectWithTag("ScrollViewContent");
            scrollRect = GameObject.FindGameObjectWithTag("ScrollRect").GetComponent<ScrollRect>();
        }

        private GameObject CreateNewTextLine(string newText)
        {
            Log(newText);
            GameObject lineTextInstance = Instantiate(linePrefab);
            lineTextInstance.GetComponent<TextMeshProUGUI>().text = newText;
            return lineTextInstance;
        }

        private void AddToScrollView(GameObject newChild)
        {
            newChild.transform.SetParent(scrollViewContent.transform);
            newChild.transform.localScale = Vector3.one;
            ScrollViewportExtra.ScrollToBottom(scrollRect);
        }

        public void CreateAndAddToScrollView(string newText)
        {
            AddToScrollView(CreateNewTextLine(newText));
        }

        [Conditional("DEBUG")]
        // [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message)
        {
            
            UnityEngine.Debug.Log(message);
        }

        [Conditional("DEBUG")]
        // [Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [Conditional("DEBUG")]
        // [Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}