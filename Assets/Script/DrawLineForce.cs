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
    public float scale = 1;
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
                        dir = VectorReduction(CalculateTidalPower(forcePoints[i])[j], forcePoints[i].transform.position, mainPlanet.sizeVectorForce);
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

                float Fgrav;       
                //distance
                float dst = Vector3.Distance(body.Position, mainPlanet.Position);
                //Vector to point on planet
                Vector3 pointToPlanet = point.transform.position - mainPlanet.Position;
                Vector3 NpointToPlanet;
                Vector3 Ndistance;
                float pointDist = pointToPlanet.magnitude;
                //NpointToPlanet = new Vector3(pointToPlanet.x, 0, pointToPlanet.z);
                //Point to planet X-Z projection
                NpointToPlanet = Vector3.ProjectOnPlane(pointToPlanet, new Vector3(0.0f, 1.0f, 0.0f));
                //Ndistance = body.Position - mainPlanet.Position;

                Ndistance = Vector3.ProjectOnPlane((body.Position - mainPlanet.Position),new Vector3(0.0f, 1.0f, 0.0f));

                float alpha = Vector3.Angle(pointToPlanet,NpointToPlanet);
                float doublealpha = 2.0f * alpha;
                alpha = alpha * Mathf.PI / 180;
                Fgrav = Universe.gravitationalConstant * (body.mass * mainPlanet.mass* pointDist) /(dst* dst* dst);
                
                float HorMag = 2.0f* Fgrav * Mathf.Cos(alpha)*100;
                float VertMag = -1.0f*Fgrav*Mathf.Sin(alpha)*100;
                float Forcemagnitude = Mathf.Sqrt((HorMag/100)* (HorMag / 100) + (VertMag/100)* (VertMag / 100));

                Vector3 horDir = Ndistance.normalized;
                Vector3 vertDir = pointToPlanet.y > 0.0f ? new Vector3(0.0f, 1.0f, 0.0f) : new Vector3(0.0f, -1.0f, 0.0f);

                Vector3 forceVert = vertDir * VertMag;
                Vector3 forceHor = horDir * HorMag;

                Debug.Log("Tidal force magnitude is:" + Forcemagnitude);
                //Debug.Log(alpha + "= Alpha");
                //Debug.Log(Mathf.Cos(doublealpha) + (1 / 3) + " Cos2a+0.3");
                //Debug.Log(Mathf.Sin(alpha) + " Sin a");
                //Debug.Log(forceVert.magnitude+" Force Vert");
                //Debug.Log(forceHor.magnitude+" Force Hor");
                //Debug.Log(Fgrav + " Fgrav");
                //Debug.Log(vertDir + " VertDir");
                //Debug.Log(horDir + " HorDir");
                //Debug.Log(HorMag + " HorMag");
                //Debug.Log(VertMag + " VertMag");

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


}
