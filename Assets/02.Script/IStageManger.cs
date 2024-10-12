using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IStageManager
{
    public void StartStage();
    public void ClearStage();
    public UniTask ClearStageTask();
    
}
