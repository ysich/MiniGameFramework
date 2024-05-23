using Core;
using UnityEngine;

public class Init : MonoBehaviour
{
    private void Start()
    {
        // Game.AddSingleton<TimeInfo>();
        Game.AddSingleton<ObjectPool>();;
    }

    private void Update()
    {
        Game.Update();
    }

    private void LateUpdate()
    {
        Game.LateUpdate();
    }

    private void OnApplicationQuit()
    {
        Game.Close();
    }
}
