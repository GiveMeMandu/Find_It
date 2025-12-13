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
        [BoxGroup("세부 설정")] [LabelText("태양")]
        [SerializeField] private Transform sun;
        [BoxGroup("세부 설정")] [LabelText("달")]
        [SerializeField] private Transform moon;
        [BoxGroup("세부 설정")] [LabelText("시침 옵젝")]
        [SerializeField] private Transform hourHand;
        [BoxGroup("세부 설정")] [LabelText("시계추 옵젝")]
        [SerializeField] private Transform clockHand;
        [BoxGroup("세부 설정")] [LabelText("분침 옵젝")]
        [SerializeField] private Transform minHand;
        [BoxGroup("효과음")] [LabelText("시계 소리")]
        [SerializeField] private AudioSource audioSource;
        [BoxGroup("효과음")] [LabelText("후쉬소리")]
        [SerializeField] private AudioClip whoosh;
        [BoxGroup("세부 설정")]
        [LabelText("시침 날짜 바뀔 때 돌릴 바퀴 수")]
        [SerializeField] private int hourRotateCount = 1;
        [SerializeField] private Stage2Manager stage2Manager;


        private CancellationTokenSource destroyCancellation = new CancellationTokenSource(); //삭제시 취소처리
        private CancellationTokenSource disableCancellation = new CancellationTokenSource(); //비활성화시 취소처리
        private int soundCount = 0;
        private bool needFastMinRotate = false;
        private bool needFastHourRotate = false;

        private bool foundClcok, foundHourHand, foundMinHand;
        private void OnEnable()
        {
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

        //* 시계침 찾음
        public void FoundClock()
        {
            foundClcok = true;
            clockHand.gameObject.SetActive(true);
            StartClockHandRotate().Forget();
            CheckDayChange();
        }
        //* 초침 찾음
        public void FoundHourHand()
        {
            foundHourHand = true;
            hourHand.gameObject.SetActive(true);
            CheckDayChange();
        }
        //* 분침 찾음
        public void FoundMinHand()
        {
            foundMinHand = true;
            minHand.gameObject.SetActive(true);
            CheckDayChange();
        }

        private void CheckDayChange()
        {
            // if (foundClcok && foundHourHand && foundMinHand)
            // {
                hourHand.gameObject.SetActive(true);
                audioSource.Play();
                needFastHourRotate = true;
                needFastMinRotate = true;
                StartClockSound().Forget();
                UniTask.Void(async () => { 
                    await UniTask.WaitForSeconds(1f);
                    // stage2Manager.StartNightStage();
                });
                UniTask.Void(async () => { 
                    audioSource.PlayOneShot(whoosh);
                    await sun.DOLocalMoveY(5f, 1f).SetEase(Ease.InOutBack);
                    await UniTask.WaitForSeconds(0.1f);
                    audioSource.PlayOneShot(whoosh);
                    moon.gameObject.SetActive(true);
                    await moon.DOLocalMoveY(-1.46f, 0f);
                    await moon.DOLocalMoveY(8.885f, 1f).SetEase(Ease.InOutBack);
                });

            // }
        }
        private void OnStartStage(object sender, EventArgs e)
        {
            CheckDayChange();
        }
        private async UniTaskVoid StartClockSound()
        {
            while (soundCount <= 2)
            {
                await UniTask.WaitForSeconds(0.99f, cancellationToken: disableCancellation.Token);
                if (audioSource.isPlaying) soundCount++;
                if (soundCount > 2)
                {
                    audioSource.Stop();
                    needFastMinRotate = false;
                    needFastHourRotate = false;
                }
            }
        }

        private async UniTaskVoid StartClockHandRotate()
        {
            var dir = new Vector3(0,0,10);
            while(gameObject.activeSelf)
            {
                await clockHand.DORotate(dir, 3).WithCancellation(destroyCancellationToken);
                await clockHand.DORotate(dir * -1, 3).WithCancellation(destroyCancellationToken);
            }
        }

        private async UniTaskVoid StartClockHandMove()
        {
            float rotateValue = -10f;
            float minRotateValueAdd = 0f;
            float hourRotateValueAdd = 0f;

            while (true)
            {
                UniTask minTask = UniTask.WaitForSeconds(0);
                UniTask hourTask = UniTask.WaitForSeconds(0);
                if(minHand == null) return;
                if (minHand.gameObject.activeSelf)
                {
                    minRotateValueAdd += rotateValue;
                    if (Mathf.Abs(minRotateValueAdd) >= 360f) minRotateValueAdd = 0f;
                    var minRotate = Quaternion.AngleAxis(minRotateValueAdd, Vector3.forward);
                    if (needFastMinRotate)
                    {
                        minRotateValueAdd += -85;
                        minRotate = Quaternion.AngleAxis(minRotateValueAdd, Vector3.forward);
                    }
                    minTask =
                        minHand.DORotateQuaternion(minRotate, 0.5f)
                                 .SetEase(Ease.Linear)
                                 .WithCancellation(disableCancellation.Token);
                }
                if (hourHand.gameObject.activeSelf)
                {
                    hourRotateValueAdd += rotateValue * 0.25f;
                    if (Mathf.Abs(hourRotateValueAdd) >= 360f) hourRotateValueAdd = 0f;
                    var hourRotate = Quaternion.AngleAxis(hourRotateValueAdd, Vector3.forward);
                    if (needFastHourRotate)
                    {
                        hourRotateValueAdd += -65;
                        hourRotate = Quaternion.AngleAxis(hourRotateValueAdd, Vector3.forward);
                    }
                    hourTask =
                        hourHand.DORotateQuaternion(hourRotate, 0.5f)
                                .SetEase(Ease.Linear)
                                .WithCancellation(disableCancellation.Token);
                }
                await UniTask.WhenAll(minTask, hourTask);
            }
        }

    }
}