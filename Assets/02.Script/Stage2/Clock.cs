using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InGame
{
    public class Clock : MonoBehaviour
    {
        [BoxGroup("세부 설정")] [LabelText("시침 옵젝")]
        [SerializeField] private Transform hourHand;
        [BoxGroup("세부 설정")] [LabelText("분침 옵젝")]
        [SerializeField] private Transform minHand;
        [BoxGroup("세부 설정")] [LabelText("시계 소리")]
        [SerializeField] private AudioSource audioSource;
        [BoxGroup("세부 설정")] [LabelText("시침 날짜 바뀔 때 돌릴 바퀴 수")]
        [SerializeField] private int hourRotateCount = 1;
        [SerializeField] private Stage2Manager stage2Manager;


        private CancellationTokenSource destroyCancellation = new CancellationTokenSource(); //삭제시 취소처리
        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리
        private int soundCount = 0;
        private bool needFastMinRotate = false;
        private bool needFastHourRotate = false;
        private void OnEnable() {
            stage2Manager.OnStartStage += OnStartStage; 
            StartClockHandMove().Forget();
            if (destroyCancellation != null)
            {
                destroyCancellation.Dispose();
            }
            destroyCancellation = new CancellationTokenSource();
        }
        private void OnDisable()
        {
            stage2Manager.OnStartStage -= OnStartStage; 
            if (!disableCancellation.IsCancellationRequested)
            {
                disableCancellation.Cancel();
            }
        }
        private void OnDestroy() { destroyCancellation.Cancel(); destroyCancellation.Dispose(); }

        private void OnStartStage(object sender, EventArgs e)
        {
            hourHand.gameObject.SetActive(true);
            audioSource.Play();
            needFastHourRotate = true;
            needFastMinRotate = true;
        }

        private async UniTaskVoid StartClockHandMove()
        {
            float rotateValue = -10f;
            float minRotateValueAdd = 0f;
            float hourRotateValueAdd = 0f;

            while (true)
            {
                minRotateValueAdd += rotateValue;
                hourRotateValueAdd += rotateValue * 0.25f;

                if (Mathf.Abs(minRotateValueAdd) >= 360f) minRotateValueAdd = 0f;
                if(audioSource.isPlaying) soundCount++; 
                if(soundCount > 4)
                {
                    rotateValue = -10;
                    audioSource.Stop();
                    needFastMinRotate = false;
                    needFastHourRotate = false;
                }
                
                var minRotate = Quaternion.AngleAxis(minRotateValueAdd, Vector3.forward);
                if(needFastMinRotate)
                {
                    minRotateValueAdd += -85;
                    minRotate = Quaternion.AngleAxis(minRotateValueAdd, Vector3.forward);
                }
                var hourRotate = Quaternion.AngleAxis(hourRotateValueAdd, Vector3.forward);
                if (needFastHourRotate)
                {
                    hourRotateValueAdd += -65;
                    hourRotate = Quaternion.AngleAxis(hourRotateValueAdd, Vector3.forward);
                }
                
                var minTask = 
                    minHand.DORotateQuaternion(minRotate, 0.5f)
                             .SetEase(Ease.Linear)
                             .WithCancellation(disableCancellation.Token);
                var hourTask = 
                    hourHand.DORotateQuaternion(hourRotate, 0.5f)
                             .SetEase(Ease.Linear)
                             .WithCancellation(disableCancellation.Token);
                
                await UniTask.WhenAll(minTask, hourTask);
            }
        }

    }
}