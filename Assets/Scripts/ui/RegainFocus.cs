using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ui
{
    public class RegainFocus : MonoBehaviour
    {
        private GameObject _lastSelectedGameObject;
        
        // Update is called once per frame
        void Update()
        {
            if (EventSystem.current.currentSelectedGameObject)
            {
                _lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            }
            else if (Input.GetButtonDown("Submit") || !Mathf.Approximately(Input.GetAxis("Vertical"), 0.0f) || !Mathf.Approximately(Input.GetAxis("Horizontal"), 0.0f))
            {
                EventSystem.current.SetSelectedGameObject(_lastSelectedGameObject);
            }
        }

        void OnEnable()
        {
            if (EventSystem.current.currentSelectedGameObject)
            {
                EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().OnSelect(null);
            }
        }
    }
}