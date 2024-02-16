using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PupilLabs
{
    public class PupilDataStream : MonoBehaviour
    {
        public SubscriptionsController subsCtrl;
        //For the humans we used 3D projection of gaze
        //You can do much more with the gaze listener in theory, but I guess it's fine
        public Vector3 Vector_R;
        public Vector3 Vector_L;
        public float Theta_R;
        public float Theta_L;
        public float Phi_R;
        public float Phi_L;

        public float confidence_R;
        public float confidence_L;

        public Vector2 CenterRightPupilNorm;
        public Vector2 CenterRightPupilPx;
        public Vector2 AxisRightPupilPx;
        public float AngleRightPupil; // degrees
        public float DiameterRight;
        public Vector2 CenterLeftPupilNorm;
        public Vector2 CenterLeftPupilPx;
        public Vector2 AxisLeftPupilPx;
        public float AngleLeftPupil; // degrees
        public float DiameterLeft;
        public double PupilTimeStamps;
        private PupilListener listener;



        void OnEnable()
        {
            if (listener == null)
            {
                listener = new PupilListener(subsCtrl);
            }

            listener.Enable();
            listener.OnReceivePupilData += ReceivePupilData;
        }

        void OnDisable()
        {
            listener.Disable();
            listener.OnReceivePupilData -= ReceivePupilData;
        }


        void Update()
        {

        }

        void ReceivePupilData(PupilData pupilData)
        {
            if (pupilData.EyeIdx == 0)
            {
                Vector_R = pupilData.Circle.Normal;
                Theta_R = pupilData.Circle.Theta * Mathf.Rad2Deg;
                Phi_R = pupilData.Circle.Phi * Mathf.Rad2Deg;

                confidence_R = pupilData.Confidence;

                CenterRightPupilNorm = pupilData.NormPos;
                CenterRightPupilPx = pupilData.Ellipse.Center;
                AxisRightPupilPx = pupilData.Ellipse.Axis;
                AngleRightPupil = pupilData.Ellipse.Angle;
                DiameterRight = pupilData.Diameter3d;


            }
            if (pupilData.EyeIdx == 1)
            {
                Vector_L = pupilData.Circle.Normal;
                Theta_L = pupilData.Circle.Theta * Mathf.Rad2Deg;
                Phi_L = pupilData.Circle.Phi * Mathf.Rad2Deg;

                confidence_L = pupilData.Confidence;

                CenterLeftPupilNorm = pupilData.NormPos;
                CenterLeftPupilPx = pupilData.Ellipse.Center;
                AxisLeftPupilPx = pupilData.Ellipse.Axis;
                AngleLeftPupil = pupilData.Ellipse.Angle;
                DiameterLeft = pupilData.Diameter3d;
            }
            PupilTimeStamps = pupilData.PupilTimestamp;
        }
    }
}