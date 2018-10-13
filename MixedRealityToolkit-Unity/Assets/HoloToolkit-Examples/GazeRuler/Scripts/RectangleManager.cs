// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using HoloToolkit.Unity;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using System.IO;
using System.Net;


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
            var point = (GameObject)Instantiate(PointPrefab, hitPoint, Quaternion.identity);
            var newPoint = new Point
            {
                Position = hitPoint,
                Root = point
            };
            if (CurrentRectangle.IsFinished)
            {
                CurrentRectangle = new Rectangle()
                {
                    IsFinished = false,
                    Root = new GameObject(),
                    Points = new List<Vector3>()
                };

                CurrentRectangle.Points.Add(newPoint.Position);
                newPoint.Root.transform.parent = CurrentRectangle.Root.transform;
            }
            else
            {
                CurrentRectangle.Points.Add(newPoint.Position);
                newPoint.Root.transform.parent = CurrentRectangle.Root.transform;
                if (CurrentRectangle.Points.Count == 2) //First side
                {
                    var index = CurrentRectangle.Points.Count - 1;
                    var centerPos = (CurrentRectangle.Points[index] + CurrentRectangle.Points[index - 1]) * 0.5f;
                    var direction = CurrentRectangle.Points[index] - CurrentRectangle.Points[index - 1];
                    var distance = Vector3.Distance(CurrentRectangle.Points[index], CurrentRectangle.Points[index - 1]);
                    var line = (GameObject)Instantiate(LinePrefab, centerPos, Quaternion.LookRotation(direction));
                    line.transform.localScale = new Vector3(distance, 0.005f, 0.005f);
                    line.transform.Rotate(Vector3.down, 90f);
                    line.transform.parent = CurrentRectangle.Root.transform;
                    CurrentRectangle.width = distance;
                }
                if (CurrentRectangle.Points.Count == 3) //Finishen
                {
                    var index = CurrentRectangle.Points.Count - 1;
                    var centerPos = (CurrentRectangle.Points[index] + CurrentRectangle.Points[index - 1]) * 0.5f;
                    var direction = CurrentRectangle.Points[index] - CurrentRectangle.Points[index - 1];
                    var distance = Vector3.Distance(CurrentRectangle.Points[index], CurrentRectangle.Points[index - 1]);
                    var line = (GameObject)Instantiate(LinePrefab, centerPos, Quaternion.LookRotation(direction));
                    line.transform.localScale = new Vector3(distance, 0.005f, 0.005f);
                    line.transform.Rotate(Vector3.down, 90f);
                    line.transform.parent = CurrentRectangle.Root.transform;
                    CurrentRectangle.length = distance;
                    Debug.Log(CurrentRectangle.length);
                    Debug.Log(CurrentRectangle.width);
                    UploadMeasurement(CurrentRectangle.length, CurrentRectangle.width, 0);
                    CurrentRectangle.IsFinished = true;
                }

            }

        }

        /// <summary>
        /// finish current geometry
        /// </summary>
        /// <param name="LinePrefab"></param>
        /// <param name="TextPrefab"></param>
        public void CloseRectangle(GameObject LinePrefab, GameObject TextPrefab)
        {
            if (CurrentRectangle != null)
            {
                CurrentRectangle.IsFinished = true;
                //var width = CalculateRectangleWidth(CurrentRectangle);
                var area = CalculateTriangleArea(CurrentRectangle.Points[1], CurrentRectangle.Points[1], CurrentRectangle.Points[1]);
                var index = CurrentRectangle.Points.Count - 1;
                var centerPos = (CurrentRectangle.Points[index] + CurrentRectangle.Points[0]) * 0.5f;
                var direction = CurrentRectangle.Points[index] - CurrentRectangle.Points[0];
                var distance = Vector3.Distance(CurrentRectangle.Points[index], CurrentRectangle.Points[0]);
                var line = (GameObject)Instantiate(LinePrefab, centerPos, Quaternion.LookRotation(direction));
                line.transform.localScale = new Vector3(distance, 0.005f, 0.005f);
                line.transform.Rotate(Vector3.down, 90f);
                line.transform.parent = CurrentRectangle.Root.transform;

                var vect = new Vector3(0, 0, 0);
                foreach (var point in CurrentRectangle.Points)
                {
                    vect += point;
                }
                var centerPoint = vect / (index + 1);
                var direction1 = CurrentRectangle.Points[1] - CurrentRectangle.Points[0];
                var directionF = Vector3.Cross(direction, direction1);
                var tip = (GameObject)Instantiate(TextPrefab, centerPoint, Quaternion.LookRotation(directionF));//anchor.x + anchor.y + anchor.z < 0 ? -1 * anchor : anchor));

                // unit is ㎡
                tip.GetComponent<TextMesh>().text = area + "㎡";
                tip.transform.parent = CurrentRectangle.Root.transform;
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

        //send Data to NAV
        public void UploadMeasurement(double Length, double Width, double Height)
        {
            Debug.Log("upload Data");
            const string URL = "http://measureit.westeurope.cloudapp.azure.com:7048/NAV/ODataV4/Company%28%27CRONUS%20International%20Ltd.%27%29/Measurement";
            const string CONTENT_TYPE = "application/json";
            const string USER = "MeasureIT";
            const string PW = "MeasureIT123";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
            httpWebRequest.ContentType = CONTENT_TYPE;
            httpWebRequest.Method = "POST";
            httpWebRequest.Credentials = new NetworkCredential(USER, PW);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"Length\": " + Length.ToString() + ","
                    + "\"Width\": " + Width.ToString() + ","
                    + "\"Height\": " + Height.ToString()
                    + "}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
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

        /// <summary>
        /// Calculate an area of triangle
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        private float CalculateTriangleArea(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var a = Vector3.Distance(p1, p2);
            var b = Vector3.Distance(p1, p3);
            var c = Vector3.Distance(p3, p2);
            var p = (a + b + c) / 2f;
            var s = Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));

            return s;
        }
        /// <summary>
        /// Calculate an area of geometry
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private float CalculateRectangleArea(Rectangle rectangle)
        {
            var s = 0.0f;
            var i = 1;
            var n = rectangle.Points.Count;
            for (; i < n - 1; i++)
                s += CalculateTriangleArea(rectangle.Points[0], rectangle.Points[i], rectangle.Points[i + 1]);
            return 0.5f * Mathf.Abs(s);
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