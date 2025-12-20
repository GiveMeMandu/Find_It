using InGame;
using NaughtyAttributes;
using UnityEngine;

public class ClockChangeDayObject : ChangeDayObject
{
    public enum ClockChangeDayObjectType
    {
        Clock,
        HourHand,
        MinHand
    }
    [Label("이게 어떤 부품인지 알려주셈")]
    public ClockChangeDayObjectType clockChangeDayObjectType;
    private Clock clock;
    void Start()
    {
        clock = FindFirstObjectByType<Clock>();
    }
    public override void Found()
    {
        base.Found();
        if (clock != null)
        {
            switch (clockChangeDayObjectType)
            {
                case ClockChangeDayObjectType.Clock:
                    clock.FoundClock();
                    break;
                case ClockChangeDayObjectType.HourHand:
                    clock.FoundHourHand();
                    break;
                case ClockChangeDayObjectType.MinHand:
                    clock.FoundMinHand();
                    break;
            }            
        }
    }

}
