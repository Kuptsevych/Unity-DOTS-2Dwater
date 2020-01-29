using UnityEngine;

public class SetFps : MonoBehaviour
{
	private void Start()
	{
		Application.targetFrameRate = 60;
	}
}