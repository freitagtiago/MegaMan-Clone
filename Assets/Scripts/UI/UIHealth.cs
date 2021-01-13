using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
    [SerializeField] Image healthUnitPrefab; 
    [SerializeField] List<Image> healthUnits;
    [SerializeField] bool isPlayerBar;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateHealthBar(int currentHealth)
    {
        if(currentHealth <= 0)
        {
            Erase();
            
        }
        else
        {
            if(healthUnits.Count < currentHealth)
            {
                for(int i = 0; healthUnits.Count < currentHealth; i++)
                {
                    healthUnits.Add(Instantiate(healthUnitPrefab,transform));
                }
            }else if(healthUnits.Count > currentHealth)
            {
                for (int i = 0; healthUnits.Count > currentHealth; i++)
                {
                    if(healthUnits.Count > 0)
                    {
                        GameObject unit = healthUnits[healthUnits.Count - 1].gameObject;
                        healthUnits.RemoveAt(healthUnits.Count - 1);
                        Destroy(unit);
                    }   
                }
            }
        }
    }  
    
    private void Erase()
    {
        for(int i = 0; healthUnits.Count > 0; i++)
        {
            GameObject unit = healthUnits[healthUnits.Count - 1].gameObject;
            healthUnits.RemoveAt(healthUnits.Count - 1);
            Destroy(unit);
        }
    }

    public bool GetIsPlayerBar()
    {
        return isPlayerBar;
    }
}
