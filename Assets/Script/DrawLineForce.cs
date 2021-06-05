using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DrawLineForce : MonoBehaviour
{
    public GameObject viewForcePoint;
    public CelestialBody mainPlanet;

    private float radiusObject;
    private float RadiosObject;
    private List<GameObject> forcePoints;

    RaycastHit hit;

    void Start()
    {
        forcePoints = new List<GameObject>();
    }
    void FixedUpdate()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000000))
            {
                if (mainPlanet == null || mainPlanet.name != hit.collider.name)
                {
                    mainPlanet = hit.collider.GetComponent<CelestialBody>();
                    foreach (var body in NBodySimulation.instance.bodies)
                    {
                        if (body.name == name)
                        {
                            mainPlanet = body;
                            break;
                        }
                    }
                    forcePoints.Clear();
                }
                forcePoints.Add(Instantiate(viewForcePoint, hit.point, Quaternion.identity, mainPlanet.transform));
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            forcePoints.Clear();
        }
    }
    void OnDrawGizmos()
    {
        if (forcePoints != null && forcePoints.Count != 0)
            for (int i = 0; i < forcePoints.Count; i++)
            {
                Gizmos.DrawLine(forcePoints[i].transform.position, CalculateTidalPower(forcePoints[i]));

            }
    }

    public Vector3 CalculateTidalPower(GameObject point)
    {
        List<Vector3> forceBodies = new List<Vector3>();
        foreach (var body in NBodySimulation.instance.bodies)
        {
            if (body.name != mainPlanet.name && body.name != "Sun")
            {

                float acceleration;
                float dst = Vector3.Distance(body.Position, mainPlanet.Position);
                Vector3 pointToPlanet = point.transform.position - mainPlanet.Position;
                Vector3 horDir = (body.Position - mainPlanet.Position).normalized;
                Vector3 vertDir = (point.transform.position.y < 0)? Vector3.down : Vector3.up;
                acceleration = Universe.gravitationalConstant * body.mass * mainPlanet.mass / Mathf.Pow(dst, 2);
                Vector3 forceVert = vertDir * 1.5f * acceleration * (mainPlanet.radius / dst)
                           * Mathf.Cos(2f * CalculateAngle(mainPlanet.Position - body.Position, pointToPlanet));
                Vector3 forceHor = horDir * 1.5f * acceleration * (mainPlanet.radius / dst)
                           * Mathf.Sin(2f * CalculateAngle(horDir, pointToPlanet));
                Debug.Log("”гол:" + CalculateAngle(horDir, pointToPlanet));
                forceBodies.Add(forceVert + forceHor);
            }
        }

        Vector3 sumForce = Vector3.zero;

        for (int i = 0; i < forceBodies.Count; i++)
        {
            sumForce += forceBodies[i];
        }

        return sumForce;

    }

    public float CalculateAngle(Vector3 from, Vector3 to)
    {

        var scalarMultiplier = Vector3.Dot(from, to);
        var cos = scalarMultiplier / (from.magnitude * to.magnitude);
        float angle = Mathf.Acos(cos);

        return angle;
    }

}
