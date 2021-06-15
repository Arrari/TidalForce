using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraRotateAround : MonoBehaviour
{
	public List<GameObject> planets;
	public Dropdown dropdown;
	public Button button;
	public Transform target;
	public Vector3 offset;
	public float sensitivity = 3; // чувствительность мышки
	public float limit = 80; // ограничение вращения по Y
	public float zoom = 0.25f; // чувствительность при увеличении, колесиком мышки
	public float zoomMax = 10; // макс. увеличение
	public float zoomMin = 3; // мин. увеличение
	private float X, Y;
	private bool isTimeStop = false;

	void Start()
	{
		limit = Mathf.Abs(limit);
		if (limit > 90) limit = 90;
		offset = new Vector3(offset.x, offset.y, -Mathf.Abs(zoomMax) / 2);
        transform.position = target.position + offset;

		button.onClick.AddListener(() =>
		{
			if (isTimeStop)
			{
				Time.timeScale = 1;
				isTimeStop = !isTimeStop;
			}
			else
			{
				Time.timeScale = 0;
				isTimeStop = !isTimeStop;
			}
		});
		dropdown.onValueChanged.AddListener((number) =>
		{
			for (int i = 0; i < planets.Count; i++)
            {
				if(planets[i].name == dropdown.options[number].text)
                {
					target = planets[i].transform;
					return;
                }
            }
		});
	}

	void Update()
	{
		if (Input.GetAxis("Mouse ScrollWheel") > 0) offset.z += zoom;
		else if (Input.GetAxis("Mouse ScrollWheel") < 0) offset.z -= zoom;
		offset.z = Mathf.Clamp(offset.z, -Mathf.Abs(zoomMax), -Mathf.Abs(zoomMin));

		X = transform.localEulerAngles.y + Input.GetAxis("Horizontal") * sensitivity;
		Y += Input.GetAxis("Vertical") * sensitivity;
		Y = Mathf.Clamp(Y, -limit, limit);
		transform.localEulerAngles = new Vector3(-Y, X, 0);
		transform.position = transform.localRotation * offset + target.position;
	}
}