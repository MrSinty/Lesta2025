using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SmallRPG
{
    public class ClassDropdownUI : MonoBehaviour
    {
        [SerializeField] private ClassManager classManager;
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private UIMode mode = UIMode.Starting;

        public UIMode AddingMode
        {
            get { return mode; }
            set { mode = value; }
        }

        private List<string> classIds = new ();

        public enum UIMode
        {
            Starting, // show all classes
            AddNew     // show only classes not owned
        }

        void Start()
        {
            //RefreshOptions();
        }

        public void RefreshOptions()
        {
            if (classManager == null || dropdown == null) return;
            classIds = mode == UIMode.Starting ? classManager.GetAllClassIds() : classManager.GetAvailableNewClassIds();
            dropdown.ClearOptions();
            dropdown.AddOptions(classIds);
        }

        // Hook to a UI Button's OnClick to add the selected class
        public void ConfirmAddSelected()
        {
            if (classManager == null || dropdown == null || classIds.Count == 0) return;
            int idx = Mathf.Clamp(dropdown.value, 0, classIds.Count - 1);
            string id = classIds[idx];
            if (mode == UIMode.Starting)
            {
                classManager.ChooseStartingClass(id);
            }
            else
            {
                classManager.ChooseAddNewClass(id);
            }
            // Refresh in case this class is no longer available after adding
            RefreshOptions();
        }
    }
}


