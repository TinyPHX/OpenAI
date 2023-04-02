using OpenAiMonoBehaviour = OpenAi.OpenAiMonoBehaviour;
using UnityEngine;

public class WASDMovementComponent : OpenAiMonoBehaviour
{
	[Tooltip("Sensitivity of the movement.")]
	[Range(0.1f, 10f)]
	public float sensitivity = 0.5f;

	[Tooltip("Force to apply for a single movement.")]
	public float force = 10f;

	void Update()
	{
		if (Input.GetKey(KeyCode.W))
		{
			transform.Translate(Vector3.forward * Time.deltaTime * force * sensitivity);
		}

		if (Input.GetKey(KeyCode.S))
		{
			transform.Translate(Vector3.back * Time.deltaTime * force * sensitivity);
		}

		if (Input.GetKey(KeyCode.A))
		{
			transform.Translate(Vector3.left * Time.deltaTime * force * sensitivity);
		}

		if (Input.GetKey(KeyCode.D))
		{
			transform.Translate(Vector3.right * Time.deltaTime * force * sensitivity);
		}
	}
}