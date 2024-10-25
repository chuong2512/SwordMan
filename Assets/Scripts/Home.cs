using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

public class Home : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        if(PlayerPrefs.HasKey("CurrentLevel"))
        {
            string currentLevel = PlayerPrefs.GetString("CurrentLevel");
            MMSceneLoadingManager.LoadScene(currentLevel);
        }
       else
            MMSceneLoadingManager.LoadScene("Level1");
    }
}
