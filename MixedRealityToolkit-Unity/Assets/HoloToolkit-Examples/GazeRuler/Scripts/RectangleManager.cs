// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using HoloToolkit.Unity;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;

namespace HoloToolkit.Examples.GazeRuler
{
    /// <summary>
    /// manager all rectangles in the scene
    /// </summary>
    public class RectangleManager : Singleton<RectangleManager>, IGeometry, IRectangleClosable
    {
        // save all geometries
        public Stack<Rectangle> Rectangles = new Stack<Rectangle>();
        public Rectangle CurrentRectangle;

        /// <summary>
        ///  handle new point users place
        /// </summary>
        /// <param name="LinePrefab"></param>
        /// <param name="PointPrefab"></param>
        /// <param name="TextPrefab"></param>
        public void AddPoint(GameObject LinePrefab, GameObject PointPrefab, GameObject TextPrefab)
        {
            var hitPoint = GazeManager.Instance.HitPosition;
           
            if (CurrentRectangle.IsFinished)
            {
                CurrentRectangle = new Rectangle()
                {
                    IsFinished = false,
                    Root = new GameObject(),
                    Points = new List<Vector3>()
                };

                return;
            }
            else
            {
                if (CurrentRectangle.Points.Count == 0) //First Point
                {
                    var point = (GameObject)Instantiate(PointPrefab, hitPoint, Quaternion.identity);
                    var newPoint = new Point
                    {
                        Position = new Vector3 { x = hitPoint.x, y = hitPoint.y, z = hitPoint.z },
                        Root = point
                    };
                    CurrentRectangle.Points.Add(newPoint.Position);
                    newPoint.Root.transform.parent = CurrentRectangle.Root.transform;
                    return;
                
                }
                if (CurrentRectangle.Points.Count == 1) //Second Point
                {
                    Vector3 modPos = new Vector3 { x = hitPoint.x, z = hitPoint.z, y = CurrentRectangle.Points[0].y };
                    var point = (GameObject)Instantiate(PointPrefab, modPos, Quaternion.identity);

                    var newPoint = new Point
                    {
                        Position = modPos,
                        Root = point
                    };
                    CurrentRectangle.Points.Add(newPoint.Position);
                    newPoint.Root.transform.parent = CurrentRectangle.Root.transform;

                    var index = CurrentRectangle.Points.Count - 1;
                    var centerPos = (CurrentRectangle.Points[index] + CurrentRectangle.Points[index - 1]) * 0.5f;
                    var direction = CurrentRectangle.Points[index] - CurrentRectangle.Points[index - 1];
                    var firstDistance = Vector3.Distance(CurrentRectangle.Points[index], CurrentRectangle.Points[index - 1]);
                    var line = (GameObject)Instantiate(LinePrefab, centerPos, Quaternion.LookRotation(direction));
                    line.transform.localScale = new Vector3(firstDistance, 0.005f, 0.005f);
                    line.transform.Rotate(Vector3.down, 90f);
                    line.transform.parent = CurrentRectangle.Root.transform;
                    return;
                }
                if (CurrentRectangle.Points.Count == 2) //Last Point
                {
                    Vector3 test = new Vector3 { x = hitPoint.x, y = CurrentRectangle.Points[0].y, z = hitPoint.z };

                    var BA = CurrentRectangle.Points[0] - CurrentRectangle.Points[1];
                    var BC = test - CurrentRectangle.Points[1];

                    var beta = System.Math.Round(Vector3.Angle(BA, BC));
                    bool clockwise = true;

                    while (!(beta == 90.0f))
                    {
                        if ((beta >= 0 && beta < 90) && clockwise)
                            test = test.RotateAround(CurrentRectangle.Points[1], Vector3.up);
                        if ((beta >= 0 && beta < 90) && !clockwise)
                            test = test.RotateAround(CurrentRectangle.Points[1], Vector3.up * -1);
                        if (!(beta >= 0 && beta < 90) && clockwise)
                            test = test.RotateAround(CurrentRectangle.Points[1], Vector3.up * -1);
                        else
                            test = test.RotateAround(CurrentRectangle.Points[1], Vector3.up);

                        BC = test - CurrentRectangle.Points[1];
                        beta = System.Math.Round(Vector3.Angle(BA, BC));
                    }

                    var point = (GameObject)Instantiate(PointPrefab, test, Quaternion.identity);

                    var newPoint = new Point
                    {
                        Position = test,
                        Root = point
                    };
                    CurrentRectangle.Points.Add(newPoint.Position);
                    newPoint.Root.transform.parent = CurrentRectangle.Root.transform;

                    var index = CurrentRectangle.Points.Count - 1;
                    var centerPos = (CurrentRectangle.Points[index] + CurrentRectangle.Points[index - 1]) * 0.5f;
                    var direction = CurrentRectangle.Points[index] - CurrentRectangle.Points[index - 1];
                    var lastDistance = Vector3.Distance(CurrentRectangle.Points[index], CurrentRectangle.Points[index - 1]);
                    var line = (GameObject)Instantiate(LinePrefab, centerPos, Quaternion.LookRotation(direction));
                    line.transform.localScale = new Vector3(lastDistance, 0.005f, 0.005f);
                    line.transform.Rotate(Vector3.down, 90f);
                    line.transform.parent = CurrentRectangle.Root.transform;
                    return;
                }

            }

        }

        /// <summary>
        /// finish current rectangle
        /// </summary>
        /// <param name="LinePrefab"></param>
        /// <param name="TextPrefab"></param>
        public void CloseRectangle(GameObject LinePrefab, GameObject PointPrefab, GameObject TextPrefab)
        {
            if (CurrentRectangle != null)
            {
                CurrentRectangle.IsFinished = true;

                var A = CurrentRectangle.Points[0];
                var B = CurrentRectangle.Points[1];
                var C = CurrentRectangle.Points[2];

                var BA = A - B;

                Vector3 modPos = C + BA;

                var point = (GameObject)Instantiate(PointPrefab, modPos, Quaternion.identity);
                var newPoint = new Point
                {
                    Position = modPos,
                    Root = point
                };
                CurrentRectangle.Points.Add(newPoint.Position);
                newPoint.Root.transform.parent = CurrentRectangle.Root.transform;

                var centerPos = (CurrentRectangle.Points[3] + CurrentRectangle.Points[2]) * 0.5f;
                var direction = CurrentRectangle.Points[3] - CurrentRectangle.Points[2];
                var distance = Vector3.Distance(CurrentRectangle.Points[3], CurrentRectangle.Points[2]);
                var line = (GameObject)Instantiate(LinePrefab, centerPos, Quaternion.LookRotation(direction));
                line.transform.localScale = new Vector3(distance, 0.005f, 0.005f);
                line.transform.Rotate(Vector3.down, 90f);
                line.transform.parent = CurrentRectangle.Root.transform;

                var index = CurrentRectangle.Points.Count - 1;
                var lastCenterPos = (CurrentRectangle.Points[index] + CurrentRectangle.Points[0]) * 0.5f;
                var lastDirection = CurrentRectangle.Points[index] - CurrentRectangle.Points[0];
                var lastDistance = Vector3.Distance(CurrentRectangle.Points[index], CurrentRectangle.Points[0]);
                var lastLine = (GameObject)Instantiate(LinePrefab, lastCenterPos, Quaternion.LookRotation(lastDirection));
                lastLine.transform.localScale = new Vector3(lastDistance, 0.005f, 0.005f);
                lastLine.transform.Rotate(Vector3.down, 90f);
                lastLine.transform.parent = CurrentRectangle.Root.transform;

                Vector3 cameraPosition = CameraCache.Main.transform.position;

                for (int i = 0; i < CurrentRectangle.Points.Count; i++)
                {
                    Vector3 tipCenterPos = (CurrentRectangle.Points[i % 4] + CurrentRectangle.Points[((i - 1) % 4 + 4) % 4]) * 0.5f;
                    Vector3 directionFromCamera = tipCenterPos - cameraPosition;
                    Vector3 normalV = Vector3.Cross(direction, directionFromCamera);
                    Vector3 normalF = Vector3.Cross(direction, normalV) * -1;
                    GameObject tip = (GameObject)Instantiate(TextPrefab, tipCenterPos, Quaternion.LookRotation(normalF));
                    var dist = Vector3.Distance(CurrentRectangle.Points[i % 4], CurrentRectangle.Points[((i - 1) % 4 + 4) % 4]);

                    tip.transform.Translate(Vector3.up * 0.05f);
                    tip.GetComponent<TextMesh>().text = System.Math.Round(dist, 2) + "m";
                }

                Rectangles.Push(CurrentRectangle);
            }
        }

        /// <summary>
        /// clear all geometries in the scene
        /// </summary>
        public void Clear()
        {
            if (Rectangles != null && Rectangles.Count > 0)
            {
                while (Rectangles.Count > 0)
                {
                    var lastLine = Rectangles.Pop();
                    Destroy(lastLine.Root);
                }
            }
        }

        // delete latest geometry
        public void Delete()
        {
            if (Rectangles != null && Rectangles.Count > 0)
            {
                var lastLine = Rectangles.Pop();
                Destroy(lastLine.Root);
            }
        }
        // Use this for initialization
        private void Start()
        {
            CurrentRectangle = new Rectangle()
            {
                IsFinished = false,
                Root = new GameObject(),
                Points = new List<Vector3>()
            };
        }


        /// <summary>
        /// reset current unfinished geometry
        /// </summary>
        public void Reset()
        {
            if (CurrentRectangle != null && !CurrentRectangle.IsFinished)
            {
                Destroy(CurrentRectangle.Root);
                CurrentRectangle = new Rectangle()
                {
                    IsFinished = false,
                    Root = new GameObject(),
                    Points = new List<Vector3>()
                };
            }
        }
    }


    public class Rectangle
    {
        public float width { get; set; }
        public float length { get; set; }

        public List<Vector3> Points { get; set; }

        public GameObject Root { get; set; }

        public bool IsFinished { get; set; }

    }
}