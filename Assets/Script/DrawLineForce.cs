using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class DrawLineForce : MonoBehaviour
{
    public GameObject viewForcePoint;
    public CelestialBody mainPlanet;
    public Dropdown dropdown;
    private bool isForceObjects = true;

    private List<GameObject> forcePoints;

    RaycastHit hit;

    void Start()
    {
        forcePoints = new List<GameObject>();
        dropdown.onValueChanged.AddListener((number) =>
        {
            if (number == 0)
            {
                isForceObjects = true;
            }
            else if (number == 1)
            {
                isForceObjects = false;
            }
        });
    }
    void FixedUpdate()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, int.MaxValue))
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
            if (isForceObjects)
                for (int i = 0; i < forcePoints.Count; i++)
                {
                    Vector3 dir = VectorReduction(CalculateTidalPower(forcePoints[i])[0], forcePoints[i].transform.position, mainPlanet.sizeVectorForce);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(forcePoints[i].transform.position, dir);
                    for (int j = 1; j < NBodySimulation.instance.bodies.Length; j++)
                    {
                        dir = VectorReduction(CalculateTidalPower(forcePoints[i])[j], forcePoints[i].transform.position, 15);
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(forcePoints[i].transform.position, dir);
                    }
                }
            else
                for (int i = 0; i < forcePoints.Count; i++)
                {
                    Vector3 dir = VectorReduction(CalculateTidalPower(forcePoints[i])[0], forcePoints[i].transform.position, mainPlanet.sizeVectorForce);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(forcePoints[i].transform.position, dir);
                    dir = VectorReduction(CalculateTidalPower(forcePoints[i])[1], forcePoints[i].transform.position, mainPlanet.sizeVectorForce);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(forcePoints[i].transform.position, dir);
                    dir = VectorReduction(CalculateTidalPower(forcePoints[i])[2], forcePoints[i].transform.position, mainPlanet.sizeVectorForce);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(forcePoints[i].transform.position, dir);
                }
    }

    Vector3 VectorReduction(Vector3 dir, Vector3 point, int size)
    {
        Vector3 vector = dir;
        for (int i = 0; i < size; i++)
        {
            vector = (vector + point) / 2;
        }
        return vector;
    }

    public List<Vector3> CalculateTidalPower(GameObject point)
    {
        List<Vector3> forceBodies = new List<Vector3>();
        List<Vector3> horForces = new List<Vector3>();
        List<Vector3> vertForces = new List<Vector3>();
        foreach (var body in NBodySimulation.instance.bodies)
        {
            if (body.name != mainPlanet.name)
            {

                float acceleration;
                float dst = Vector3.Distance(body.Position, mainPlanet.Position);
                Vector3 pointToPlanet = point.transform.position - mainPlanet.Position;
                Vector3 horDir = (body.Position - mainPlanet.Position).normalized;
                Vector3 vertDir = (point.transform.position.y < 0) ? Vector3.down : Vector3.up;
                acceleration = Universe.gravitationalConstant * body.mass * mainPlanet.mass / Mathf.Pow(dst, 2);
                Vector3 forceVert = vertDir * 1.5f * acceleration * (mainPlanet.radius / dst)
                           * Mathf.Cos(2f * CalculateAngle(mainPlanet.Position - body.Position, pointToPlanet));
                Vector3 forceHor = horDir * 1.5f * acceleration * (mainPlanet.radius / dst)
                           * Mathf.Sin(2f * CalculateAngle(horDir, pointToPlanet));
                //Debug.Log("”гол:" + CalculateAngle(horDir, pointToPlanet));
                forceBodies.Add(forceVert + forceHor);
                horForces.Add(forceHor);
                vertForces.Add(forceVert);
            }
        }

        Vector3 sumForce = Vector3.zero;
        Vector3 horForce = Vector3.zero;
        Vector3 vertForce = Vector3.zero;


        for (int i = 0; i < forceBodies.Count; i++)
        {
            sumForce += forceBodies[i];
            horForce += horForces[i];
            vertForce += vertForces[i];
        }

        List<Vector3> force = new List<Vector3>();

        force.Add(sumForce);
        if (isForceObjects)
        {
            for (int i = 0; i < forceBodies.Count; i++)
            {
                force.Add(forceBodies[i]);
            }
        }
        else
        {
            force.Add(horForce);
            force.Add(vertForce);
        }

        return force;

    }

    public float CalculateAngle(Vector3 from, Vector3 to)
    {

        var scalarMultiplier = Vector3.Dot(from, to);
        var cos = scalarMultiplier / (from.magnitude * to.magnitude);
        float angle = Mathf.Acos(cos);

        return angle;
    }

}
