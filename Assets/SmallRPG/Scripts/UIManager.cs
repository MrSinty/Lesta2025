using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace SmallRPG
{
    public class UIManager : MonoBehaviour
    {
        private GameObject nextFightBtn;
        private GameObject nextTurnBtn;
        private GameObject addClassBtn;
        private GameObject dropdownClass;
        private GameObject winPanel;
        private GameObject lvlUpPanel;
        private GameObject weaponPanel;
        private GameObject droppedWeaponText;
        private GameObject dropdownExistClass;
        private TMP_Dropdown dropdownComp;
        private TextMeshProUGUI droppedTextComp;
        static UIManager _instance;
        public static UIManager GetInstance()
        {
            if(_instance == null)
            {
            _instance = new GameObject("_UIManager").AddComponent<UIManager>();
            _instance.Initialise();
            }

            return _instance;
        }

        private void Initialise()
        {
            nextFightBtn = GameObject.FindGameObjectWithTag("NextFightBtn");
            nextTurnBtn = GameObject.FindGameObjectWithTag("NextTurnBtn");
            addClassBtn = GameObject.FindGameObjectWithTag("AddClassBtn");
            dropdownClass = GameObject.FindGameObjectWithTag("DropdownClass");

            winPanel = GameObject.FindGameObjectWithTag("WinPanel");
            winPanel.SetActive(false);

            weaponPanel = GameObject.FindGameObjectWithTag("WeaponPanel");
            droppedWeaponText = GameObject.FindGameObjectWithTag("DroppedWeaponText");
            droppedTextComp = droppedWeaponText.GetComponent<TextMeshProUGUI>();
            weaponPanel.SetActive(false);

            lvlUpPanel = GameObject.FindGameObjectWithTag("LvlUpPanel");
            dropdownExistClass = GameObject.FindGameObjectWithTag("DropdownExistClass");
            dropdownComp = dropdownExistClass.GetComponent<TMP_Dropdown>();
            lvlUpPanel.SetActive(false);
        }

        public void ShowLvlUpPanel(ClassManager classManager)
        {
            var classIds = classManager.GetOwnedClassIds();
            dropdownComp.ClearOptions();
            dropdownComp.AddOptions(classIds);
            lvlUpPanel.SetActive(true);
        }

        public void HideLvlUpPanel()
        {
            lvlUpPanel.SetActive(false);
        }

        public string GetSelectedClassID()
        {
            return dropdownComp.options[dropdownComp.value].text;
        }

        public void ShowDroppedWeaponPanel(Weapon dropped)
        {
            weaponPanel.SetActive(true);
            droppedTextComp.text = dropped.DisplayName;
        }
        public void HideDroppedWeaponPanel()
        {
            droppedTextComp.text = "Nothing";
            weaponPanel.SetActive(false);
        }

        public void ShowWinWindow()
        {
            winPanel.SetActive(true);
        }

        public void FightStarted()
        {
            nextTurnBtn.SetActive(true);

            nextFightBtn.SetActive(false);
            addClassBtn.SetActive(false);
            dropdownClass.SetActive(false);
        }

        public void FightEnded()
        {
            nextTurnBtn.SetActive(false);

            nextFightBtn.SetActive(true);
            addClassBtn.SetActive(true);
            dropdownClass.SetActive(true);
        }
    }
}