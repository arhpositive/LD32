﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ui
{
    public class RegainFocus : MonoBehaviour
    {
	    public GameObject DefaultGameObject;

        private GameObject _lastSelectedGameObject;
        
        // Update is called once per frame
	    private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject)
            {
                _lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            }
            else if (Input.GetButtonDown("Submit") || !Mathf.Approximately(Input.GetAxis("Vertical"), 0.0f) ||
                     !Mathf.Approximately(Input.GetAxis("Horizontal"), 0.0f))
            {
	            EventSystem.current.SetSelectedGameObject(_lastSelectedGameObject);
            }
        }

	    private void OnEnable()
        {
            if (EventSystem.current.currentSelectedGameObject)
            {
				EventSystem.current.SetSelectedGameObject(DefaultGameObject);
                EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().OnSelect(null);
            }
        }
    }
}